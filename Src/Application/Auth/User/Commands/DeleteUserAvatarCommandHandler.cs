using Application.Common.Interfaces;
using Application.Common.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Auth.User.Commands;

public sealed class DeleteUserAvatarCommandHandler
    : IRequestHandler<DeleteUserAvatarCommand, BaseResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IIdentityService _identityService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<DeleteUserAvatarCommandHandler> _logger;

    public DeleteUserAvatarCommandHandler(
        ICurrentUserService currentUserService,
        IIdentityService identityService,
        IFileStorageService fileStorageService,
        ILogger<DeleteUserAvatarCommandHandler> logger)
    {
        _currentUserService = currentUserService;
        _identityService = identityService;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<BaseResponse> Handle(
        DeleteUserAvatarCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
            return BaseResponse.Fail("User is not authenticated.");

        var avatarUrl = await _identityService.GetAvatarUrlAsync(userId, cancellationToken);

        if (string.IsNullOrWhiteSpace(avatarUrl))
            return BaseResponse.Fail("User does not have an avatar.");

        try
        {
            await _fileStorageService.DeleteFileAsync(avatarUrl, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to delete avatar file from storage. UserId: {UserId}, Avatar: {Avatar}",
                userId,
                avatarUrl);
        }

        var result = await _identityService.UpdateAvatarAsync(
            userId,
            null,
            cancellationToken);

        if (!result.Success)
            return BaseResponse.Fail(result.Message ?? "Failed to update avatar.");

        _logger.LogInformation("Avatar deleted for user {UserId}", userId);

        return BaseResponse.Ok("Avatar deleted successfully.");
    }
}
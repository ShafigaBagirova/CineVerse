using Application.Common.Interfaces;
using Application.Common.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Auth.User.Commands;

public sealed class UploadUserAvatarCommandHandler
    : IRequestHandler<UploadUserAvatarCommand, BaseResponse<string>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IIdentityService _identityService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<UploadUserAvatarCommandHandler> _logger;

    public UploadUserAvatarCommandHandler(
        ICurrentUserService currentUserService,
        IIdentityService identityService,
        IFileStorageService fileStorageService,
        ILogger<UploadUserAvatarCommandHandler> logger)
    {
        _currentUserService = currentUserService;
        _identityService = identityService;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<BaseResponse<string>> Handle(
        UploadUserAvatarCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
            return BaseResponse<string>.Fail("User is not authenticated.");

        var oldAvatarUrl = await _identityService.GetAvatarUrlAsync(userId, cancellationToken);

        await using var stream = request.Request.Avatar.OpenReadStream();

        var avatarPath = await _fileStorageService.SaveAsync(
            stream,
            request.Request.Avatar.FileName,
            request.Request.Avatar.ContentType,
            "avatars",
            cancellationToken);

        var updateResult = await _identityService.UpdateAvatarAsync(
            userId,
            avatarPath,
            cancellationToken);

        if (!updateResult.Success)
            return BaseResponse<string>.Fail(updateResult.Message);

        if (!string.IsNullOrWhiteSpace(oldAvatarUrl))
        {
            try
            {
                await _fileStorageService.DeleteFileAsync(oldAvatarUrl, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Old avatar could not be deleted for user {UserId}. OldAvatar: {OldAvatar}",
                    userId,
                    oldAvatarUrl);
            }
        }

        _logger.LogInformation("Avatar uploaded successfully for user {UserId}", userId);

        return BaseResponse<string>.Ok(avatarPath, "Avatar uploaded successfully.");
    }
}
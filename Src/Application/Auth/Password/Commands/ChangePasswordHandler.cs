using Application.Common.Interfaces;
using Application.Common.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Auth.Password.Commands;

public sealed class ChangePasswordCommandHandler
    : IRequestHandler<ChangePasswordCommand, BaseResponse>
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<ChangePasswordCommandHandler> _logger;

    public ChangePasswordCommandHandler(
        IIdentityService identityService,
        ILogger<ChangePasswordCommandHandler> logger)
    {
        _identityService = identityService;
        _logger = logger;
    }

    public async Task<BaseResponse> Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        _logger.LogInformation(
            "Changing password for UserId: {UserId}",
            request.UserId);

        var response = await _identityService.ChangePasswordAsync(
            request.UserId,
            request.CurrentPassword,
            request.NewPassword);

        if (response.Success)
        {
            _logger.LogInformation(
                "Password changed successfully for UserId: {UserId}",
                request.UserId);
        }
        else
        {
            _logger.LogWarning(
                "Password change failed for UserId: {UserId}. Message: {Message}",
                request.UserId,
                response.Message);
        }

        return response;
    }
}

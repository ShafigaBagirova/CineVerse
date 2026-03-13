using Application.Common.Interfaces;
using Application.Common.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Auth.Password.Commands;

public sealed class ResetPasswordHandler
    : IRequestHandler<ResetPasswordCommand, BaseResponse>
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<ResetPasswordHandler> _logger;

    public ResetPasswordHandler(
        IIdentityService identityService,
        ILogger<ResetPasswordHandler> logger)
    {
        _identityService = identityService;
        _logger = logger;
    }

    public async Task<BaseResponse> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        _logger.LogInformation(
            "Password reset attempt. Email: {Email}, Time: {Time}",
            request.Request.Email,
            DateTime.UtcNow);

        var response = await _identityService.ResetPasswordAsync(request.Request);

        if (response.Success)
        {
            _logger.LogInformation(
                "Password reset successful for Email: {Email}",
                request.Request.Email);
        }
        else
        {
            _logger.LogWarning(
                "Password reset failed for Email: {Email}. Message: {Message}",
                request.Request.Email,
                response.Message);
        }

        return response;
    }
}
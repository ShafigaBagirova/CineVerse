using Application.Common.Interfaces;
using Application.Common.Options;
using Application.Common.Responses;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace Application.Auth.Password.Commands;

public sealed class ForgotPasswordHandler
    : IRequestHandler<ForgotPasswordCommand, BaseResponse>
{
    private readonly IIdentityService _identityService;
    private readonly IEmailSender _emailSender;
    private readonly FrontendOptions _frontend;
    private readonly ILogger<ForgotPasswordHandler> _logger;

    public ForgotPasswordHandler(
        IIdentityService identityService,
        IEmailSender emailSender,
        IOptions<FrontendOptions> frontendOptions,
        ILogger<ForgotPasswordHandler> logger)
    {
        _identityService = identityService;
        _emailSender = emailSender;
        _frontend = frontendOptions.Value;
        _logger = logger;
    }

    public async Task<BaseResponse> Handle(ForgotPasswordCommand request, CancellationToken ct)
    {
        var rq = request.Request;

        _logger.LogInformation("Password reset request received for Email: {Email}", rq.Email);

        if (string.IsNullOrWhiteSpace(rq.Email))
        {
            _logger.LogWarning("Password reset failed. Email is empty.");
            return BaseResponse.Fail("Email is required.");
        }

        var token = await _identityService.GeneratePasswordResetTokenAsync(rq.Email);

        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogInformation(
                "Password reset requested but user not found or token not generated. Email: {Email}",
                rq.Email);

            return BaseResponse.Ok("If the account exists, a reset link has been sent.");
        }

        var encodedToken = WebUtility.UrlEncode(token);

        var resetLink =
            $"{_frontend.BaseUrl}/reset-password?email={Uri.EscapeDataString(rq.Email)}&token={encodedToken}";

        var htmlBody = $"""
            <p>To reset your password, click the link below:</p>
            <p><a href="{resetLink}">Reset Password</a></p>
            """;

        await _emailSender.SendAsync(
            rq.Email,
            "Reset Password",
            htmlBody,
            ct: ct);

        _logger.LogInformation(
            "Password reset email sent successfully. Email: {Email}",
            rq.Email);

        return BaseResponse.Ok("If the account exists, a reset link has been sent.");
    }
}
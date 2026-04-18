using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace Application.Auth.Register.Commands;

public sealed class ResendVerificationCodeCommandHandler
    : IRequestHandler<ResendVerificationCodeCommand, BaseResponse>
{
    private readonly IIdentityService _identityService;
    private readonly IEmailVerificationCodeRepository _codeRepository;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<ResendVerificationCodeCommandHandler> _logger;

    public ResendVerificationCodeCommandHandler(
        IIdentityService identityService,
        IEmailVerificationCodeRepository codeRepository,
        IEmailSender emailSender,
        ILogger<ResendVerificationCodeCommandHandler> logger)
    {
        _identityService = identityService;
        _codeRepository = codeRepository;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<BaseResponse> Handle(ResendVerificationCodeCommand request, CancellationToken ct)
    {
        var emailInput = request.Email.Trim();
        if (string.IsNullOrWhiteSpace(emailInput))
            return BaseResponse.Fail("Email is required.");

        const string GenericOk =
            "If this email is registered and not yet verified, a new code has been sent.";

        var user = await _identityService.GetUserByEmailAsync(emailInput);
        if (user is null)
        {
            _logger.LogInformation("Resend verification: no user for email.");
            return BaseResponse.Ok(GenericOk);
        }

        if (await _identityService.IsEmailConfirmedAsync(user.UserId))
        {
            _logger.LogInformation("Resend verification: email already confirmed. UserId: {UserId}", user.UserId);
            return BaseResponse.Ok(GenericOk);
        }

        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        var codeHash = VerificationCodeHasher.Hash(code);

        var latest = await _codeRepository.GetLatestByEmailAsync(emailInput, ct);

        if (latest is not null)
        {
            latest.UserId = user.UserId;
            latest.Email = user.Email;
            latest.CodeHash = codeHash;
            latest.ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5);
            latest.IsUsed = false;
            await _codeRepository.UpdateAsync(latest, ct);
        }
        else
        {
            var entity = new EmailVerificationCode
            {
                UserId = user.UserId,
                Email = user.Email,
                CodeHash = codeHash,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false,
            };
            await _codeRepository.AddAsync(entity, ct);
        }

        await _codeRepository.SaveChangesAsync(ct);

        var htmlBody = $"""
            <h2>CineVerse Verification Code</h2>
            <p>Your verification code is:</p>
            <h1>{code}</h1>
            <p>This code will expire in 5 minutes.</p>
            """;

        var textBody = $"Your CineVerse verification code is: {code}. This code will expire in 5 minutes.";

        await _emailSender.SendAsync(
            user.Email,
            "CineVerse Verification Code",
            htmlBody,
            textBody,
            ct);

        _logger.LogInformation("Verification code resent. UserId: {UserId}", user.UserId);

        return BaseResponse.Ok("Verification code sent to your email.");
    }
}

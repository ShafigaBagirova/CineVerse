using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Auth.Email.Commands;

public sealed class UpdateEmailCommandHandler
    : IRequestHandler<UpdateEmailCommand, BaseResponse>
{
    private readonly IIdentityService _identityService;
    private readonly IEmailVerificationCodeRepository _codeRepository;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<UpdateEmailCommandHandler> _logger;

    public UpdateEmailCommandHandler(
        IIdentityService identityService,
        IEmailVerificationCodeRepository codeRepository,
        IEmailSender emailSender,
        ILogger<UpdateEmailCommandHandler> logger)
    {
        _identityService = identityService;
        _codeRepository = codeRepository;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<BaseResponse> Handle(UpdateEmailCommand request, CancellationToken ct)
    {
        _logger.LogInformation(
            "Update email request received. UserId: {UserId}, NewEmail: {NewEmail}",
            request.UserId,
            request.NewEmail);

        if (string.IsNullOrWhiteSpace(request.NewEmail))
        {
            _logger.LogWarning(
                "Update email failed. NewEmail is empty. UserId: {UserId}",
                request.UserId);

            return BaseResponse.Fail("New email is required.");
        }

        var existingUser = await _identityService.GetUserByEmailAsync(request.NewEmail);

        if (existingUser is not null)
        {
            _logger.LogWarning(
                "Update email failed. Email already in use. UserId: {UserId}, NewEmail: {NewEmail}",
                request.UserId,
                request.NewEmail);

            return BaseResponse.Fail("This email is already in use.");
        }

        var activeCode = await _codeRepository.GetActiveByEmailAsync(request.NewEmail, ct);

        if (activeCode is not null && !activeCode.IsUsed && activeCode.ExpiresAtUtc > DateTime.UtcNow)
        {
            _logger.LogInformation(
                "Existing verification code invalidated before creating a new one. UserId: {UserId}, NewEmail: {NewEmail}",
                request.UserId,
                request.NewEmail);

            await _codeRepository.MarkAsUsedAsync(activeCode.Id, ct);
        }

        var code = Random.Shared.Next(100000, 999999).ToString();

        var entity = new EmailVerificationCode
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Email = request.NewEmail,
            CodeHash = VerificationCodeHasher.Hash(code),
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _codeRepository.AddAsync(entity, ct);

        _logger.LogInformation(
            "Verification code generated for email update. UserId: {UserId}, NewEmail: {NewEmail}",
            request.UserId,
            request.NewEmail);

        var htmlBody = $"""
            <p>Your email change verification code is:</p>
            <h2>{code}</h2>
            <p>This code will expire in 10 minutes.</p>
            """;

        await _emailSender.SendAsync(
            request.NewEmail,
            "Confirm your new email",
            htmlBody,
            ct: ct);

        _logger.LogInformation(
            "Verification email sent successfully. UserId: {UserId}, NewEmail: {NewEmail}",
            request.UserId,
            request.NewEmail);

        return BaseResponse.Ok("Verification code sent to your new email.");
    }
}
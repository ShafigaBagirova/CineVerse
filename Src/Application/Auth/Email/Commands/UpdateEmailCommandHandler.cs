using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using Domain.Entities;
using MediatR;

namespace Application.Auth.Email.Commands;

public sealed class UpdateEmailCommandHandler
    : IRequestHandler<UpdateEmailCommand, BaseResponse>
{
    private readonly IIdentityService _identityService;
    private readonly IEmailVerificationCodeRepository _codeRepository;
    private readonly IEmailSender _emailSender;

    public UpdateEmailCommandHandler(
        IIdentityService identityService,
        IEmailVerificationCodeRepository codeRepository,
        IEmailSender emailSender)
    {
        _identityService = identityService;
        _codeRepository = codeRepository;
        _emailSender = emailSender;
    }

    public async Task<BaseResponse> Handle(UpdateEmailCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.NewEmail))
            return BaseResponse.Fail("New email is required.");

        var existingUser = await _identityService.GetUserByEmailAsync(request.NewEmail);
        if (existingUser is not null)
            return BaseResponse.Fail("This email is already in use.");

        var activeCode = await _codeRepository.GetActiveByEmailAsync(request.NewEmail, ct);
        if (activeCode is not null && !activeCode.IsUsed && activeCode.ExpiresAtUtc > DateTime.UtcNow)
        {
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

        return BaseResponse.Ok("Verification code sent to your new email.");
    }
}
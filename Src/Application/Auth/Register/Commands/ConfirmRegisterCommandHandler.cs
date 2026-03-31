using Application.Common.Helpers;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace Application.Auth.Register.Commands;

public sealed class ConfirmRegisterCommandHandler(
    IEmailVerificationCodeRepository codeRepository,
    IIdentityService identityService,
    ILogger<ConfirmRegisterCommandHandler> logger)
    : IRequestHandler<ConfirmRegisterCommand, bool>
{
    public async Task<bool> Handle(ConfirmRegisterCommand request, CancellationToken ct)
    {
        
        var email = request.Email.Trim().ToLower();
        
       
        logger.LogInformation("Confirm register attempt for Email: {Email}", request.Email);
        
        var record = await codeRepository.GetActiveByEmailAsync(email, ct);

        if (record is null)
        {
            logger.LogWarning("Email confirmation failed. No verification record found for Email: {Email}", request.Email);
            return false;
        }

        if (record.IsUsed || record.ExpiresAtUtc <= DateTime.UtcNow)
        {
            logger.LogWarning("Email confirmation failed. Code expired or already used. Email: {Email}", request.Email);
            return false;
        }

        if (string.IsNullOrWhiteSpace(record.UserId))
        {
            logger.LogWarning("Email confirmation failed. Missing UserId for Email: {Email}", request.Email);
            return false;
        }

        var code = request.Code.Trim();
        var codeHash = VerificationCodeHasher.Hash(code);

        if (!string.Equals(record.CodeHash, codeHash, StringComparison.Ordinal))
        {
            logger.LogWarning("Email confirmation failed. Invalid verification code for Email: {Email}", request.Email);
            return false;
        }

        var confirmed = await identityService.ConfirmEmailAsync(record.UserId);

        if (!confirmed)
        {
            logger.LogError("Email confirmation failed during identity service confirmation. UserId: {UserId}", record.UserId);
            return false;
        }

        await codeRepository.MarkAsUsedAsync(record.Id, ct);

        logger.LogInformation("Email confirmed successfully. UserId: {UserId}, Email: {Email}", record.UserId, request.Email);

        return true;
    }

}
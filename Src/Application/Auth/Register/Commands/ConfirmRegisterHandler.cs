using Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace Application.Auth.Register.Commands;

public sealed class ConfirmRegisterHandler(
    IEmailVerificationCodeRepository codeRepository,
    IIdentityService identityService,
    ILogger<ConfirmRegisterHandler> logger)
    : IRequestHandler<ConfirmRegisterCommand, bool>
{
    public async Task<bool> Handle(ConfirmRegisterCommand request, CancellationToken ct)
    {
        logger.LogInformation("Confirm register attempt for Email: {Email}", request.Email);

        var record = await codeRepository.GetActiveByEmailAsync(request.Email, ct);

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

        var codeHash = ComputeSha256(request.Code);

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

    private static string ComputeSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}
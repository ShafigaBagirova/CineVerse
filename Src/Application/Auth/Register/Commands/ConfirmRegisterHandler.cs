using Application.Common.Interfaces;
using MediatR;
using System.Security.Cryptography;
using System.Text;

namespace Application.Auth.Register.Commands;

public sealed class ConfirmRegisterHandler(
    IEmailVerificationCodeRepository codeRepository,
    IIdentityService identityService)
    : IRequestHandler<ConfirmRegisterCommand, bool>
{
    public async Task<bool> Handle(ConfirmRegisterCommand request, CancellationToken ct)
    {
        var record = await codeRepository.GetActiveByEmailAsync(request.Email, ct);

        if (record is null)
            return false;

        if (record.IsUsed || record.ExpiresAtUtc <= DateTime.UtcNow)
            return false;

        if (string.IsNullOrWhiteSpace(record.UserId))
            return false;

        var codeHash = ComputeSha256(request.Code);

        if (!string.Equals(record.CodeHash, codeHash, StringComparison.Ordinal))
            return false;

        var confirmed = await identityService.ConfirmEmailAsync(record.UserId);

        if (!confirmed)
            return false;

        await codeRepository.MarkAsUsedAsync(record.Id, ct);

        return true;
    }

    private static string ComputeSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}
using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IEmailVerificationCodeRepository
{
    Task AddAsync(EmailVerificationCode entity, CancellationToken ct);
    Task<EmailVerificationCode?> GetActiveByEmailAsync(string email, CancellationToken ct);
    Task<bool> MarkAsUsedAsync(Guid id, CancellationToken ct);
    Task UpdateAsync(EmailVerificationCode entity, CancellationToken ct);
}

using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IEmailVerificationCodeRepository:IRepository<EmailVerificationCode,Guid>
{
    Task<EmailVerificationCode?> GetActiveByEmailAsync(string email, CancellationToken ct);
    Task<bool> MarkAsUsedAsync(Guid id, CancellationToken ct);
}

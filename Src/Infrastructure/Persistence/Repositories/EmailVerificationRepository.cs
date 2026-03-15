using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class EmailVerificationCodeRepository
    : GenericRepository<EmailVerificationCode,Guid>,IEmailVerificationCodeRepository
{
    private readonly CineVerseDbContext _context;

    public EmailVerificationCodeRepository(CineVerseDbContext context): base(context)
    {
        _context = context;
    }


    public async Task<EmailVerificationCode?> GetActiveByEmailAsync(string email, CancellationToken ct)
    {
        return await _context.EmailVerificationCodes
            .Where(x => x.Email == email && !x.IsUsed)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> MarkAsUsedAsync(Guid id, CancellationToken ct)
    {
        var entity = await _context.EmailVerificationCodes.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
            return false;

        entity.IsUsed = true;
        await _context.SaveChangesAsync(ct);
        return true;
    }

}
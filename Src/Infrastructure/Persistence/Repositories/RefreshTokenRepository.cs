using Application.Auth.Refresh.Queries;
using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly CineVerseDbContext _context;

    public RefreshTokenRepository(CineVerseDbContext context) => _context = context;


    public Task<RefreshTokenWithUserResult?> GetByTokenWithUserAsync(string token, CancellationToken ct = default)
       => _context.RefreshTokens
           .Where(rt => rt.Token == token)
           .Join(
               _context.Users,
               rt => rt.UserId,
               u => u.Id,
               (rt, u) => new RefreshTokenWithUserResult(
                   rt.Token,
                   rt.ExpiresAtUtc,
                   u.Id,
                   u.UserName!,
                   u.Email!
               )
           )
           .FirstOrDefaultAsync(ct);

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken ct = default)
    {
        await _context.RefreshTokens.AddAsync(refreshToken, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> DeleteByTokenAsync(string token, CancellationToken ct = default)
    {
        var affected = await _context.RefreshTokens
            .Where(x => x.Token == token && x.ExpiresAtUtc > DateTime.UtcNow)
            .ExecuteDeleteAsync(ct);

        return affected == 1;
    }
}
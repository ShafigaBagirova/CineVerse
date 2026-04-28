using Application.Abstracts.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly CineVerseDbContext _context;
    public RefreshTokenRepository(CineVerseDbContext context)
    {
        _context = context;
    }
    public async Task AddAsync(RefreshToken refreshToken, CancellationToken ct = default)
    {
        await _context.RefreshTokens.AddAsync(refreshToken, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> DeleteByTokenAsync(string token, CancellationToken ct = default)
    {
        var affected = await _context.RefreshTokens
             .Where(x => x.Token == token)
             .ExecuteDeleteAsync(ct);

        return affected > 0;
    }

    public Task<RefreshToken?> GetByTokenWithUserAsync(string token, CancellationToken ct = default)
    {
        return _context.RefreshTokens
              .Include(x => x.User)
              .FirstOrDefaultAsync(x => x.Token == token, ct);
    }

}

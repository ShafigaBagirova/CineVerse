using Application.Common.Interfaces;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Email;

public sealed class UserEmailProvider : IUserEmailProvider
{
    private readonly CineVerseDbContext _context;

    public UserEmailProvider(CineVerseDbContext context)
    {
        _context = context;
    }

    public async Task<string?> GetEmailByUserIdAsync(string userId, CancellationToken cancellationToken)
    {
        return await _context.Users
            .Where(x => x.Id == userId)
            .Select(x => x.Email)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
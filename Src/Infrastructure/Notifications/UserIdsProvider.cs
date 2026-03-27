using Application.Common.Interfaces;
using Domain.Enums;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Notifications;

public sealed class UserIdsProvider : IUserIdsProvider
{
    private readonly CineVerseDbContext _context;

    public UserIdsProvider(CineVerseDbContext context)
    {
        _context = context;
    }

    public async Task<List<string>> GetAllUserIdsAsync(CancellationToken cancellationToken)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(x => x.Status==UserStatus.Active)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);
    }
}
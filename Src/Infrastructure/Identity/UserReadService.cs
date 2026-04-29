using Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Identity;

public sealed class UserReadService : IUserReadService
{
    private readonly UserManager<CineVerseUser> _userManager;

    public UserReadService(UserManager<CineVerseUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<string?> GetUserNameAsync(string userId, CancellationToken cancellationToken)
    {
        return await _userManager.Users
            .Where(x => x.Id == userId)
            .Select(x => x.UserName)
            .FirstOrDefaultAsync(cancellationToken);
    }
}

using Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity;

public class IdentityUserUniquenessChecker : IUserUniquenessChecker
{
    private readonly UserManager<CineVerseUser> _userManager;

    public IdentityUserUniquenessChecker(UserManager<CineVerseUser> userManager)
        => _userManager = userManager;

    public async Task<bool> IsUserNameTakenAsync(string userName, CancellationToken ct)
        => await _userManager.FindByNameAsync(userName) != null;

    public async Task<bool> IsEmailTakenAsync(string email, CancellationToken ct)
        => await _userManager.FindByEmailAsync(email) != null;
}


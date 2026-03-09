using Application.Auth.Options;
using Domain.Constants;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Infrastructure.Persistence.Data;

public static class AdminSeeder
{
    public static async Task SeedAsync(UserManager<CineVerseUser> userManager, IOptions<SeedOptions> seedOptions)
    {
        var opts = seedOptions.Value;

        if (string.IsNullOrWhiteSpace(opts.AdminEmail) ||
            string.IsNullOrWhiteSpace(opts.AdminPassword))
            return;

        var existing = await userManager.FindByEmailAsync(opts.AdminEmail);
        if (existing is not null)
            return;

        var admin = new CineVerseUser
        {
            UserName = opts.AdminEmail,
            Email = opts.AdminEmail,
            FullName = opts.AdminFullName,
            EmailConfirmed = true
        };

        var created = await userManager.CreateAsync(admin, opts.AdminPassword);
        if (!created.Succeeded)
            return;

        await userManager.AddToRoleAsync(admin, RoleNames.Admin);
    }
}

using Domain.Constants;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Persistence.Data;

public static class RoleSeeder
{
    public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[] { RoleNames.Admin, RoleNames.User, RoleNames.Vip };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}
using Application.Auth.Options;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Infrastructure.Persistence.Data;

public static class IdentitySeedExtensions
{
    public static async Task SeedIdentityAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await RoleSeeder.SeedAsync(roleManager);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<CineVerseUser>>();
        var seedOptions = scope.ServiceProvider.GetRequiredService<IOptions<SeedOptions>>();

        await AdminSeeder.SeedAsync(userManager, seedOptions);
    }
}

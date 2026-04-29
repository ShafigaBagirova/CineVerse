using API.Middlewares;
using Application.Common.Options;
using Infrastructure.Identity;
using Infrastructure.Persistence.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Serilog;
using API.Hubs;

namespace API.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseApi(this WebApplication app)
    {
        SeedOnce(app);
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CineVerse API v1");
            });
        }

        app.UseHttpsRedirection();

        app.UseRouting();
        app.UseCors("Frontend");
       
        app.UseSerilogRequestLogging();

        app.UseAuthentication();
        app.UseAuthorization();
        

        app.UseMiddleware<ExceptionHandlingMiddleware>();
       
        app.MapControllers();
        app.MapHub<ChatHub>("/chatHub");
        app.MapHub<ChatHub>("/hubs/chat");
        return app;
    }
    private static void SeedOnce(WebApplication app)
        => SeedOnceAsync(app).GetAwaiter().GetResult();

    private static async Task SeedOnceAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await RoleSeeder.SeedAsync(roleManager);

        if (app.Environment.IsDevelopment())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<CineVerseUser>>();
            var seedOptions = scope.ServiceProvider.GetRequiredService<IOptions<SeedOptions>>();

            await AdminSeeder.SeedAsync(userManager, seedOptions);
        }
    }
}

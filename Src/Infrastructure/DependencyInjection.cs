using Application.Common.Interfaces;
using Application.Common.Options;
using Infrastructure.Email;
using Infrastructure.FileStorage;
using Infrastructure.Identity;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Redis;
using Infrastructure.Tmdb;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Minio;
using Serilog;
using StackExchange.Redis;
using System.Net.Http.Headers;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<CineVerseDbContext>(opt =>
            opt.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        services.AddIdentity<CineVerseUser, IdentityRole>(opt =>
        {
            opt.User.RequireUniqueEmail = true;
            opt.Password.RequiredLength = 8;
            opt.Password.RequireDigit = true;
            opt.Password.RequireUppercase = true;
            opt.Password.RequireLowercase = true;
            opt.Password.RequireNonAlphanumeric = true;
        })
        .AddEntityFrameworkStores<CineVerseDbContext>()
        .AddDefaultTokenProviders();

        services.Configure<JwtOptions>(config.GetSection(JwtOptions.SectionName));
        services.Configure<SeedOptions>(config.GetSection(SeedOptions.SectionName));

        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IUserUniquenessChecker, IdentityUserUniquenessChecker>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IEmailVerificationCodeRepository, EmailVerificationCodeRepository>();
        services.ConfigureApplicationCookie(options =>
        {
            options.Events.OnRedirectToLogin = ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };

            options.Events.OnRedirectToAccessDenied = ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });

        services.Configure<EmailOptions>(config.GetSection(EmailOptions.SectionName));
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.Configure<FrontendOptions>(
       config.GetSection(FrontendOptions.SectionName));

        services.Configure<MinioOptions>(
            config.GetSection("Minio"));

        var minioOptions = config
            .GetSection("Minio")
            .Get<MinioOptions>()!;

        services.AddSingleton<IMinioClient>(_ =>
            new MinioClient()
                .WithEndpoint(minioOptions.Endpoint)
                .WithCredentials(minioOptions.AccessKey, minioOptions.SecretKey)
                .WithSSL(minioOptions.UseSSL)
                .Build());

        services.AddScoped<IFileStorageService, MinioFileStorageService>();
        services.AddScoped<IFileStorageService, S3MinioFileStorageService>();
        services.AddScoped<IMovieRepository, MovieRepository>();
        services.AddScoped<IMovieVideoRepository, MovieVideoRepository>();
        services.Configure<RedisOptions>(config.GetSection("Redis"));

        var redisOptions = config.GetSection("Redis").Get<RedisOptions>();

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(redisOptions!.ConnectionString));

        services.AddScoped<ICacheService, RedisCacheService>();
        services.Configure<TmdbOptions>(
            config.GetSection(TmdbOptions.SectionName));

        services.AddHttpClient<IMovieProvider, TmdbMovieProvider>((sp, client) =>
        {
            var options = sp.GetRequiredService<
                Microsoft.Extensions.Options.IOptions<TmdbOptions>>().Value;

            client.BaseAddress = new Uri(options.BaseUrl);
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", options.ReadAccessToken);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        });
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IMovieRatingRepository, MovieRatingRepository>();
        services.AddScoped<IGenreRepository,GenreRepository>();
        services.AddScoped<IMovieGenreRepository,MovieGenreRepository>();
        services.AddScoped<IReviewRepository,ReviewRepository>();
        services.AddScoped<IWatchListItemRepository, WatchListItemRepository>();
        services.AddScoped<IWatchLogRepository, WatchLogRepository>();
        services.AddScoped<IFollowRepository,FollowRepository>();
        services.AddScoped<IUserSuggestionRepository, UserSuggestionRepository>();
        return services;
    }
}
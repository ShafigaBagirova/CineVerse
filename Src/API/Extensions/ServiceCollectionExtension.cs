using Application.Common.Options;
using Domain.Constants;
using Infrastructure.Tmdb.Mapping;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Application.Common.Mappings;

namespace API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration config)
    {
        services.AddControllers();

        services.AddSwaggerGen(options =>
        {

            options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "JWT Authorization header using the Bearer scheme."
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("bearer", document)] = []
            });
        });


        var jwtSection = config.GetSection(JwtOptions.SectionName);
        var jwt = jwtSection.Get<JwtOptions>()
                  ?? throw new InvalidOperationException($"Missing configuration section '{JwtOptions.SectionName}'.");

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),

                    ClockSkew = TimeSpan.Zero,

                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = ClaimTypes.NameIdentifier
                };

            });

        services.AddAuthorization(options =>
        {
  
            options.AddPolicy(Policies.Authenticated, p => p.RequireAuthenticatedUser());

            options.AddPolicy(Policies.AdminOnly, p => p.RequireRole(RoleNames.Admin));
            options.AddPolicy(Policies.ManageMovies, p => p.RequireRole(RoleNames.Admin));
            options.AddPolicy(Policies.VipOnly, p => p.RequireRole(RoleNames.Vip));

            options.AddPolicy(Policies.ManageCinemas, p => p.RequireRole(RoleNames.Admin));
            options.AddPolicy(Policies.ManageScreenings, p => p.RequireRole(RoleNames.Admin));
            options.AddPolicy(Policies.ManageFoods, p => p.RequireRole(RoleNames.Admin));

            options.AddPolicy(Policies.PurchaseTicket, p => p.RequireAuthenticatedUser());

            options.AddPolicy(Policies.ViewFriendTicketActivity, p => p.RequireRole(RoleNames.Vip));
            options.AddPolicy(Policies.ViewTasteMatchSuggestions, p => p.RequireRole(RoleNames.Vip));

            // Owner policies - hələlik placeholder (sonra handler yazacağıq)
            // options.AddPolicy(Policies.ReviewOwnerOrAdmin, p => p.Requirements.Add(new ReviewOwnerOrAdminRequirement()));
        });
        services.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies());


        return services;
    }
}

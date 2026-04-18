using API;
using Application.Common.Options;
using Application.Common.Responses;
using Domain.Constants;
using Infrastructure.Payments;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration config)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.Converters.Add(
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
            });

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(x => x.Value is { Errors.Count: > 0 })
                    .SelectMany(x =>
                        x.Value!.Errors.Select(e =>
                            string.IsNullOrEmpty(x.Key)
                                ? e.ErrorMessage
                                : $"{x.Key}: {e.ErrorMessage}"))
                    .ToList();
                var response = BaseResponse.Fail("Validation failed.", errors);
                return new BadRequestObjectResult(response);
            };
        });

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

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json; charset=utf-8";
                        var message = "Authentication required.";
                        if (!string.IsNullOrEmpty(context.Error) || !string.IsNullOrEmpty(context.ErrorDescription))
                            message = context.ErrorDescription ?? context.Error ?? message;
                        var body = BaseResponse.Fail(message);
                        await context.Response.WriteAsync(
                            JsonSerializer.Serialize(body, ApiJsonSerializerOptions.Web));
                    },
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json; charset=utf-8";
                        var body = BaseResponse.Fail("You do not have permission to perform this action.");
                        await context.Response.WriteAsync(
                            JsonSerializer.Serialize(body, ApiJsonSerializerOptions.Web));
                    }
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

           // options.AddPolicy(Policies.ReviewOwnerOrAdmin, p => p.Requirements.Add(new ReviewOwnerOrAdminRequirement()));
        });
        services.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies());

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Application.DependencyInjection).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(ProcessStripeWebhookCommandHandler).Assembly);
        });
        return services;
    }
}

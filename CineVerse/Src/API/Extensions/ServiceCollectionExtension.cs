using Infrastructure.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi;
using JwtOptions = CineVerse.Application.Common.Options.JwtOptions;
namespace API.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        //Jwt 
        services
           .AddAuthentication(options =>
           {
               options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
               options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
           })
           .AddJwtBearer();
        services.ConfigureOptions<ConfigureJwtBearerOptions>();
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddSwaggerGen(options =>
        {

         options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme

        {
        Name = "Authorization",
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
       });

       options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
        {

        {
            new OpenApiSecuritySchemeReference("Bearer"),
            new List<string>()
        }
       });
       });

        services.AddControllers();

        return services;
    }
}

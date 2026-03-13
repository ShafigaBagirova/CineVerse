using API.Extensions;
using Application;
using Infrastructure;
using Infrastructure.Persistence.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApi(builder.Configuration);


var app = builder.Build();
await app.Services.SeedIdentityAsync();

app.UseApi();

app.Run();
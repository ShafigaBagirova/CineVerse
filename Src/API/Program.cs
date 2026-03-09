using API.Extensions;
using Application;
using Infrastructure;
using Infrastructure.Persistence.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApi(builder.Configuration);

var app = builder.Build();
await app.Services.SeedIdentityAsync();

app.UseApi();

app.Run();
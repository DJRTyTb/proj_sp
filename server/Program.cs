using Server.Hubs;
using Server.Services;
using Server.Services.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = false;
});

builder.Services.AddSingleton<GameManager>();
builder.Services.AddSingleton<GameLockProvider>();
builder.Services.AddSingleton<IRandomProvider, RandomProvider>();

builder.Services.AddScoped<BoardGenerator>();
builder.Services.AddScoped<MoveValidator>();
builder.Services.AddScoped<GameService>();

builder.Services.AddHostedService<GameCleanupService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetIsOriginAllowed(_ => true);
    });
});

WebApplication app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors();

app.MapHub<GameHub>("/gamehub");

app.Run();
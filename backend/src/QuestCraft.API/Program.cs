using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuestCraft.API.Hubs;
using QuestCraft.API.Middleware;
using QuestCraft.API.Services;
using QuestCraft.Application;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;
using QuestCraft.Infrastructure;
using QuestCraft.Infrastructure.Identity;
using QuestCraft.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt konfiqurasiyası tapılmadı.");

builder.Services
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
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ClockSkew = TimeSpan.FromSeconds(30),
        };
        // SignalR's websocket/SSE transports can't attach an Authorization header, so the JS client
        // sends the token as a query string param instead — only honored on the hub path.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            },
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSignalR();
builder.Services.AddScoped<IRealtimeNotifier, SignalRNotifier>();
builder.Services.AddScoped<IBattleHubNotifier, SignalRBattleNotifier>();
builder.Services.AddHostedService<QuestCraft.API.Services.WeeklyRecapBackgroundService>();

builder.Services.AddMemoryCache();

builder.Services.AddRateLimiter(options =>
{
    // Partitioned per client IP — AddFixedWindowLimiter's simple overload would otherwise share one
    // global budget across every caller, letting one abusive IP lock out everyone else.
    options.AddPolicy("auth", httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
        });
    });

    options.OnRejected = async (rejectedContext, cancellationToken) =>
    {
        rejectedContext.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        var db = rejectedContext.HttpContext.RequestServices.GetRequiredService<IApplicationDbContext>();
        var ip = rejectedContext.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var endpoint = rejectedContext.HttpContext.Request.Path.Value ?? "unknown";
        var windowStart = DateTime.UtcNow;

        var existing = await db.RateLimitLogs.FirstOrDefaultAsync(
            r => r.IpAddress == ip && r.Endpoint == endpoint && r.WindowStart > windowStart.AddMinutes(-1), cancellationToken);
        if (existing is not null)
        {
            existing.RequestCount++;
        }
        else
        {
            db.RateLimitLogs.Add(new RateLimitLog { IpAddress = ip, Endpoint = endpoint, RequestCount = 1, WindowStart = windowStart });
        }
        await db.SaveChangesAsync(cancellationToken);

        await rejectedContext.HttpContext.Response.WriteAsJsonAsync(
            new { success = false, data = (object?)null, message = "Çox sayda cəhd. Bir az sonra yenidən cəhd edin.", errors = (object?)null },
            cancellationToken);
    };
});

const string FrontendCorsPolicy = "Frontend";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await db.Database.MigrateAsync();
    await ApplicationDbContextSeeder.SeedAsync(db, passwordHasher);
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors(FrontendCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();
app.MapHub<NotificationsHub>("/hubs/notifications");
app.MapHub<QuestCraft.API.Hubs.BattleHub>("/hubs/battle");

app.Run();

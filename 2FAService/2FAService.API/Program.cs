
using Microsoft.EntityFrameworkCore;
using TwoFactorService.Application.Interfaces;
using TwoFactorService.Application.Services;
using TwoFactorService.Infrastructure.Persistence.Repositories;
using TwoFactorService.Infrastructure.Persistance;
using TwoFactorService.Infrastructure.Persistence;
using System.Threading.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddSwaggerGen();

builder.Services.AddDataProtection();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IAppDbContext, AppDbContext>();
builder.Services.AddScoped<ITwoFactorService, TwoFactorAuthService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITwoFactorSettingsRepository, TwoFactorSettingsRepository>();
builder.Services.AddScoped<IRecoveryCodeRepository, RecoveryCodeRepository>();

builder.Services.AddControllers();

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("bruteforce_2fa", httpContext =>
    {
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();

        var partitionKey = string.IsNullOrEmpty(ipAddress) ? "DEFAULT_KEY" : ipAddress;

        return RateLimitPartition.GetFixedWindowLimiter(partitionKey, key =>
            new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(10),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Jwt:Authority"];

        options.Audience = builder.Configuration["Jwt:Audience"];

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapSwagger();

app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

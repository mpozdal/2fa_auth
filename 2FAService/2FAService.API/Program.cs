
using Microsoft.EntityFrameworkCore;
using TwoFactorService.Application.Interfaces;
using TwoFactorService.Application.Services;
using TwoFactorService.Infrastructure.Persistence.Repositories;
using TwoFactorService.Infrastructure.Persistance;
using TwoFactorService.Infrastructure.Persistence;
using System.Threading.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<IAppDbContext, AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddMassTransit(config => {
    config.UsingRabbitMq((context, cfg) => {
        cfg.Host(builder.Configuration.GetValue<string>("RabbitMq:Host"), "/", h => {
            h.Username(builder.Configuration.GetValue<string>("RabbitMq:Username")!);
            h.Password(builder.Configuration.GetValue<string>("RabbitMq:Password")!);
        });
    });
});

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Wprowadź 'Bearer' [spacja] a następnie swój token. \n\nPrzykład: 'Bearer 12345abcdef'"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDataProtection();

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

builder.Services.AddAuthentication(options =>
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


        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!))
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

using AuthService.Infrastructure.Persistence;
using AuthService.Application.Services;
using AuthService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using AuthService.Infrastructure.Security;
using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using AuthService.Infrastructure.HttpClients;
using AuthService.Infrastructure.Repositories;
using AuthService.Infrastructure.Data;
using MassTransit;
using AuthService.Application.Consumers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddMassTransit(config => {
    config.AddConsumer<User2FAStatusConsumer>();

    config.UsingRabbitMq((context, cfg) => {

        cfg.Host(builder.Configuration.GetValue<string>("RabbitMq:Host"), "/", h =>
        {
            h.Username(builder.Configuration.GetValue<string>("RabbitMq:Username")!);
            h.Password(builder.Configuration.GetValue<string>("RabbitMq:Password")!);
        });

        cfg.ReceiveEndpoint("auth-service-events", e => {
            e.ConfigureConsumer<User2FAStatusConsumer>(context);
        });
    });
});

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRespository>();

builder.Services.AddControllers();

builder.Services.AddHttpClient<ITwoFactorApiClient, TwoFactorApiClient>(client =>
{
    string? serviceUrl = builder.Configuration["ServiceUrls:TwoFactorService"];

    if (string.IsNullOrEmpty(serviceUrl))
    {
        throw new InvalidOperationException("TwoFactorService URL not configured in appsettings.json");
    }

    client.BaseAddress = new Uri(serviceUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
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

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

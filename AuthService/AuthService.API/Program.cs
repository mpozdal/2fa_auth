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
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(connectionString));


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

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IUserService, UserService>();
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

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

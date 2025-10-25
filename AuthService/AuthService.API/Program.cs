using AuthService.Infrastructure.Persistence;
using AuthService.Application.Services;
using AuthService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();

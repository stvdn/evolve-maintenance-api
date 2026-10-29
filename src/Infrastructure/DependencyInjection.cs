using System.Text;
using Application.Interfaces;
using Application.Services;
using Domain.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database - InMemory for development, PostgreSQL for production
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("MSApiMantenimientosEvolutivos"));
        }
        else
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));
        }

        // Repositories
        services.AddScoped<IMaintenanceRepository, MaintenanceRepository>();
        services.AddScoped<IMetricRepository, MetricRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IMaintenanceService, MaintenanceService>();
        services.AddScoped<IMetricService, MetricService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        // JWT Authentication
        var jwtSecret = configuration["Jwt:Secret"] ?? "DefaultSuperSecretKey1234567890ABCDEF!";
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"] ?? "MSApiMantenimientosEvolutivos",
                    ValidAudience = configuration["Jwt:Audience"] ?? "BancoEcuador",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
                };
            });

        services.AddAuthorization();

        return services;
    }
}

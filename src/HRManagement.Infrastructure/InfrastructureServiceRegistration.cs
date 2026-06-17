// File Path: src/HRManagement.Infrastructure/InfrastructureServiceRegistration.cs
// Purpose: Extension class to configure dependency injection for the Infrastructure layer.
// Code Explanation: Registers ITokenService and IAuthService, configures strongly-typed JwtSettings, configures JWT Bearer authentication, and establishes the role authorization policies (AdminOnly, HRManagerOnly, EmployeeOnly, AdminOrHR).

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using HRManagement.Application.Contracts.Identity;
using HRManagement.Application.Models.Identity;
using HRManagement.Infrastructure.Services;

namespace HRManagement.Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAuthService, AuthService>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = System.TimeSpan.Zero,
                    ValidIssuer = configuration["JwtSettings:Issuer"],
                    ValidAudience = configuration["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"] ?? "SecretDefaultKeySuperLongPasswordStringKeyForJWTTokenVerification"))
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                options.AddPolicy("HRManagerOnly", policy => policy.RequireRole("HR Manager"));
                options.AddPolicy("EmployeeOnly", policy => policy.RequireRole("Employee"));
                options.AddPolicy("AdminOrHR", policy => policy.RequireRole("Admin", "HR Manager"));
            });

            return services;
        }
    }
}

// File Path: src/HRManagement.API/Program.cs
// Purpose: Entry point for the Web API presentation layer.
// Code Explanation: Sets up dependency injection for Application, Persistence, and Infrastructure layers. Integrates global exception handlers, maps MVC controllers, maps standard health check endpoints, and configures Swagger with JWT Bearer security UI headers.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using HRManagement.Application;
using HRManagement.Persistence;
using HRManagement.Infrastructure;
using HRManagement.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Register Layer Services
builder.Services.AddApplicationServices();
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddControllers();

// Configure Swagger/OpenAPI with JWT Bearer Security UI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "HR Management API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

// Configure Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

var app = builder.Build();

// Enable Global Exception Handler Middleware
app.UseMiddleware<ExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "HR Management API v1"));
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Map Endpoints
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

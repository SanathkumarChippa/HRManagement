// File Path: src/HRManagement.Application/ApplicationServiceRegistration.cs
// Purpose: Extension class to configure dependency injection for the Application layer.
// Code Explanation: Automatically registers AutoMapper profiles and FluentValidation validators scanning the executing assembly.

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;

namespace HRManagement.Application
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            return services;
        }
    }
}

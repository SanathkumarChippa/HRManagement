// File Path: src/HRManagement.Persistence/PersistenceServiceRegistration.cs
// Purpose: Extension class to register dependency injection for the Persistence layer.
// Code Explanation: Sets up SQL Server configurations for ApplicationDbContext and registers GenericRepository, specific repositories, the ADO.NET reporting repository, and the Unit of Work.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HRManagement.Application.Contracts.Persistence;
using HRManagement.Persistence.Repositories;

namespace HRManagement.Persistence
{
    public static class PersistenceServiceRegistration
    {
        public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IDepartmentRepository, DepartmentRepository>();
            services.AddScoped<ILeaveRepository, LeaveRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IAuditRepository, AuditRepository>();
            services.AddScoped<IWorkforceReportRepository, WorkforceReportRepository>();

            return services;
        }
    }
}

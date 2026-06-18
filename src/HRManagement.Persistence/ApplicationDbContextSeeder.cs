// File Path: src/HRManagement.Persistence/ApplicationDbContextSeeder.cs
// Purpose: Seed data helper class to populate database tables on initialization.
// Code Explanation: Automatically adds standard roles (Admin, HR Manager, Employee), default departments (HR, IT, Finance, Marketing, Sales, Operations), leave configurations, and creates default users linked to corresponding Employee profile rows.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HRManagement.Domain.Entities;

namespace HRManagement.Persistence
{
    public static class ApplicationDbContextSeeder
    {
        public static async Task SeedDatabaseAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            // 1. Seed Roles
            var defaultRoles = new List<ApplicationRole>
            {
                new("Admin", "Full administrative capabilities"),
                new("HR Manager", "Human resources and department lifecycle management"),
                new("Employee", "Standard employee profile access")
            };

            foreach (var role in defaultRoles)
            {
                if (!await roleManager.RoleExistsAsync(role.Name!))
                {
                    await roleManager.CreateAsync(role);
                }
            }

            // 2. Seed Departments
            if (!await context.Departments.AnyAsync())
            {
                var departments = new List<Department>
                {
                    new() { Name = "HR", CreatedBy = "System" },
                    new() { Name = "IT", CreatedBy = "System" },
                    new() { Name = "Finance", CreatedBy = "System" },
                    new() { Name = "Sales", CreatedBy = "System" },
                    new() { Name = "Marketing", CreatedBy = "System" },
                    new() { Name = "Operations", CreatedBy = "System" }
                };

                await context.Departments.AddRangeAsync(departments);
                await context.SaveChangesAsync();
            }

            // 3. Seed LeaveTypes
            if (!await context.LeaveTypes.AnyAsync())
            {
                var leaveTypes = new List<LeaveType>
                {
                    new() { Name = "Casual Leave", DefaultAllocationDays = 12, CreatedBy = "System" },
                    new() { Name = "Sick Leave", DefaultAllocationDays = 10, CreatedBy = "System" },
                    new() { Name = "Paid Leave", DefaultAllocationDays = 15, CreatedBy = "System" },
                    new() { Name = "Maternity Leave", DefaultAllocationDays = 180, CreatedBy = "System" }
                };

                await context.LeaveTypes.AddRangeAsync(leaveTypes);
                await context.SaveChangesAsync();
            }

            // 4. Seed Employees & Users
            var itDept = await context.Departments.FirstOrDefaultAsync(d => d.Name == "IT");
            var hrDept = await context.Departments.FirstOrDefaultAsync(d => d.Name == "HR");

            if (itDept != null && hrDept != null)
            {
                // Seed Admin User
                var adminEmail = "admin@hrmanagement.com";
                if (await userManager.FindByEmailAsync(adminEmail) == null)
                {
                    var adminEmployee = new Employee
                    {
                        EmployeeCode = "EMP-2026-0001",
                        FirstName = "System",
                        LastName = "Administrator",
                        Email = adminEmail,
                        PhoneNumber = "1234567890",
                        Gender = "Male",
                        Designation = "IT Administrator",
                        DateOfJoining = DateTime.UtcNow,
                        EmploymentStatus = "Active",
                        DepartmentId = itDept.Id,
                        CreatedBy = "System"
                    };

                    await context.Employees.AddAsync(adminEmployee);
                    await context.SaveChangesAsync();

                    var adminUser = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true,
                        IsActive = true,
                        EmployeeId = adminEmployee.Id
                    };

                    var result = await userManager.CreateAsync(adminUser, "Admin@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                    }
                }

                // Seed HR Manager User
                var hrEmail = "hr@hrmanagement.com";
                if (await userManager.FindByEmailAsync(hrEmail) == null)
                {
                    var hrEmployee = new Employee
                    {
                        EmployeeCode = "EMP-2026-0002",
                        FirstName = "Jane",
                        LastName = "HR",
                        Email = hrEmail,
                        PhoneNumber = "0987654321",
                        Gender = "Female",
                        Designation = "HR Manager",
                        DateOfJoining = DateTime.UtcNow,
                        EmploymentStatus = "Active",
                        DepartmentId = hrDept.Id,
                        CreatedBy = "System"
                    };

                    await context.Employees.AddAsync(hrEmployee);
                    await context.SaveChangesAsync();

                    var hrUser = new ApplicationUser
                    {
                        UserName = hrEmail,
                        Email = hrEmail,
                        EmailConfirmed = true,
                        IsActive = true,
                        EmployeeId = hrEmployee.Id
                    };

                    var result = await userManager.CreateAsync(hrUser, "HRManager@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(hrUser, "HR Manager");
                    }
                }

                // Seed Employee User (test.user@hrmanagement.com)
                var employeeEmail = "test.user@hrmanagement.com";
                var hrEmployeeRecord = await context.Employees.FirstOrDefaultAsync(e => e.Email == hrEmail);

                var existingUser = await userManager.FindByEmailAsync(employeeEmail);
                Employee? employeeRecord = await context.Employees.FirstOrDefaultAsync(e => e.Email == employeeEmail && !e.IsDeleted);

                if (employeeRecord == null)
                {
                    employeeRecord = new Employee
                    {
                        EmployeeCode = "EMP-2026-0003",
                        FirstName = "Test",
                        LastName = "Employee",
                        Email = employeeEmail,
                        PhoneNumber = "555-019-2834",
                        Gender = "Male",
                        Designation = "Software Engineer",
                        DateOfJoining = DateTime.UtcNow,
                        EmploymentStatus = "Active",
                        DepartmentId = itDept.Id,
                        ManagerId = hrEmployeeRecord?.Id,
                        CreatedBy = "System"
                    };

                    await context.Employees.AddAsync(employeeRecord);
                    await context.SaveChangesAsync();
                }

                if (existingUser == null)
                {
                    var employeeUser = new ApplicationUser
                    {
                        UserName = employeeEmail,
                        Email = employeeEmail,
                        EmailConfirmed = true,
                        IsActive = true,
                        EmployeeId = employeeRecord.Id,
                        MustChangePassword = true,
                        IsFirstLogin = true
                    };

                    var result = await userManager.CreateAsync(employeeUser, "Employee@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(employeeUser, "Employee");
                    }
                }
                else if (!existingUser.EmployeeId.HasValue || existingUser.EmployeeId.Value != employeeRecord.Id)
                {
                    existingUser.EmployeeId = employeeRecord.Id;
                    await userManager.UpdateAsync(existingUser);
                }

                // Seed Leave Balances for all seeded users (Admin, HR Manager, Employee)
                var seededEmails = new[] { adminEmail, hrEmail, employeeEmail };
                var leaveTypes = await context.LeaveTypes.ToListAsync();

                foreach (var emailAddr in seededEmails)
                {
                    var emp = await context.Employees.FirstOrDefaultAsync(e => e.Email == emailAddr && !e.IsDeleted);
                    if (emp != null)
                    {
                        foreach (var lt in leaveTypes)
                        {
                            var hasBalance = await context.LeaveBalances.AnyAsync(lb => lb.EmployeeId == emp.Id && lb.LeaveTypeId == lt.Id && lb.Year == DateTime.UtcNow.Year);
                            if (!hasBalance)
                            {
                                var balance = new LeaveBalance
                                {
                                    EmployeeId = emp.Id,
                                    LeaveTypeId = lt.Id,
                                    AllocatedDays = lt.DefaultAllocationDays,
                                    UsedDays = 0,
                                    PendingDays = 0,
                                    Year = DateTime.UtcNow.Year,
                                    CreatedBy = "System"
                                };
                                await context.LeaveBalances.AddAsync(balance);
                            }
                        }
                    }
                }
                await context.SaveChangesAsync();
            }
        }
    }
}

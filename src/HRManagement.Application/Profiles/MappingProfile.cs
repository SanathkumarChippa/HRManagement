// File Path: src/HRManagement.Application/Profiles/MappingProfile.cs
// Purpose: AutoMapper configuration mapping entities to DTOs.
// Code Explanation: Sets up profiles for Departments, Employees, LeaveRequests, and LeaveBalances, handling flattened field resolution like resolving department/manager names from references.

using AutoMapper;
using HRManagement.Application.DTOs.Department;
using HRManagement.Application.DTOs.Employee;
using HRManagement.Application.DTOs.Leave;
using HRManagement.Domain.Entities;

namespace HRManagement.Application.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Department Mapping
            CreateMap<Department, DepartmentDto>().ReverseMap();
            CreateMap<CreateDepartmentDto, Department>();

            // Employee Mapping
            CreateMap<Employee, EmployeeDto>()
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department.Name))
                .ForMember(dest => dest.ManagerName, opt => opt.MapFrom(src => src.Manager != null ? $"{src.Manager.FirstName} {src.Manager.LastName}" : string.Empty))
                .ReverseMap();
            CreateMap<CreateEmployeeDto, Employee>();

            // LeaveRequest Mapping
            CreateMap<LeaveRequest, LeaveRequestDto>()
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => $"{src.Employee.FirstName} {src.Employee.LastName}"))
                .ForMember(dest => dest.LeaveTypeName, opt => opt.MapFrom(src => src.LeaveType.Name))
                .ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => src.ApprovedBy != null ? $"{src.ApprovedBy.FirstName} {src.ApprovedBy.LastName}" : string.Empty))
                .ReverseMap();
            CreateMap<CreateLeaveRequestDto, LeaveRequest>();

            // LeaveBalance Mapping
            CreateMap<LeaveBalance, LeaveBalanceDto>()
                .ForMember(dest => dest.LeaveTypeName, opt => opt.MapFrom(src => src.LeaveType.Name))
                .ReverseMap();
        }
    }
}

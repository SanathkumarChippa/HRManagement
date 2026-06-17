using System;
using Xunit;
using FluentAssertions;
using HRManagement.Application.DTOs.Employee;
using HRManagement.Application.Validation;

namespace HRManagement.Tests.ValidationTests
{
    public class EmployeeValidationTests
    {
        private readonly CreateEmployeeDtoValidator _validator;

        public EmployeeValidationTests()
        {
            _validator = new CreateEmployeeDtoValidator();
        }

        [Fact]
        public void Valid_Employee_Passes_Validation()
        {
            var dto = new CreateEmployeeDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "+12345678901",
                Gender = "Male",
                Designation = "Developer",
                DepartmentId = 1
            };

            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Empty_FirstName_Fails_Validation()
        {
            var dto = new CreateEmployeeDto { FirstName = "", LastName = "Doe", Email = "j@e.com", PhoneNumber = "1234567890", Gender = "Male", Designation = "Dev", DepartmentId = 1 };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "FirstName");
        }

        [Fact]
        public void Invalid_Email_Fails_Validation()
        {
            var dto = new CreateEmployeeDto { FirstName = "John", LastName = "Doe", Email = "invalid-email", PhoneNumber = "1234567890", Gender = "Male", Designation = "Dev", DepartmentId = 1 };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Email");
        }

        [Fact]
        public void Invalid_PhoneNumber_Fails_Validation()
        {
            var dto = new CreateEmployeeDto { FirstName = "John", LastName = "Doe", Email = "j@e.com", PhoneNumber = "invalid", Gender = "Male", Designation = "Dev", DepartmentId = 1 };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "PhoneNumber");
        }

        [Fact]
        public void DepartmentId_Zero_Fails_Validation()
        {
            var dto = new CreateEmployeeDto { FirstName = "John", LastName = "Doe", Email = "j@e.com", PhoneNumber = "1234567890", Gender = "Male", Designation = "Dev", DepartmentId = 0 };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "DepartmentId");
        }
        
        [Fact]
        public void Designation_Exceeding_Length_Fails_Validation()
        {
            var dto = new CreateEmployeeDto { FirstName = "John", LastName = "Doe", Email = "j@e.com", PhoneNumber = "1234567890", Gender = "Male", Designation = new string('A', 101), DepartmentId = 1 };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Designation");
        }
    }
}

using System;
using Xunit;
using FluentAssertions;
using HRManagement.Application.DTOs.Department;
using HRManagement.Application.Validation;

namespace HRManagement.Tests.ValidationTests
{
    public class DepartmentValidationTests
    {
        private readonly CreateDepartmentDtoValidator _validator;

        public DepartmentValidationTests()
        {
            _validator = new CreateDepartmentDtoValidator();
        }

        [Fact]
        public void Valid_Department_Passes_Validation()
        {
            var dto = new CreateDepartmentDto { Name = "Engineering" };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Empty_DepartmentName_Fails_Validation()
        {
            var dto = new CreateDepartmentDto { Name = "" };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Name");
        }

        [Fact]
        public void DepartmentName_Exceeding_Length_Fails_Validation()
        {
            var dto = new CreateDepartmentDto { Name = new string('A', 101) };
            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Name");
        }
    }
}

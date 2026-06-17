// File Path: src/HRManagement.Application/Validation/CreateDepartmentDtoValidator.cs
// Purpose: Validator checking department creation payloads.
// Code Explanation: Defines rules requiring department Name to be populated and within acceptable length limits.

using FluentValidation;
using HRManagement.Application.DTOs.Department;

namespace HRManagement.Application.Validation
{
    public class CreateDepartmentDtoValidator : AbstractValidator<CreateDepartmentDto>
    {
        public CreateDepartmentDtoValidator()
        {
            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("Department name is required.")
                .MaximumLength(100).WithMessage("Department name must not exceed 100 characters.");
        }
    }
}

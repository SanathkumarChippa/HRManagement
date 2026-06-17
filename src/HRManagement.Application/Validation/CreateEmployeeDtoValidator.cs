// File Path: src/HRManagement.Application/Validation/CreateEmployeeDtoValidator.cs
// Purpose: Validator checking employee creation payloads.
// Code Explanation: Inherits from FluentValidation AbstractValidator and defines rules for required fields, string lengths, correct email formatting, and valid references.

using FluentValidation;
using HRManagement.Application.DTOs.Employee;

namespace HRManagement.Application.Validation
{
    public class CreateEmployeeDtoValidator : AbstractValidator<CreateEmployeeDto>
    {
        public CreateEmployeeDtoValidator()
        {
            RuleFor(p => p.FirstName)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull()
                .MaximumLength(100).WithMessage("{PropertyName} must not exceed 100 characters.");

            RuleFor(p => p.LastName)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull()
                .MaximumLength(100).WithMessage("{PropertyName} must not exceed 100 characters.");

            RuleFor(p => p.Email)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .EmailAddress().WithMessage("A valid email address is required.");

            RuleFor(p => p.PhoneNumber)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .Matches(@"^\+?[0-9]{10,15}$").WithMessage("A valid phone number is required.");

            RuleFor(p => p.Gender)
                .NotEmpty().WithMessage("{PropertyName} is required.");

            RuleFor(p => p.Designation)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .MaximumLength(100).WithMessage("{PropertyName} must not exceed 100 characters.");

            RuleFor(p => p.DepartmentId)
                .GreaterThan(0).WithMessage("Select a valid department.");
        }
    }
}

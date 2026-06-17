// File Path: src/HRManagement.Application/Validation/CreateLeaveRequestDtoValidator.cs
// Purpose: Validator checking leave request payloads.
// Code Explanation: Defines validations including future date rules, end date limits relative to start date, and non-empty reasons.

using System;
using FluentValidation;
using HRManagement.Application.DTOs.Leave;

namespace HRManagement.Application.Validation
{
    public class CreateLeaveRequestDtoValidator : AbstractValidator<CreateLeaveRequestDto>
    {
        public CreateLeaveRequestDtoValidator()
        {
            RuleFor(p => p.LeaveTypeId)
                .GreaterThan(0).WithMessage("Select a valid leave type.");

            RuleFor(p => p.StartDate)
                .NotEmpty().WithMessage("Start date is required.")
                .Must(date => date.Date >= DateTime.Today).WithMessage("Start date must be today or in the future.");

            RuleFor(p => p.EndDate)
                .NotEmpty().WithMessage("End date is required.")
                .GreaterThanOrEqualTo(p => p.StartDate).WithMessage("End date must be on or after the start date.");

            RuleFor(p => p.Reason)
                .NotEmpty().WithMessage("Reason for leave is required.")
                .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.");
        }
    }
}

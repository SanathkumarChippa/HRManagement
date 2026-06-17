using System;
using Xunit;
using FluentAssertions;
using HRManagement.Application.DTOs.Leave;
using HRManagement.Application.Validation;

namespace HRManagement.Tests.ValidationTests
{
    public class LeaveRequestValidationTests
    {
        private readonly CreateLeaveRequestDtoValidator _validator;

        public LeaveRequestValidationTests()
        {
            _validator = new CreateLeaveRequestDtoValidator();
        }

        [Fact]
        public void Valid_LeaveRequest_Passes_Validation()
        {
            var dto = new CreateLeaveRequestDto
            {
                LeaveTypeId = 1,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(3),
                Reason = "Vacation"
            };

            var result = _validator.Validate(dto);
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Invalid_LeaveTypeId_Fails_Validation()
        {
            var dto = new CreateLeaveRequestDto
            {
                LeaveTypeId = 0,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(3),
                Reason = "Vacation"
            };

            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "LeaveTypeId");
        }

        [Fact]
        public void Past_StartDate_Fails_Validation()
        {
            var dto = new CreateLeaveRequestDto
            {
                LeaveTypeId = 1,
                StartDate = DateTime.Today.AddDays(-1),
                EndDate = DateTime.Today.AddDays(3),
                Reason = "Vacation"
            };

            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "StartDate");
        }

        [Fact]
        public void EndDate_Before_StartDate_Fails_Validation()
        {
            var dto = new CreateLeaveRequestDto
            {
                LeaveTypeId = 1,
                StartDate = DateTime.Today.AddDays(3),
                EndDate = DateTime.Today.AddDays(1),
                Reason = "Vacation"
            };

            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "EndDate");
        }

        [Fact]
        public void Empty_Reason_Fails_Validation()
        {
            var dto = new CreateLeaveRequestDto
            {
                LeaveTypeId = 1,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(3),
                Reason = ""
            };

            var result = _validator.Validate(dto);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Reason");
        }
    }
}

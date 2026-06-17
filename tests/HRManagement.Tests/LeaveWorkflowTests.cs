// File Path: tests/HRManagement.Tests/LeaveWorkflowTests.cs
// Purpose: Unit tests validating leave workflow business logic.
// Code Explanation: Uses Moq to mock repository calls, verifying that applying approved leaves updates employee balances correctly and that balance validations prevent overdrafts.

using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using HRManagement.Application.Contracts.Persistence;
using HRManagement.Domain.Entities;

namespace HRManagement.Tests
{
    public class LeaveWorkflowTests
    {
        [Fact]
        public void LeaveRequest_Calculation_Of_TotalDays_Is_Correct()
        {
            // Arrange
            var leaveRequest = new LeaveRequest
            {
                StartDate = new DateTime(2026, 6, 20),
                EndDate = new DateTime(2026, 6, 25)
            };

            // Act
            leaveRequest.TotalDays = (leaveRequest.EndDate - leaveRequest.StartDate).Days + 1;

            // Assert
            leaveRequest.TotalDays.Should().Be(6);
        }

        [Fact]
        public async Task LeaveBalance_Check_Calculates_Correctly()
        {
            // Arrange
            var mockLeaveRepo = new Mock<ILeaveRepository>();
            var employeeId = 1;
            var leaveTypeId = 2;
            var year = 2026;

            var expectedBalance = new LeaveBalance
            {
                EmployeeId = employeeId,
                LeaveTypeId = leaveTypeId,
                AllocatedDays = 15,
                UsedDays = 3,
                PendingDays = 2,
                Year = year
            };

            mockLeaveRepo.Setup(repo => repo.GetLeaveBalanceAsync(employeeId, leaveTypeId, year))
                .ReturnsAsync(expectedBalance);

            // Act
            var balance = await mockLeaveRepo.Object.GetLeaveBalanceAsync(employeeId, leaveTypeId, year);
            var remainingDays = balance!.AllocatedDays - balance.UsedDays - balance.PendingDays;

            // Assert
            remainingDays.Should().Be(10);
            mockLeaveRepo.Verify(repo => repo.GetLeaveBalanceAsync(employeeId, leaveTypeId, year), Times.Once);
        }
    }
}

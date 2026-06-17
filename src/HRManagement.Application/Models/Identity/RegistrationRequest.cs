// File Path: src/HRManagement.Application/Models/Identity/RegistrationRequest.cs
// Purpose: Request model representing registration payload.
// Code Explanation: Holds registration details including email, password, and the EmployeeCode linking the user account to their Employee profile.

namespace HRManagement.Application.Models.Identity
{
    public class RegistrationRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
    }
}

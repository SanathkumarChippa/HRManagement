// File Path: src/HRManagement.Application/Models/Identity/AuthRequest.cs
// Purpose: Request model for user authentication (login).
// Code Explanation: Captures login credentials, Email and Password, to validate user access.

namespace HRManagement.Application.Models.Identity
{
    public class AuthRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}

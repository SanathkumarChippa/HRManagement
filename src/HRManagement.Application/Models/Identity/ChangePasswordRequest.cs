// File Path: src/HRManagement.Application/Models/Identity/ChangePasswordRequest.cs
// Purpose: Request model capturing password update payloads.
// Code Explanation: Holds OldPassword and NewPassword properties.

namespace HRManagement.Application.Models.Identity
{
    public class ChangePasswordRequest
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}

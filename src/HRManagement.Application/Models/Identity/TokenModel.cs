// File Path: src/HRManagement.Application/Models/Identity/TokenModel.cs
// Purpose: Model encapsulating access and refresh tokens.
// Code Explanation: Holds token strings for authentication refresh operations.

namespace HRManagement.Application.Models.Identity
{
    public class TokenModel
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}

// File Path: src/HRManagement.Application/Models/Identity/AuthResponse.cs
// Purpose: Response model returned upon successful authentication.
// Code Explanation: Holds basic user details, roles list, generated JWT Access Token, and Refresh Token.

using System.Collections.Generic;

namespace HRManagement.Application.Models.Identity
{
    public class AuthResponse
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}

// File Path: src/HRManagement.Application/Models/Identity/JwtSettings.cs
// Purpose: Class representing JWT configuration settings.
// Code Explanation: Holds key parameters required for JWT creation and validation, including Key, Issuer, Audience, and DurationInMinutes.

namespace HRManagement.Application.Models.Identity
{
    public class JwtSettings
    {
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public double DurationInMinutes { get; set; }
        public double RefreshTokenExpirationDays { get; set; }
    }
}

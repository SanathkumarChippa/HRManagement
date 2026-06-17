// File Path: src/HRManagement.Application/Contracts/Identity/IAuthService.cs
// Purpose: Interface defining core authentication and session operations.
// Code Explanation: Declares async signatures for login, user registration (linking profile), token refreshes, logouts, and password changes.

using System.Threading.Tasks;
using HRManagement.Application.Models.Identity;

namespace HRManagement.Application.Contracts.Identity
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(AuthRequest request);
        Task<AuthResponse> RegisterAsync(RegistrationRequest request);
        Task<AuthResponse> RefreshTokenAsync(string token, string refreshToken, string ipAddress);
        Task<bool> LogoutAsync(string refreshToken, string ipAddress);
        Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequest request);
    }
}

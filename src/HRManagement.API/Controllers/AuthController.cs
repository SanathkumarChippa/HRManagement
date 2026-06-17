// File Path: src/HRManagement.API/Controllers/AuthController.cs
// Purpose: API controller managing authentication requests.
// Code Explanation: Exposes endpoints for login, registration, password updates, and token refresh logic. Uses IAuthService to coordinate underlying actions and handles request IP address capturing for refresh logs.

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HRManagement.Application.Contracts.Identity;
using HRManagement.Application.Models.Identity;

namespace HRManagement.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthRequest request)
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequest request)
        {
            var response = await _authService.RegisterAsync(request);
            return Ok(response);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenModel request)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var response = await _authService.RefreshTokenAsync(request.AccessToken, request.RefreshToken, ipAddress);
            return Ok(response);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] string refreshToken)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var result = await _authService.LogoutAsync(refreshToken, ipAddress);
            if (!result)
            {
                return BadRequest("Invalid or already revoked token.");
            }
            return Ok(new { Message = "Logged out successfully." });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _authService.ChangePasswordAsync(userId, request);
            if (!result)
            {
                return BadRequest("Password update failed. Verify old password matches criteria.");
            }
            return Ok(new { Message = "Password changed successfully." });
        }
    }
}

// File Path: src/HRManagement.Infrastructure/Services/AuthService.cs
// Purpose: Concrete implementation of IAuthService for user authentication, registration, password updates, and token refresh rotations.
// Code Explanation: Coordinates UserManager, ITokenService, and IUnitOfWork to validate passwords, check employee codes during registration, rotate/revoke refresh tokens, and log out active user sessions.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HRManagement.Application.Contracts.Identity;
using HRManagement.Application.Contracts.Persistence;
using HRManagement.Application.Models.Identity;
using HRManagement.Domain.Entities;
using HRManagement.Persistence;

namespace HRManagement.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService,
            IUnitOfWork unitOfWork,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<AuthResponse> LoginAsync(AuthRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !user.IsActive || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                throw new Exception("Invalid credentials or inactive account.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.GenerateAccessToken(user, roles);
            
            // Create and store Refresh Token
            var refreshToken = _tokenService.GenerateRefreshToken("127.0.0.1");
            refreshToken.UserId = user.Id;
            refreshToken.CreatedBy = user.UserName;

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Roles = roles.ToList(),
                Token = token,
                RefreshToken = refreshToken.Token
            };
        }

        public async Task<AuthResponse> RegisterAsync(RegistrationRequest request)
        {
            var employee = await _unitOfWork.Employees.GetEmployeeByCodeAsync(request.EmployeeCode);
            if (employee == null)
            {
                throw new Exception($"Employee with code {request.EmployeeCode} does not exist.");
            }

            // Verify if a user is already registered for this employee
            var existingUser = await _context.Users.AnyAsync(u => u.EmployeeId == employee.Id);
            if (existingUser)
            {
                throw new Exception("A user account is already registered for this employee.");
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                EmailConfirmed = true,
                IsActive = true,
                EmployeeId = employee.Id
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Registration failed: {errors}");
            }

            await _userManager.AddToRoleAsync(user, "Employee");

            var roles = new List<string> { "Employee" };
            var token = _tokenService.GenerateAccessToken(user, roles);
            var refreshToken = _tokenService.GenerateRefreshToken("127.0.0.1");
            refreshToken.UserId = user.Id;
            refreshToken.CreatedBy = user.UserName;

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Roles = roles,
                Token = token,
                RefreshToken = refreshToken.Token
            };
        }

        public async Task<AuthResponse> RefreshTokenAsync(string token, string refreshToken, string ipAddress)
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(token);
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new Exception("Invalid token details.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.IsActive)
            {
                throw new Exception("User not found or inactive.");
            }

            // Validate the refresh token in database
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.UserId == userId && rt.IsDeleted == false);

            if (storedToken == null || !storedToken.IsActive)
            {
                throw new Exception("Invalid or expired refresh token.");
            }

            // Generate new tokens (Rotation)
            var roles = await _userManager.GetRolesAsync(user);
            var newAccessToken = _tokenService.GenerateAccessToken(user, roles);
            var newRefreshToken = _tokenService.GenerateRefreshToken(ipAddress);
            newRefreshToken.UserId = user.Id;
            newRefreshToken.CreatedBy = user.UserName;

            // Revoke old token
            storedToken.Revoked = DateTime.UtcNow;
            storedToken.RevokedByIp = ipAddress;
            storedToken.ReplacedByToken = newRefreshToken.Token;

            await _context.RefreshTokens.AddAsync(newRefreshToken);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Roles = roles.ToList(),
                Token = newAccessToken,
                RefreshToken = newRefreshToken.Token
            };
        }

        public async Task<bool> LogoutAsync(string refreshToken, string ipAddress)
        {
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (storedToken == null || !storedToken.IsActive)
            {
                return false;
            }

            // Revoke the token
            storedToken.Revoked = DateTime.UtcNow;
            storedToken.RevokedByIp = ipAddress;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequest request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
            return result.Succeeded;
        }
    }
}

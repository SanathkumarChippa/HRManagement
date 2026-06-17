using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Options;
using HRManagement.Application.Models.Identity;
using HRManagement.Domain.Entities;
using HRManagement.Infrastructure.Services;

namespace HRManagement.Tests.Services
{
    public class TokenServiceTests
    {
        private readonly TokenService _tokenService;
        private readonly JwtSettings _jwtSettings;

        public TokenServiceTests()
        {
            _jwtSettings = new JwtSettings
            {
                Key = "SuperSecretKeyThatIsAtLeast32BytesLong123!",
                Issuer = "HRManagementAPI",
                Audience = "HRManagementWeb",
                DurationInMinutes = 60,
                RefreshTokenExpirationDays = 7
            };

            var optionsMock = Options.Create(_jwtSettings);
            _tokenService = new TokenService(optionsMock);
        }

        [Fact]
        public void GenerateAccessToken_Returns_Valid_Jwt_Format()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "user123",
                Email = "test@hrmanagement.com",
                EmployeeId = 1
            };
            var roles = new List<string> { "Employee" };

            // Act
            var token = _tokenService.GenerateAccessToken(user, roles);

            // Assert
            token.Should().NotBeNullOrEmpty();
            var handler = new JwtSecurityTokenHandler();
            handler.CanReadToken(token).Should().BeTrue();
            
            var jwtToken = handler.ReadJwtToken(token);
            jwtToken.Issuer.Should().Be(_jwtSettings.Issuer);
            jwtToken.Audiences.First().Should().Be(_jwtSettings.Audience);
        }

        [Fact]
        public void GenerateRefreshToken_Returns_Valid_Token()
        {
            // Act
            var refreshToken = _tokenService.GenerateRefreshToken("127.0.0.1");

            // Assert
            refreshToken.Should().NotBeNull();
            refreshToken.Token.Should().NotBeNullOrEmpty();
            refreshToken.CreatedByIp.Should().Be("127.0.0.1");
            refreshToken.Expires.Should().BeAfter(DateTime.UtcNow.AddDays(6)); // Approx 7 days
        }

        [Fact]
        public void GenerateAccessToken_Includes_All_Required_Claims()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "user123",
                Email = "admin@hrmanagement.com",
                EmployeeId = 42
            };
            var roles = new List<string> { "Admin" };

            // Act
            var token = _tokenService.GenerateAccessToken(user, roles);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Assert
            jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
            jwtToken.Claims.Should().Contain(c => c.Type == "EmployeeId" && c.Value == "42");
            jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == "admin@hrmanagement.com");
        }
    }
}

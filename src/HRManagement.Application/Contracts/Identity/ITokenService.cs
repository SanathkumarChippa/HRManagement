// File Path: src/HRManagement.Application/Contracts/Identity/ITokenService.cs
// Purpose: Interface defining token assembly and validation operations.
// Code Explanation: Declares methods for assembling a JWT Access Token, generating a secure random RefreshToken, and extracting claims from an expired token context.

using System.Collections.Generic;
using System.Security.Claims;
using HRManagement.Domain.Entities;

namespace HRManagement.Application.Contracts.Identity
{
    public interface ITokenService
    {
        string GenerateAccessToken(ApplicationUser user, IList<string> roles);
        RefreshToken GenerateRefreshToken(string ipAddress);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}

using System.Security.Claims;
using System.Threading.Tasks;
using HRManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace HRManagement.Persistence.Identity
{
    public class ApplicationUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
    {
        public ApplicationUserClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            // Ensure ClaimTypes.Email is added
            if (!string.IsNullOrWhiteSpace(user.Email) && !identity.HasClaim(c => c.Type == ClaimTypes.Email))
            {
                identity.AddClaim(new Claim(ClaimTypes.Email, user.Email));
            }

            // Ensure EmployeeId is added
            if (user.EmployeeId.HasValue && !identity.HasClaim(c => c.Type == "EmployeeId"))
            {
                identity.AddClaim(new Claim("EmployeeId", user.EmployeeId.Value.ToString()));
            }

            // Ensure MustChangePassword claim is added
            identity.AddClaim(new Claim("MustChangePassword", user.MustChangePassword.ToString().ToLower()));

            return identity;
        }
    }
}

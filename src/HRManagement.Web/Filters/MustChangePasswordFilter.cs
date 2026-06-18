using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace HRManagement.Web.Filters
{
    public class MustChangePasswordFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = context.HttpContext.User;
            if (user.Identity?.IsAuthenticated == true)
            {
                var mustChange = user.FindFirst("MustChangePassword")?.Value == "true";
                if (mustChange)
                {
                    var controller = context.RouteData.Values["controller"]?.ToString();
                    var action = context.RouteData.Values["action"]?.ToString();

                    // Allow ChangePasswordJson, ForceChangePassword, and Logout
                    bool isAllowed = (controller == "Profile" && (action == "ChangePasswordJson" || action == "ForceChangePassword")) ||
                                     (controller == "Account" && action == "Logout");

                    if (!isAllowed)
                    {
                        context.Result = new RedirectToActionResult("ForceChangePassword", "Profile", null);
                        return;
                    }
                }
            }

            await next();
        }
    }
}

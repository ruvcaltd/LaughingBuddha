using Microsoft.AspNetCore.Authorization;

namespace LAF.WebApi.Authorization
{
    public class DualAuthRequirement : IAuthorizationRequirement
    {
    }

    public class DualAuthHandler : AuthorizationHandler<DualAuthRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, DualAuthRequirement requirement)
        {
            // Accept both JWT and Azure AD authentication
            if (context.User.Identity?.IsAuthenticated == true)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
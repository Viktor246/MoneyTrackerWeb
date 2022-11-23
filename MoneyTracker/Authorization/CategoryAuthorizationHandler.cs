using Microsoft.AspNetCore.Authorization;
using MoneyTracker.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace MoneyTracker.Authorization
{
    public class CategoryAuthorizationHandler : AuthorizationHandler<IsOwnerRequirement, Category>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       IsOwnerRequirement requirement,
                                                       Category resource)
        {
            if (resource.OwnerId == null)
            {
                return Task.CompletedTask;
            }
            if (context.User.HasClaim(ClaimTypes.NameIdentifier, resource.OwnerId))
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}

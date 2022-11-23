using Microsoft.AspNetCore.Authorization;
using MoneyTracker.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace MoneyTracker.Authorization
{
    public class IncomeAuthorizationHandler : AuthorizationHandler<IsOwnerRequirement, Income>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       IsOwnerRequirement requirement,
                                                       Income resource)
        {
            if (context.User.HasClaim(ClaimTypes.NameIdentifier, resource.OwnerId))
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}

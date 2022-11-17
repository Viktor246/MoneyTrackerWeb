using Microsoft.AspNetCore.Authorization;

namespace MoneyTracker.Authorization
{
    public class IsOwnerRequirement : IAuthorizationRequirement
    {
    }
}

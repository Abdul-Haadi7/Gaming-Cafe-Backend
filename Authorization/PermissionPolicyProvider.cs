/*This class created the policy automatically. Instead of adding all policies in program.cs, we can use it to automatically 
identify and create policy. At the controller endpoint, we have [Authorize(Policy = "ploicyName")], this class creates policy
of that name*/

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace GAME_CAFE.Authorization;

public class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public PermissionPolicyProvider(
        IOptions<AuthorizationOptions> options)
        : base(options)
    {
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(
        string policyName)
    {
        Console.WriteLine("🔥 POLICY PROVIDER CALLED: " + policyName);
        // First check if ASP.NET already knows about this policy
        var policy = await base.GetPolicyAsync(policyName);

        if (policy != null)
        {
            Console.WriteLine("Policy already exists: " + policyName);
            return policy;
        }

        // If no policy exists, create one automatically
        return new AuthorizationPolicyBuilder()
            .AddRequirements(
                new PermissionRequirement(policyName)
            )
            .Build();
    }
}
//This class checks the token of user and identifies if the user has the required permission.
//It gets the required permission like CanAddAdmin etc from the PermissionHandler.cs file that checks user`s token for that permission in the claims.

using Microsoft.AspNetCore.Authorization;

namespace GAME_CAFE.Authorization;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Look for the permission in the user's token
        bool hasPermission = context.User.Claims.Any(
            claim =>
                claim.Type == "permission" &&
                claim.Value == requirement.Permission
        );

        if (hasPermission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
//This class gives us the required permission, like CanAddAdmin, CanViewGames etc.
//The handler uses it to know what is the permission required.

using Microsoft.AspNetCore.Authorization;

namespace GAME_CAFE.Authorization;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}
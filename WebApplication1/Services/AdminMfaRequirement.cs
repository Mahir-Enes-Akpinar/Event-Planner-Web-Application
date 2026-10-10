using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using WebApplication1.Models;

namespace WebApplication1.Services;

public sealed class AdminMfaRequirement : IAuthorizationRequirement;

public sealed class AdminMfaHandler : AuthorizationHandler<AdminMfaRequirement>
{
    private readonly UserManager<Kullanici> _userManager;
    public AdminMfaHandler(UserManager<Kullanici> userManager) => _userManager = userManager;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminMfaRequirement requirement)
    {
        var user = await _userManager.GetUserAsync(context.User);
        if (user is not null && await _userManager.IsInRoleAsync(user, "Admin") && user.TwoFactorEnabled)
            context.Succeed(requirement);
    }
}

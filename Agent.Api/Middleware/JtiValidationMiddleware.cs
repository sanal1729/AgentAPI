// <copyright file="JtiValidationMiddleware.cs" company="Agent">
// © Agent 2025
// </copyright>

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Agent.Domain.Entities;
using Microsoft.AspNetCore.Identity;

public class JtiValidationMiddleware : IMiddleware
{
    private readonly UserManager<User> _userManager;

    public JtiValidationMiddleware(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var jti = context.User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(jti))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("User not found.");
                    return;
                }

                var storedJti = await _userManager.GetAuthenticationTokenAsync(user, "AgentApp", "jti");

                if (storedJti != jti)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Token has been revoked.");
                    return;
                }
            }
        }

        await next(context);
    }
}

// <copyright file="HttpVerbPolicyMiddleware.cs" company="Agent">
// © Agent 2025
// </copyright>

using Microsoft.AspNetCore.Authorization;

public class HttpVerbPolicyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<HttpVerbPolicyMiddleware> _logger;

    public HttpVerbPolicyMiddleware(RequestDelegate next, ILogger<HttpVerbPolicyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();

        // Bypass for [AllowAnonymous]
        if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() is not null)
        {
            await _next(context);
            return;
        }

        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }

        // Bypass if explicit [Authorize(Roles=...)] or [Authorize(Policy=...)] is applied
        var authorizeAttributes = endpoint?.Metadata?.GetOrderedMetadata<IAuthorizeData>() ?? Enumerable.Empty<IAuthorizeData>();
        bool hasExplicitRoleOrPolicy = authorizeAttributes.Any(attr =>
            !string.IsNullOrWhiteSpace(attr.Roles) ||
            !string.IsNullOrWhiteSpace(attr.Policy));

        if (hasExplicitRoleOrPolicy)
        {
            _logger.LogDebug("Skipping verb policy check due to [Authorize(Roles/Policy=...)] on endpoint.");
            await _next(context);
            return;
        }

        // Determine required permission from HTTP method
        var requiredPermission = context.Request.Method.ToUpperInvariant() switch
        {
            "GET" => "CanRead",
            "POST" => "CanCreate",
            "PUT" => "CanUpdate",
            "PATCH" => "CanUpdate",
            "DELETE" => "CanDelete",
            _ => null,
        };

        if (requiredPermission is null)
        {
            // Skip check for unsupported HTTP verbs like OPTIONS, HEAD
            await _next(context);
            return;
        }

        // Extract permissions from JWT claims
        var userPermissions = context.User.FindAll("Permission").Select(p => p.Value);
        if (!userPermissions.Contains(requiredPermission))
        {
            _logger.LogWarning("User '{User}' lacks permission '{Permission}'", context.User.Identity?.Name, requiredPermission);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync($"Forbidden: Missing '{requiredPermission}' permission.");
            return;
        }

        await _next(context);
    }
}

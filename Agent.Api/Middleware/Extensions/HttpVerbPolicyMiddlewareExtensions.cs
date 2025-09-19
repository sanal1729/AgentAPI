// <copyright file="HttpVerbPolicyMiddlewareExtensions.cs" company="Agent">
// © Agent 2025
// </copyright>

public static class HttpVerbPolicyMiddlewareExtensions
{
    public static IApplicationBuilder UseHttpVerbPolicyMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<HttpVerbPolicyMiddleware>();
    }
}

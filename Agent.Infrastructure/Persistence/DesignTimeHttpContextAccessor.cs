// <copyright file="DesignTimeHttpContextAccessor.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Infrastructure.Persistence
{
    using Microsoft.AspNetCore.Http;

    public class DesignTimeHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; } = null;
    }
}

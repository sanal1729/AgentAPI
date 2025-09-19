// <copyright file="IJwtTokenHandler.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Common.Interfaces.Authentication
{
    using System.Security.Claims;
    using Agent.Domain.Entities;

    public interface IJwtTokenHandler
    {
        string GenerateAccessToken(User user, IEnumerable<Claim>? roles = null);

        (string RefreshToken, DateTime RefreshTokenExpires) GenerateRefreshToken();
    }
}

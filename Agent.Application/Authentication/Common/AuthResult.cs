// <copyright file="AuthResult.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Authentication.Common
{
    using Agent.Domain.Entities;

    public record AuthResult(
        User User,
        string? AccessToken,
        string? RefreshToken,
        long? RefreshTokenExpires);
}

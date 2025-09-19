// <copyright file="JwtSettings.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Infrastructure.Authentication
{
    public class JwtSettings
    {
        public const string SectionName = "JwtSettings";

        public string? Secret { get; init; } = null;

        public long AccessTokenExpiryInMinutes { get; init; }

        public long RefreshTokenExpiryInMinutes { get; init; }

        public string? Issuer { get; init; } = null;

        public string? Audience { get; init; } = null;
    }
}

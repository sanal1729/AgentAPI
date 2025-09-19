// <copyright file="AuthResponse.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Contracts.Authentication;

public record AuthResponse(
        Guid Id,
        string FirstName,
        string LastName,
        string Email,
        string AccessToken);

        // string RefreshToken

// <copyright file="RegisterRequest.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Contracts.Authentication;

public record RegisterRequest(
        string FirstName,
        string LastName,
        string Email,
        string Password);
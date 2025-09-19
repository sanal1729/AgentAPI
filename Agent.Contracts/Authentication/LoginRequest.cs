// <copyright file="LoginRequest.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Contracts.Authentication;

public record LoginRequest(
        string Email,
        string Password);


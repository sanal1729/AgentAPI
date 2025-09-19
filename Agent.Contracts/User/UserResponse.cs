// <copyright file="UserResponse.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Contracts.User
{
    using System;
    using System.Collections.Generic;

    public record UserResponse(
        string Id,
        string FirstName,
        string LastName,
        string Email,
        List<string> Roles);
}
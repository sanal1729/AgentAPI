// <copyright file="QueryOptions.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Contracts.Common;

public record QueryOptions
{
    public string? Filter { get; init; }

    public string? Sort { get; init; }

    public long PageNumber { get; init; } = 1;

    public long PageSize { get; init; } = long.MaxValue;

    public bool IncludeNavigations { get; init; } = true;
}

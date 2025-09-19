// <copyright file="AppDbSettings.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Infrastructure.Persistence;

public class AppDbSettings
{
    public const string SectionName = "AppDbSettings";

    public string ConnectionString { get; set; } = string.Empty;

    public int MaxRetryCount { get; set; } = 3;  // Default value

    public int CommandTimeout { get; set; } = 30;  // Default value in seconds
}


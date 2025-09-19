// <copyright file="IDateTimeProvider.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Common.Interfaces.Services
{
    using System;

    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }
    }
}

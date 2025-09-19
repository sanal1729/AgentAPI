// <copyright file="DateTimeProvider.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Infrastructure.Services
{
    using System;
    using Agent.Application.Common.Interfaces.Services;

    public class DateTimeProvider : IDateTimeProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeProvider"/> class.
        /// </summary>
        public DateTimeProvider()
        {
        }

        /// <inheritdoc/>
        public DateTime UtcNow => DateTime.UtcNow;
    }
}

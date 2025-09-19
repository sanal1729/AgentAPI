// <copyright file="IDomainEvent.cs" company="Agent">
// © Agent 2025
// </copyright>
namespace Agent.Domain.Common.Models
{
    using MediatR;

    /// <summary>
    /// Marker interface for domain events.
    /// </summary>
    public interface IDomainEvent : INotification
    {
    }
}
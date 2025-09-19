// <copyright file="IHasDomainEvents.cs" company="Agent">
// © Agent 2025
// </copyright>
namespace Agent.Domain.Common.Models
{
    using System.Collections.Generic;

    public interface IHasDomainEvents
    {
        IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

        void AddDomainEvent(IDomainEvent domainEvent);

        void RemoveDomainEvent(IDomainEvent domainEvent);

        void ClearDomainEvents();
    }
}
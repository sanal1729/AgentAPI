// <copyright file="Entity.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Domain.Common.Models
{
    public abstract class Entity<TId>(TId id) : IEquatable<Entity<TId>>, IHasDomainEvents

        where TId : ValueObject
    {
        private readonly List<IDomainEvent> _domainEvents = new();

        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public TId Id { get; protected set; } = id;

        public override bool Equals(object? obj)
        {
            return obj is Entity<TId> entity && this.Id.Equals(entity.Id);
        }

        public bool Equals(Entity<TId>? other)
        {
            return this.Equals((object?)other);
        }

        public static bool operator ==(Entity<TId> left, Entity<TId> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Entity<TId> left, Entity<TId> right)
        {
            return !Equals(left, right);
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        public void AddDomainEvent(IDomainEvent domainEvent)
        {
            if (domainEvent == null)
            {
                throw new ArgumentNullException(nameof(domainEvent));
            }

            _domainEvents.Add(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }

        public void RemoveDomainEvent(IDomainEvent domainEvent)
        {
            if (domainEvent == null)
            {
            throw new ArgumentNullException(nameof(domainEvent));
            }

            _domainEvents.Remove(domainEvent);
        }
    }
}

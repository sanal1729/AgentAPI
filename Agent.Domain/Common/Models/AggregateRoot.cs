// <copyright file="AggregateRoot.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Domain.Common.Models
{
    public class AggregateRoot<TId> : Entity<TId>

        where TId : ValueObject
    {
        protected AggregateRoot(TId id)
            : base(id)
        {
        }
    }
}

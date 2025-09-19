// <copyright file="OrganizationId.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Domain.Aggregates.Organization.ValueObjects
{
    using System;
    using Agent.Domain.Common.Models;

    public sealed class OrganizationId : ValueObject
    {
        public Guid Value { get; }

        // Parameterless constructor for EF Core
        public OrganizationId()
        {
            this.Value = Guid.CreateVersion7();  // Default value (can use Guid.Empty if preferred)
        }

        private OrganizationId(Guid value)
        {
            this.Value = value;
        }

        // Factory method to create a unique OrganizationId
        public static OrganizationId CreateUnique()
        {
            return new(Guid.CreateVersion7());
        }

        // Factory method to create OrganizationId from an existing Guid
        public static OrganizationId FromGuid(Guid value)
        {
            return new OrganizationId(value);
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return this.Value;
        }
    }
}

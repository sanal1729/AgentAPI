// <copyright file="Branch.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Domain.Aggregates.Organization.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Agent.Domain.Aggregates.Organization.ValueObjects;
    using Agent.Domain.Common.Models;

    public sealed class Branch : Entity<BranchId>
    {
        // Expose the Guid value for EF Core
        public Guid BranchIdGuid => this.Id.Value; // Expose the underlying Guid value

        // Public properties with setters for EF Core
        public string? Name { get; private set; } // Private setter to prevent modification outside the constructor

        public string? Code { get; private set; } // Private setter to prevent modification outside the constructor

        public OrganizationId? OrganizationId { get; private set; } // Foreign Key to Organization

        // Parameterless constructor required by EF Core
        // EF Core will use this constructor during materialization of the entity
        public Branch()
            : base(BranchId.CreateUnique()) // Calling base constructor with default BranchId
        {
        }

        // Private constructor for entity creation with parameters
        private Branch(BranchId id, string name, string code, OrganizationId? organizationId)
            : base(id)
        {
            this.Name = name;
            this.Code = code;
            this.OrganizationId = organizationId;
        }

        // Factory method to create new Branch instances
        public static Branch Create(BranchId? id, string name, string code, OrganizationId? organizationId)
        {
            // Use the provided BranchId or create a new one if it's null
            return new Branch(
                id ?? BranchId.CreateUnique(), // If id is null, create a unique BranchId
                name,
                code,
                organizationId);
        }

        public void Update(string name, string code)
        {
            this.Name = name;
            this.Code = code;
        }
    }
}

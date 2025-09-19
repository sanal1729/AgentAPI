// <copyright file="Organization.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Domain.Aggregates.Organization
{
    using System;
    using System.Collections.Generic;
    using Agent.Domain.Aggregates.Organization.Entities;
    using Agent.Domain.Aggregates.Organization.Events;
    using Agent.Domain.Aggregates.Organization.ValueObjects;
    using Agent.Domain.Common.Models;

    public sealed class Organization : AggregateRoot<OrganizationId>
    {
        // Expose the Guid value for EF Core
        public Guid OrganizationIdGuid => this.Id.Value;

        private readonly List<Branch> _branches = new();

        public string Name { get; private set; } = null!;

        public string CountryCode { get; private set; } = null!;

        public string CurrencyCode { get; private set; } = null!;

        // Navigation Property
        public IReadOnlyList<Branch> Branches => _branches.AsReadOnly();

        // EF Core requires a parameterless constructor
        private Organization()
            : base(OrganizationId.CreateUnique())
        {
        }

        // Private constructor used by the factory method
        private Organization(
            OrganizationId id,
            string name,
            string countryCode,
            string currencyCode)
            : base(id)
        {
            this.Name = name;
            this.CountryCode = countryCode;
            this.CurrencyCode = currencyCode;
        }

        // Factory method to ensure domain consistency
        public static Organization Create(
            OrganizationId? id,  // Optional OrganizationId parameter
            string name,
            string countryCode,
            string currencyCode,
            List<Branch>? branches = null)
        {
            // Use the provided OrganizationId or create a new one if it's null
            var organization = new Organization(
                id ?? OrganizationId.CreateUnique(),  // If id is null, create a unique OrganizationId
                name,
                countryCode,
                currencyCode);

            // If branches are provided, add them to the organization
            if (branches is not null)
            {
                organization._branches.AddRange(branches);
            }

            // Raise the OrganizationCreated event
            organization.AddDomainEvent(new OrganizationCreated(organization.Name, organization.CountryCode, organization.CurrencyCode));

            return organization;
        }

        // Optional: Add method to add branch after creation
        public void AddBranch(Branch branch)
        {
            _branches.Add(branch);
        }

        public void Update(string name, string countryCode, string currencyCode)
        {
            this.Name = name;
            this.CountryCode = countryCode;
            this.CurrencyCode = currencyCode;
        }

        public void UpdateBranches(List<Branch> branches)
        {
            this._branches.Clear();  // Clear existing branches
            this._branches.AddRange(branches);  // Add the updated branches
        }

        public void RemoveBranch(Branch branch)
        {
            this._branches.Remove(branch);  // Remove the branch from the list
        }
    }
}

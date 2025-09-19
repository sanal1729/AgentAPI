// <copyright file="OrganizationCreated.cs" company="Agent">
// © Agent 2025
// </copyright>
namespace Agent.Domain.Aggregates.Organization.Events
{
    using System;
    using Agent.Domain.Common.Models;

    public record OrganizationCreated(string Name, string CountryCode, string CurrencyCode) : IDomainEvent;
}
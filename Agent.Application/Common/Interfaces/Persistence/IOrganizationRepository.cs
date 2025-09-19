// <copyright file="IOrganizationRepository.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Common.Interfaces.Persistence;

using Agent.Domain.Aggregates.Organization;

public interface IOrganizationRepository
{
    void Add(Organization organization);
}
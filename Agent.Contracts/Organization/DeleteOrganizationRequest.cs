// <copyright file="DeleteOrganizationRequest.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Contracts.Organization
{
    public record DeleteOrganizationRequest(string Id, string Name, string CountryCode, string CurrencyCode, List<DeleteBranchRequest> Branches);

    public record DeleteBranchRequest(string Id, string Name, string Code);
}
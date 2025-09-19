// <copyright file="GetOrganizationRequest.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Contracts.Organization
{
    public record GetOrganizationRequest(string Id, string Name, string CountryCode, string CurrencyCode, List<UpdateBranchRequest> Branches);

    public record GetBranchRequest(string Id, string Name, string Code);
}

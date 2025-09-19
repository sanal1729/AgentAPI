// <copyright file="UpdateOrganizationRequest.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Contracts.Organization
{
    public record UpdateOrganizationRequest(string Id, string Name, string CountryCode, string CurrencyCode, List<UpdateBranchRequest> Branches);

    public record UpdateBranchRequest(string Id, string Name, string Code);
}

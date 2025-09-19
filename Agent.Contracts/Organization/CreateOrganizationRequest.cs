// <copyright file="CreateOrganizationRequest.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Contracts.Organization
{
    using System;

    public record CreateOrganizationRequest(
        string Name, string CountryCode, string CurrencyCode, List<CreateBranchRequest> Branches);

    public record CreateBranchRequest(string Name, string Code);
}

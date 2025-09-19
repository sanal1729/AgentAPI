// <copyright file="OrganizationResponse.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Contracts.Organization
{
    using System;

    public record OrganizationResponse(long RowNumber, string Id, string Name, string CountryCode, string CurrencyCode, List<BranchResponse> Branches);

    public record BranchResponse(long RowNumber, string Id, string Name, string Code);
}


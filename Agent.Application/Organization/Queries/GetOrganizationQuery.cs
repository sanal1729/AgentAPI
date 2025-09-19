// <copyright file="GetOrganizationQuery.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Organization.Queries
{
    using Agent.Application.Organization.Common;
    using Agent.Domain.Aggregates.Organization;
    using ErrorOr;
    using MediatR;

    public record GetOrganizationQuery(string Id, string Name, string CountryCode, string CurrencyCode, List<GetBranchQuery> Branches) : IRequest<ErrorOr<Organization>>;

    public record GetBranchQuery(string Id, string Name, string Code);
}
// <copyright file="GetOrganizationsQuery.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Organization.Queries
{
    using Agent.Domain.Aggregates.Organization;
    using ErrorOr;
    using MediatR;

    public record GetOrganizationsQuery(string? Filter, string? Sort, long PageNumber, long PageSize, bool IncludeNavigations) : IRequest<ErrorOr<(IReadOnlyList<Organization> Organizations, long TotalCount)>>;
}

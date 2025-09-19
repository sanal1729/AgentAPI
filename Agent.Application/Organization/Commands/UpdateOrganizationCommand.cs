// <copyright file="UpdateOrganizationCommand.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Organization.Commands
{
    using Agent.Domain.Aggregates.Organization;
    using ErrorOr;
    using MediatR;

    public record UpdateOrganizationCommand(string Id, string Name, string CountryCode, string CurrencyCode, List<UpdateBranchCommand>? Branches) : IRequest<ErrorOr<Organization>>;

    public record UpdateBranchCommand(string Id, string Name, string Code);
}
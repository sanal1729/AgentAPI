// <copyright file="DeleteOrganizationCommand.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Organization.Commands
{
    using Agent.Domain.Aggregates.Organization;
    using ErrorOr;
    using MediatR;

    public record DeleteOrganizationCommand(string Id, string Name, string CountryCode, string CurrencyCode, List<DeleteBranchCommand> Branches) : IRequest<ErrorOr<Organization>>;

    public record DeleteBranchCommand(string Id, string Name, string Code);
}
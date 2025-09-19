// <copyright file="CreateOrganizationCommand.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Organization.Commands
{
    using System;
    using Agent.Domain.Aggregates.Organization;
    using ErrorOr;
    using MediatR;

    public record CreateOrganizationCommand(
        string Name, string CountryCode, string CurrencyCode, List<CreateBranchCommand> Branches) : IRequest<ErrorOr<Organization>>;

    public record CreateBranchCommand(string Name, string Code);
}

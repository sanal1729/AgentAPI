// <copyright file="OrganizationMappingConfig.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Api.Common.Mapping
{
    using Agent.Application.Organization.Commands;
    using Agent.Contracts.Organization;
    using Agent.Domain.Aggregates.Organization;
    using Agent.Domain.Aggregates.Organization.Entities;
    using Mapster;

    public class OrganizationMappingConfig : IRegister
    {
        public OrganizationMappingConfig()
        {
        }

        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CreateOrganizationRequest, CreateOrganizationCommand>()
                .Map(d => d, s => s);

            config.NewConfig<Organization, OrganizationResponse>()
            .Map(d => d.Id, s => s.Id.Value)
            .Map(d => d.Branches, s => s.Branches);

            config.NewConfig<Branch, BranchResponse>()
            .Map(d => d.Id, s => s.Id.Value);
        }
    }
}

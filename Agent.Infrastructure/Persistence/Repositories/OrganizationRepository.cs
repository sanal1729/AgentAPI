// <copyright file="OrganizationRepository.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Infrastructure.Persistence.Repositories
{
    using Agent.Application.Common.Interfaces.Persistence;
    using Agent.Domain.Aggregates.Organization;
    using Agent.Domain.Entities;

    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly AppDbContext _appDbContext;

        public OrganizationRepository(AppDbContext appDbContext)
        {
             _appDbContext = appDbContext;
        }

        private static readonly List<Organization> _organizations = new();

        public void Add(Organization organization)
        {
            _appDbContext.Add(organization);
            _appDbContext.SaveChanges();

            _organizations.Add(organization);
        }
    }
}

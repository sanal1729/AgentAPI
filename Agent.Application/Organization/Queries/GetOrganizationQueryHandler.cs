// <copyright file="GetOrganizationQueryHandler.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Organization.Queries
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Agent.Application.Common.Interfaces.Persistence;
    using Agent.Application.Organization.Common;
    using Agent.Domain.Aggregates.Organization;
    using Agent.Domain.Aggregates.Organization.ValueObjects;
    using ErrorOr;
    using MediatR;

    public class GetOrganizationQueryHandler : IRequestHandler<GetOrganizationQuery, ErrorOr<Organization>>
    {
        private readonly IUnitOfWork _unitOfWork;

          // Using expression-bodied constructor
        public GetOrganizationQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<ErrorOr<Organization>> Handle(GetOrganizationQuery request, CancellationToken cancellationToken)
        {
            var organizationRepository = _unitOfWork.GetRepository<Organization>();

            var organization = Organization.Create(
                id: OrganizationId.FromGuid(new Guid(request.Id)),
                name: request.Name,
                countryCode: request.CountryCode,
                currencyCode: request.CurrencyCode);

            // Fetch the organization by its ID from the repository
            var orgEntity = await organizationRepository.GetByIdAsync(organization, true, cancellationToken);

            // Return error if organization not found
            if (orgEntity is null)
            {
                return Error.NotFound("Organization not found.");
            }

            return orgEntity;
        }
    }
}

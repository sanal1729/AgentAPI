// <copyright file="CreateOrganizationCommandHandler.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Organization.Commands
{
    using Agent.Application.Common.Interfaces.Persistence;
    using Agent.Domain.Aggregates.Organization;
    using Agent.Domain.Aggregates.Organization.Entities;
    using ErrorOr;
    using MediatR;

    public class CreateOrganizationCommandHandler : IRequestHandler<CreateOrganizationCommand, ErrorOr<Organization>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateOrganizationCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ErrorOr<Organization>> Handle(CreateOrganizationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                var organization = Organization.Create(
                    id: null,
                    name: request.Name,
                    countryCode: request.CountryCode,
                    currencyCode: request.CurrencyCode,
                    branches: request.Branches.Select(branch => Branch.Create(
                        id: null,
                        name: branch.Name,
                        code: branch.Code,
                        organizationId: null)).ToList());

                var organizationRepository = _unitOfWork.GetRepository<Organization>();
                await organizationRepository.AddAsync(organization, cancellationToken);

                 // Finally, call SaveChangesAsync from UnitOfWork.
                // await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return organization;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Error.Failure(description: $"Failed to create organization. {ex.Message}");
            }
        }
    }
}

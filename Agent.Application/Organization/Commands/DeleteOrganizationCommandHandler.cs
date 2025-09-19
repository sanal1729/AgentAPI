// <copyright file="DeleteOrganizationCommandHandler.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Organization.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Agent.Application.Common.Interfaces.Persistence;
    using Agent.Domain.Aggregates.Organization;
    using Agent.Domain.Aggregates.Organization.ValueObjects;
    using ErrorOr;
    using MediatR;

    public class DeleteOrganizationCommandHandler : IRequestHandler<DeleteOrganizationCommand, ErrorOr<Organization>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteOrganizationCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ErrorOr<Organization>> Handle(DeleteOrganizationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Start the transaction
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                // Create the Organization entity from the command
                var organization = Organization.Create(
                    id: OrganizationId.FromGuid(new Guid(request.Id)),
                    name: request.Name,
                    countryCode: request.CountryCode,
                    currencyCode: request.CurrencyCode);

                // Delete the organization using the repository within the transaction scope
                var organizationRepository = _unitOfWork.GetRepository<Organization>();
                bool deleted = await organizationRepository.DeleteByIdAsync(organization, true, cancellationToken);

                // Finally, call SaveChangesAsync from UnitOfWork.
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                if (!deleted)
                {
                    // Rollback if deletion fails
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                    // Return a list of errors
                    return ErrorOr<Organization>.From(new List<Error>
                    {
                        Error.Validation("DeletionFailed", "The organization could not be deleted."),
                    });
                }

                // Commit the transaction
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return organization; // Return the deleted organization entity
            }
            catch (Exception ex)
            {
                // In case of an exception, rollback the transaction
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                // Return a list of errors
                return ErrorOr<Organization>.From(new List<Error>
                {
                    Error.Unexpected(ex.Message),
                });
            }
        }
    }
}

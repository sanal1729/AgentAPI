// <copyright file="UpdateOrganizationCommandHandler.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Organization.Commands
{
    using Agent.Application.Common.Interfaces.Persistence;
    using Agent.Domain.Aggregates.Organization;
    using Agent.Domain.Aggregates.Organization.Entities;
    using Agent.Domain.Aggregates.Organization.ValueObjects;
    using ErrorOr;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata;
    using Microsoft.Extensions.Logging;

    public class UpdateOrganizationCommandHandler : IRequestHandler<UpdateOrganizationCommand, ErrorOr<Organization>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateOrganizationCommandHandler> _logger;

        public UpdateOrganizationCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateOrganizationCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ErrorOr<Organization>> Handle(UpdateOrganizationCommand request, CancellationToken cancellationToken)
        {
            bool success = false;

            try
            {
                if (request is null)
                {
                    throw new ArgumentNullException(nameof(request), "Request cannot be null.");
                }

                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                var branches = request?.Branches?
                                .Select(req => Branch.Create(
                                    id: string.IsNullOrEmpty(req.Id) ? BranchId.CreateUnique() : BranchId.FromGuid(Guid.Parse(req.Id)),
                                    name: req.Name,
                                    code: req.Code,
                                    organizationId: OrganizationId.FromGuid(Guid.Parse(request.Id))))
                                .ToList() ?? new List<Branch>();

                var organization = Organization.Create(
                    id: request?.Id is not null ? OrganizationId.FromGuid(Guid.Parse(request.Id)) : OrganizationId.CreateUnique(),
                    name: request?.Name ?? throw new ArgumentNullException(nameof(request.Name), "Organization name cannot be null."),
                    countryCode: request?.CountryCode ?? throw new ArgumentNullException(nameof(request.CountryCode), "Country code cannot be null."),
                    currencyCode: request?.CurrencyCode ?? throw new ArgumentNullException(nameof(request.CurrencyCode), "Currency code cannot be null."),
                    branches: branches);

                var organizationRepository = _unitOfWork.GetRepository<Organization>();
                bool updated = await organizationRepository.UpdateAsync(organization, true, cancellationToken);

                success = true;
                return organization;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update organization with ID {OrganizationId}", request.Id);
                return Error.Failure("Organization.UpdateError", $"Failed to update organization. {ex.Message}");
            }
            finally
            {
                if (success)
                {
                    await _unitOfWork.CommitTransactionAsync(cancellationToken);
                }
                else
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                }
            }
        }

        private static bool KeysMatch<T>(T entity, T other, IReadOnlyList<IProperty> keyProperties)
        {
            foreach (var keyProp in keyProperties)
            {
                var val1 = keyProp.PropertyInfo?.GetValue(entity);
                var val2 = keyProp.PropertyInfo?.GetValue(other);

                if (!Equals(val1, val2))
                {
                    return false;
                }
            }

            return true;
        }
    }
}

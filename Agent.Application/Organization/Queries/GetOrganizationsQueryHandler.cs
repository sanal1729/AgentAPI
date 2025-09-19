// <copyright file="GetOrganizationsQueryHandler.cs" company="Agent">
// © Agent 2025
// </copyright>

using Agent.Application.Common.Interfaces.Persistence;
using Agent.Application.Common.Interfaces.Services;
using Agent.Application.Organization.Queries;
using Agent.Domain.Aggregates.Organization;
using ErrorOr;
using MediatR;

public class GetOrganizationsQueryHandler : IRequestHandler<GetOrganizationsQuery, ErrorOr<(IReadOnlyList<Organization> Organizations, long TotalCount)>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IQueryBuilder<Organization> _queryBuilder;

    public GetOrganizationsQueryHandler(IUnitOfWork unitOfWork, IQueryBuilder<Organization> queryBuilder)
    {
        _unitOfWork = unitOfWork;
        _queryBuilder = queryBuilder;
    }

    public async Task<ErrorOr<(IReadOnlyList<Organization> Organizations, long TotalCount)>> Handle(GetOrganizationsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var organizationsRepository = _unitOfWork.GetRepository<Organization>();
            var queryBuilder = _queryBuilder.GetExpression(request.Filter, request.Sort);

            (IReadOnlyList<Organization> organizations, long totalCount) =
             await organizationsRepository.GetPagedAsync(
                predicate: queryBuilder.Predicate,
                orderBy: queryBuilder.OrderBy,
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                includeNavigations: request.IncludeNavigations,
                cancellationToken: cancellationToken);

            // If no organizations are found, return an empty list with a total count of 0
            if (totalCount == 0)
            {
                return (Array.Empty<Organization>().ToList().AsReadOnly(), 0L);
            }

            // Return the list of organizations and the total count
            return (organizations.ToList().AsReadOnly(), totalCount);
        }
        catch (Exception)
        {
            return (Array.Empty<Organization>().ToList().AsReadOnly(), 0L);
        }
    }
}



// <copyright file="OrganizationController.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Api.Controllers
{
    using System.Threading.Tasks;
    using Agent.Api.ModelBinders;
    using Agent.Application.Organization.Commands;
    using Agent.Application.Organization.Queries;
    using Agent.Contracts.Common;
    using Agent.Contracts.Organization;
    using Agent.Domain.Aggregates.Organization;
    using ErrorOr;
    using MapsterMapper;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Route("organization")]

    // [AllowAnonymous]
    public class OrganizationController : ApiController
    {
        private readonly ISender _mediator;

        private readonly IMapper _mapper;

        public OrganizationController(ISender mediator, IMapper mapper) => (this._mediator, this._mapper) = (mediator, mapper);

        [HttpGet("list")]
        public async Task<IActionResult> GetOrganizations(
            [ModelBinder(BinderType = typeof(ModelBinder<QueryOptions>))] QueryOptions orgQueryOptions)
        {
            var query = this._mapper.Map<GetOrganizationsQuery>(orgQueryOptions);

            ErrorOr<(IReadOnlyList<Organization> Organizations, long TotalCount)> result = await _mediator.Send(query);

            return result.Match(
               org => Ok(new
               {
                   Organizations = _mapper.Map<IReadOnlyList<OrganizationResponse>>(org.Organizations),

                // .Select((o, index) => o with
                //    {
                //        RowNumber = index + 1,
                //        Branches = o.Branches
                //    .Select((b, branchIndex) => b with { RowNumber = branchIndex + 1 })
                //    .ToList(),
                //    }).ToList(),
                   TotalCount = org.TotalCount,
               }),
               error => Problem(error));
        }

        [HttpPost]

        public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganizationRequest request)
        {
            var command = this._mapper.Map<CreateOrganizationCommand>(request);
            var createOrgResult = await this._mediator.Send(command);

            return createOrgResult.Match(
                org => Ok(_mapper.Map<OrganizationResponse>(org)),
                error => Problem(error));
        }

        // GET: /organization
        [HttpGet]
        public async Task<IActionResult> GetOrganization([ModelBinder] GetOrganizationRequest request)
        {
            // Send the query to get the organization by keys
            var query = this._mapper.Map<GetOrganizationQuery>(request);
            var result = await _mediator.Send(query);

            // return result.Match(
            //     org => Ok(_mapper.Map<OrganizationResponse>(org)),
            //     error => Problem(error));
            var organizationResponse = _mapper.Map<OrganizationResponse>(result.Value);
            return result.Match(
                org => Ok(organizationResponse),  // Return OK with the organization response
                error => Problem(error)); // Return 404 if not found
        }

        // PUT: /organization/{id}
        [HttpPut]
        public async Task<IActionResult> UpdateOrganization([FromBody] UpdateOrganizationRequest request)
        {
            var command = this._mapper.Map<UpdateOrganizationCommand>(request);
            var updateOrgResult = await this._mediator.Send(command);

            return updateOrgResult.Match(
                org => Ok(_mapper.Map<OrganizationResponse>(org)), // Successfully updated
                error => Problem(error)); // Error handling
        }

        // DELETE: /organization
        [HttpDelete]
        public async Task<IActionResult> DeleteOrganization([FromQuery] DeleteOrganizationRequest request)
        {
            var command = this._mapper.Map<DeleteOrganizationCommand>(request);
            var deleteOrgResult = await _mediator.Send(command);

            return deleteOrgResult.Match(
                _ => NoContent(), // Successfully deleted
                error => Problem(error)); // Error handling
        }
    }
}

// <copyright file="OrganizationCreatedHandler.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Organization.Events
{
    using System.Threading;
    using System.Threading.Tasks;
    using Agent.Application.Common;
    using Agent.Application.Common.Interfaces.Services;
    using Agent.Domain.Aggregates.Organization.Events;
    using MediatR;

    public class OrganizationCreatedHandler : INotificationHandler<OrganizationCreated>
    {
       private readonly IPublisher<OrganizationCreated> _publisher;

       public OrganizationCreatedHandler(IPublisher<OrganizationCreated> publisher)
        {
            _publisher = publisher;
        }

       public async Task Handle(OrganizationCreated notification, CancellationToken cancellationToken)
        {
           await _publisher.PublishAsync(notification, cancellationToken);
        }
    }
}
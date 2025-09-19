// <copyright file="PublishDomainEventInterceptor.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Infrastructure.Persistence.Interceptors
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Agent.Domain.Common;
    using Agent.Domain.Common.Models;
    using Agent.Infrastructure.Services;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;

    public class PublishDomainEventInterceptor : SaveChangesInterceptor
    {
        private readonly IPublisher _domainEventPublisher;

        public PublishDomainEventInterceptor(IPublisher domainEventPublisher)
        {
            _domainEventPublisher = domainEventPublisher;
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context != null)
            {
                await PublishDomainEvents(context, cancellationToken);
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private async Task PublishDomainEvents(DbContext context, CancellationToken cancellationToken)
        {
             var entitiesWithDomainEvents = context.ChangeTracker
                .Entries<IHasDomainEvents>()
                .Select(e => e.Entity)
                .Where(e => e.DomainEvents != null && e.DomainEvents.Any())
                .ToList();

             var domainEvents = entitiesWithDomainEvents
                .SelectMany<IHasDomainEvents, IDomainEvent>(e => e.DomainEvents)
                .ToList();

             foreach (var entity in entitiesWithDomainEvents)
            {
                entity.ClearDomainEvents();
            }

             foreach (var domainEvent in domainEvents)
            {
                await _domainEventPublisher.Publish(domainEvent, cancellationToken);
            }
        }
    }
}
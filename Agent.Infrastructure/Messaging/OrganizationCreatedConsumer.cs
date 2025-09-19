// <copyright file="OrganizationCreatedConsumer.cs" company="Agent">
// © Agent 2025
// </copyright>
namespace Agent.Infrastructure.Messaging
{
    using Agent.Application.Common.Interfaces.Services;
    using Agent.Domain.Aggregates.Organization.Events;
    using Microsoft.Extensions.Logging;

    public class OrganizationCreatedConsumer : RabbitMqConsumer<OrganizationCreated>
    {
        private static string GetQueueName() =>
        typeof(OrganizationCreated).Name.ToLower() + "_queue";

        public OrganizationCreatedConsumer(ILogger<OrganizationCreatedConsumer> logger, IMessageConnection messageConnection)
            : base(logger, GetQueueName(),  messageConnection)
        {
        }

        protected override Task HandleMessageAsync(OrganizationCreated message)
        {
            Console.WriteLine($"Received OrganizationCreated event: {message.Name}");
            return Task.CompletedTask;
        }
    }
}
// <copyright file="RabbitMqPublisher.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Infrastructure.Messaging
{
    using System.Text;
    using System.Text.Json;
    using Agent.Application.Common.Interfaces.Services;
    using RabbitMQ.Client;

    public class RabbitMqPublisher<T> : IPublisher<T>
{
    private readonly IModel _channel;
    private readonly string _queueName;

    public RabbitMqPublisher(string queueName = null!, IMessageConnection messageConnection = null!)
    {
        _queueName = queueName ?? typeof(T).Name.ToLower() + "_queue";

        _channel = messageConnection.GetChannel();

        _channel?.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
    }

    public Task PublishAsync(T message, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var props = _channel.CreateBasicProperties();
            props.Persistent = true;

            props.ReplyTo = "ack_" + typeof(T).Name.ToLower() + "reply_queue";
            props.CorrelationId = Guid.CreateVersion7().ToString();

            _channel.BasicPublish(
                exchange: string.Empty,
                routingKey: _queueName,
                basicProperties: props,
                body: body);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // Log or wrap exception
            throw new InvalidOperationException($"Failed to publish message to RabbitMQ: {ex.Message}", ex);
        }
    }
}
}
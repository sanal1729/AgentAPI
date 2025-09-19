// <copyright file="RabbitMqConsumer.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Infrastructure.Messaging
{
    using System;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Agent.Application.Common.Interfaces.Services;
    using Microsoft.Extensions.Logging;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;

    public abstract class RabbitMqConsumer<T> : IConsumer<T>
    {
        private readonly ILogger _logger;
        private readonly string _queueName;

        private readonly IModel? _channel;

        protected RabbitMqConsumer(ILogger logger, string queueName, IMessageConnection messageConnection)
        {
            _logger = logger;
            _queueName = queueName;
            _channel = messageConnection.GetChannel();
            if (_channel == null)
            {
                _logger.LogError("The channel is not initialized.");
            }

            _channel?.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        }

        public async Task ConsumeAsync(CancellationToken cancellationToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += async (sender, args) =>
            {
                var body = args.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);
                var props = args.BasicProperties;

                try
                {
                    var message = JsonSerializer.Deserialize<T>(messageJson);
                    if (message != null)
                    {
                        await HandleMessageAsync(message);
                        if (_channel != null)
                        {
                            _channel.BasicAck(args.DeliveryTag, multiple: false);
                        }

                        // Optional: Send reply/ack back to publisher
                        if (!string.IsNullOrEmpty(props.ReplyTo))
                        {
                            var replyBody = Encoding.UTF8.GetBytes("Processed");

                            var replyProps = _channel?.CreateBasicProperties();
                            if (replyProps == null)
                            {
                                _logger.LogWarning("Failed to create basic properties as the channel is null.");
                                return;
                            }

                            replyProps.CorrelationId = props.CorrelationId;

                            _channel.BasicPublish(
                                exchange: string.Empty,
                                routingKey: props.ReplyTo,
                                basicProperties: replyProps,
                                body: replyBody);

                            _logger.LogInformation("Sent acknowledgment to publisher on queue: {ReplyTo}", props.ReplyTo);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Invalid message format.");

                        if (_channel != null)
                        {
                            _channel?.BasicNack(args.DeliveryTag, false, false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message.");
                    _channel?.BasicNack(args.DeliveryTag, false, false);
                }
            };

            _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);
            _logger.LogInformation("Started consuming from queue: {Queue}", _queueName);

            // Keep the consumer alive until cancellation
            cancellationToken.Register(() =>
            {
                _logger.LogInformation("Cancellation requested, shutting down consumer...");
                _channel?.Close(); // optional: only if channel should be closed
                _channel?.Dispose();
            });

            // Await until cancellation is requested to keep the method asynchronous
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }

        protected abstract Task HandleMessageAsync(T message);
    }
}
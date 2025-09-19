// <copyright file="MessageConnection.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Infrastructure.Messaging
{
    using System;
    using System.Threading;
    using Agent.Application.Common.Interfaces.Services;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Exceptions;

    public class MessageConnection : IMessageConnection, IDisposable
    {
        private readonly IOptionsMonitor<RabbitMqSettings> _settingsMonitor;
        private readonly ILogger<MessageConnection> _logger;
        private readonly object _lock = new();

        private IConnection? _connection;
        private IModel? _channel;

        public MessageConnection(
            IOptionsMonitor<RabbitMqSettings> settingsMonitor,
            ILogger<MessageConnection> logger)
        {
            _settingsMonitor = settingsMonitor;
            _logger = logger;

            InitializeConnection(_settingsMonitor.CurrentValue);

            _settingsMonitor.OnChange(settings =>
            {
                lock (_lock)
                {
                    DisposeConnection();
                    InitializeConnection(settings);
                }
            });
        }

        public IConnection? GetConnection()
        {
            if (_connection == null || !_connection.IsOpen)
            {
                _logger.LogWarning("RabbitMQ connection is not available.");
                return null;
            }

            return _connection;
        }

        public IModel? GetChannel()
        {
            if (_channel == null || _channel.IsClosed)
            {
                _logger.LogWarning("RabbitMQ channel is not available.");
                return null;
            }

            return _channel;
        }

        public void Dispose()
        {
            lock (_lock)
            {
                DisposeConnection();
            }
        }

        private void DisposeConnection()
        {
            try
            {
                _channel?.Close();
                _channel?.Dispose();
                _connection?.Close();
                _connection?.Dispose();
                _logger.LogInformation("RabbitMQ connection and channel disposed.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error occurred while disposing RabbitMQ connection.");
            }
            finally
            {
                _channel = null;
                _connection = null;
            }
        }

        private void InitializeConnection(RabbitMqSettings settings)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = settings.HostName,
                    Port = settings.Port,
                    UserName = settings.UserName,
                    Password = settings.Password,
                    DispatchConsumersAsync = true,
                };

                _logger.LogInformation(
                    "Attempting to connect to RabbitMQ at {Host}:{Port}...",
                    settings.HostName,
                    settings.Port);

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _logger.LogInformation("Successfully connected to RabbitMQ.");
            }
            catch (BrokerUnreachableException ex)
            {
                _logger.LogError(
                    ex,
                    "RabbitMQ broker unreachable at {Host}:{Port}. Please ensure RabbitMQ is running and accessible.",
                    settings.HostName,
                    settings.Port);
                _connection = null;
                _channel = null;

                // throw; // Fail fast
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while connecting to RabbitMQ at {Host}:{Port}.",
                    settings.HostName,
                    settings.Port);
                _connection = null;
                _channel = null;

                // throw;
            }
        }
    }
}

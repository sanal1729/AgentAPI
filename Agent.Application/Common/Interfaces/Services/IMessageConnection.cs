// <copyright file="IMessageConnection.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Common.Interfaces.Services
{
    using RabbitMQ.Client;

    public interface IMessageConnection
    {
        IConnection? GetConnection();

        IModel? GetChannel();
    }
}
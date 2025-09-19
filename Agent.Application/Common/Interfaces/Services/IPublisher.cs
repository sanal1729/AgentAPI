// <copyright file="IPublisher.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Common.Interfaces.Services
{
    public interface IPublisher<T>
    {
        Task PublishAsync(T message, CancellationToken cancellationToken = default);
    }
}
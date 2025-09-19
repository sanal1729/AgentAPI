// <copyright file="IConsumer.cs" company="Agent">
// © Agent 2025
// </copyright>
namespace Agent.Application.Common.Interfaces.Services
{
    public interface IConsumer<T>
    {
        Task ConsumeAsync(CancellationToken cancellationToken);
    }
}
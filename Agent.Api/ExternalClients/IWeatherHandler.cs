// <copyright file="IWeatherHandler.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Api.ExternalClients
{
    public interface IWeatherHandler
    {
        Task<string> Get(string city);
    }
}

// <copyright file="WeatherClient.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Api.ExternalClients
{
    public class WeatherClient
    {
        public const string SectionName = "WeatherClient";

        public string? BaseUrl { get; init; }

        public string? Key { get; init; }
    }
}

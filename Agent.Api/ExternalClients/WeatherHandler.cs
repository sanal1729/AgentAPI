// <copyright file="WeatherHandler.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Api.ExternalClients
{
    using Microsoft.Extensions.Options;

    public class WeatherHandler(IOptions<WeatherClient> weatherClient, HttpClient httpClient) : IWeatherHandler
    {
        private readonly WeatherClient weatherClient = weatherClient.Value;
        private HttpClient httpClient = httpClient;

        public async Task<string> Get(string city)
        {
            var url = $"?key={this.weatherClient.Key}&q={city}&aqi=yes";

            // var httpClient = httpClientFactory.CreateClient("weather");
            var response = await this.httpClient.GetAsync(url);
            return await response.Content.ReadAsStringAsync();
        }
    }
}

// <copyright file="WeatherController.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Api.Controllers
{
    using System.Threading.Tasks;
    using Agent.Api.ExternalClients;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;

    [Route("weather")]

    // [AllowAnonymous]
    public class WeatherController(
        IWeatherHandler weatherHandler, // IHttpClientFactory httpClientFactory,
        IOptions<WeatherClient> weatherClient) : ApiController
    {
        // private IHttpClientFactory httpClientFactory;
        private readonly IWeatherHandler weatherHandler = weatherHandler;
        private readonly WeatherClient weatherClient = weatherClient.Value;

        [HttpGet]

        public async Task<ActionResult> Get(string city)
        {
            var url = $"?key={this.weatherClient.Key}&q={city}&aqi=yes";

            // var httpClient = httpClientFactory.CreateClient(WeatherClient.SectionName);

            // var response = await httpClient.GetStreamAsync(URL);
            var response = await this.weatherHandler.Get(city);

            return this.Ok(response);
        }
    }
}

// <copyright file="DependencyInjection.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Api
{
    using System.IO.Compression;
    using System.Net.Http;
    using Agent.Api.Common.Errors;
    using Agent.Api.Common.Mapping;
    using Agent.Api.ExternalClients;
    using Agent.Api.HostedServices;
    using Agent.Api.ModelBinders;
    using Agent.Application.Common;
    using Agent.Application.Common.Interfaces.Services;
    using Agent.Domain.Aggregates.Organization.Events;
    using Agent.Infrastructure.Messaging;
    using Agent.Infrastructure.Services;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.ResponseCompression;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Polly;

    public static class DependencyInjection
    {
        public static IServiceCollection AddPresentation(this IServiceCollection services, ConfigurationManager configuration)
        {
            services.AddHsts(opt =>
            {
                opt.Preload = true;
                opt.IncludeSubDomains = true;
                opt.MaxAge = TimeSpan.FromDays(180); // 6 months recommended
                opt.ExcludedHosts.Add("localhost"); // Exclude localhost in development
            });

            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
                options.HttpsPort = 7263;
            });

            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/json" });
            });

            services.Configure<BrotliCompressionProviderOptions>(
                options =>
                {
                    options.Level = CompressionLevel.SmallestSize;
                });

            services.AddControllers(options =>
            {
                options.ModelBinderProviders.Insert(0, new ModelBinderProvider());
            });

            services.AddTransient<JtiValidationMiddleware>();

            var weatherClient = new WeatherClient();
            configuration.Bind(WeatherClient.SectionName, weatherClient);
            services.AddSingleton(Options.Create(weatherClient));

            services.Configure<WeatherClient>(configuration.GetSection(WeatherClient.SectionName));

            services.AddHttpClient<IWeatherHandler, WeatherHandler>(c =>
            {
                if (!Uri.TryCreate(weatherClient.BaseUrl, UriKind.Absolute, out Uri? baseUri))
                {
                    throw new ArgumentException("Invalid or null BaseUrl.");
                }

                c.BaseAddress = new Uri(weatherClient.BaseUrl);
                c.DefaultRequestHeaders.Add("Accept", "application/json");
            })

            // Retry Policy: Retry 3 times with exponential backoff (e.g., 2, 4, 8 seconds)
            .AddTransientHttpErrorPolicy(policyBuilder =>
                policyBuilder.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))) // Exponential backoff

            // Circuit Breaker Policy: Break the circuit after 5 failures within 10 seconds
            .AddTransientHttpErrorPolicy(policyBuilder =>
                policyBuilder.CircuitBreakerAsync(5, TimeSpan.FromSeconds(10))) // Circuit breaker

            // Timeout Policy: Apply a timeout of 10 seconds for each request
            .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10))) // Timeout after 10 seconds

            // Bulkhead Isolation: Limit concurrency to 10 requests, queue up to 20 requests
            .AddPolicyHandler(Policy.BulkheadAsync<HttpResponseMessage>(10, 20)) // Bulkhead isolation

            ;

            // services.AddHttpClient(WeatherClient.SectionName, c =>
            // {
            //    c.BaseAddress = new Uri(weatherClient.BaseUrl);
            // });
            services.AddSingleton<ProblemDetailsFactory, ExceptionDetailsFactory>();
            services.AddMappings();

            services.AddHostedService<QueueHostedService>();
            services.AddLogging();

            return services;
        }
    }
}

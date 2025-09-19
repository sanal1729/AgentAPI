// <copyright file="QueueHostedService.cs" company="Agent">
// © Agent 2025
// </copyright>
namespace Agent.Api.HostedServices;

using Agent.Application.Common;
using Agent.Application.Common.Interfaces.Services;
using Agent.Domain.Aggregates.Organization.Events;

public class QueueHostedService : BackgroundService
{
    private readonly IConsumer<OrganizationCreated> _consumer;
    private readonly ILogger<QueueHostedService> _logger;

    public QueueHostedService(IConsumer<OrganizationCreated> consumer, ILogger<QueueHostedService> logger)
    {
        _consumer = consumer;
        _logger = logger;
    }

    // protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    // {
    //     _logger.LogInformation("QueueHostedService is starting...");

    // while (!stoppingToken.IsCancellationRequested)
    //     {
    //         try
    //         {
    //             // Consume a message (or batch) here
    //             // Assuming this is an async, cancellable method
    //             await Task.Run(() => _consumer.ConsumeAsync(stoppingToken), stoppingToken);
    //         }
    //         catch (OperationCanceledException)
    //         {
    //             _logger.LogInformation("EmailQueueHostedService cancellation requested.");
    //             break;
    //         }
    //         catch (Exception ex)
    //         {
    //             _logger.LogError(ex, "An error occurred while consuming email messages.");
    //             await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); // Backoff before retrying
    //         }
    //     }

    // _logger.LogInformation("EmailQueueHostedService is stopping...");
    // }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("QueueHostedService is starting...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _consumer.ConsumeAsync(stoppingToken);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("RabbitMQ"))
            {
                _logger.LogWarning("⚠️ RabbitMQ is not available. Retrying in 10 seconds...");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("QueueHostedService cancellation requested.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Unexpected error while consuming messages.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); // backoff
            }
        }

        _logger.LogInformation("QueueHostedService is stopping...");
    }
}

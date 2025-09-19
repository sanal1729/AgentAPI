// <copyright file="DependencyInjection.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application
{
    using System.Reflection;
    using Agent.Application.Common.Behaviors;
    using FluentValidation;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;

    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // services.AddScoped<IAuthCommandService, AuthCommandService>();
            // services.AddScoped<IAuthQueryService, AuthQueryService>();

            // Register MediatR
            services.AddMediatR(typeof(DependencyInjection).Assembly);

            // Register Validation Pipeline Behavior
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviors<,>));

            // services.AddScoped<IValidator<RegisterCommand>, RegisterCommandValidator>();

            // Register FluentValidation
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            return services;
        }
    }
}

// <copyright file="DependencyInjection.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Api.Common.Mapping
{
    using System.Reflection;
    using Mapster;
    using MapsterMapper;

    public static class DependencyInjection
    {
        public static IServiceCollection AddMappings(this IServiceCollection services)
        {
            var config = TypeAdapterConfig.GlobalSettings;
            config.Scan(Assembly.GetExecutingAssembly());

            services.AddSingleton(config);
            services.AddScoped<IMapper, ServiceMapper>();

            return services;
        }
    }
}

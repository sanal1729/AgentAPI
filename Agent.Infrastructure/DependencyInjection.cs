// <copyright file="DependencyInjection.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Infrastructure
{
    using System.Text;
    using Agent.Application.Common.Interfaces.Authentication;
    using Agent.Application.Common.Interfaces.Persistence;
    using Agent.Application.Common.Interfaces.Services;
    using Agent.Domain.Aggregates.Organization.Events;
    using Agent.Domain.Entities;
    using Agent.Infrastructure.Authentication;
    using Agent.Infrastructure.Messaging;
    using Agent.Infrastructure.Persistence;
    using Agent.Infrastructure.Persistence.Interceptors;
    using Agent.Infrastructure.Persistence.Repositories;
    using Agent.Infrastructure.Services;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;

    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            ConfigurationManager configuration)
        {
            services
            .AddAuth(configuration)
            .AddPersistence(configuration)
            .AddMessaging(configuration);

            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

            services.AddSingleton(typeof(IQueryBuilder<>), typeof(QueryBuilder<>));
            services.AddScoped<IQueryBuilderFactory, QueryBuilderFactory>();

            services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddScoped<ICurrentUserService, CurrentUserService>();

            return services;
        }

        public static IServiceCollection AddMessaging(
            this IServiceCollection services,
            ConfigurationManager configuration)
        {
            services.Configure<RabbitMqSettings>(
              configuration.GetSection(RabbitMqSettings.SectionName));

            services.AddSingleton<IMessageConnection, MessageConnection>();
            services.AddSingleton<IConsumer<OrganizationCreated>, OrganizationCreatedConsumer>();
            services.AddSingleton(typeof(IPublisher<>), typeof(RabbitMqPublisher<>));

            // services.AddHealthChecks()
            //         .AddRabbitMQ("amqp://admin:admin@localhost:5672", name: "rabbitmq");
            return services;
        }

        public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        ConfigurationManager configuration)
        {
            // 1️⃣  Options binding – one line, no manual extras
            services.Configure<AppDbSettings>(
                configuration.GetSection(AppDbSettings.SectionName));

            // 2️⃣  Infrastructure
            services.AddHttpContextAccessor();

            // Register scoped services
            services.AddScoped<SaveChangesInterceptor, AuditableEntitySaveChangesInterceptor>();
            services.AddScoped<SaveChangesInterceptor, PublishDomainEventInterceptor>();

            // Register DbContext with interception
            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                var dbSettings = sp.GetRequiredService<IOptionsMonitor<AppDbSettings>>().CurrentValue;
                var interceptors = sp.GetRequiredService<IEnumerable<SaveChangesInterceptor>>(); // Properly resolving scoped service

                // options.UseSqlServer(dbSettings.ConnectionString);
                options.UseNpgsql(dbSettings.ConnectionString)
                .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information) // logs SQL
                .EnableSensitiveDataLogging();
                options.AddInterceptors(interceptors);
            });

            // 4️⃣  Generic repositories / services
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IEfSqlService<>), typeof(EfSqlService<>));
            services.AddScoped(typeof(ISqlService<>), typeof(SqlService<>));

            services.AddScoped<IUserRepository, UserRepository>();

            return services;
        }

        public static IServiceCollection AddAuth(
    this IServiceCollection services,
    ConfigurationManager configuration)
        {
            // 1️⃣ Bind JwtSettings
            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

            // 2️⃣ Add Identity (User = custom class, Role = built-in IdentityRole)
            services.AddIdentity<User, Role>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 7;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 3;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = true;

                // Sign-in settings
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            // .RoleManager<Role>();

            // .AddUserStore<Microsoft.AspNetCore.Identity.EntityFrameworkCore.UserStore<User, Role, AppDbContext, string, IdentityUserClaim<string>, UserRole, IdentityUserLogin<string>, UserToken, IdentityRoleClaim<string>>>()
            // .AddRoleStore<Microsoft.AspNetCore.Identity.EntityFrameworkCore.RoleStore<Role, AppDbContext, string, UserRole, IdentityRoleClaim<string>>>();

            // 3️⃣ Configure application cookie to prevent redirect on unauthorized API access
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.SlidingExpiration = true;

                // Disable redirects for APIs
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
            });

            // 4️⃣ Set JWT as the default authentication scheme
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var jwtSettings = configuration
                    .GetSection(JwtSettings.SectionName)
                    .Get<JwtSettings>() ?? throw new InvalidOperationException("JwtSettings is not configured.");

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.Secret
                            ?? throw new InvalidOperationException("JWT Secret is not configured."))),
                };
            });

            // 5️⃣ Register custom JWT handler
            services.AddScoped<IJwtTokenHandler, JwtTokenHandler>();

            return services;
        }
    }
}

// <copyright file="AuditableEntitySaveChangesInterceptor.cs" company="Agent">
// © Agent 2025
// </copyright>
namespace Agent.Infrastructure.Persistence.Interceptors
{
    using System.Security.Claims;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using Microsoft.EntityFrameworkCore.Diagnostics;

    public class AuditableEntitySaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditableEntitySaveChangesInterceptor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            if (eventData.Context is not null)
            {
                ApplyAuditProperties(eventData.Context.ChangeTracker);
            }

            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context is not null)
            {
                ApplyAuditProperties(eventData.Context.ChangeTracker);
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private string GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
        }

        private long GetCurrentUnixTimeSeconds()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        private void ApplyAuditProperties(ChangeTracker changeTracker)
        {
            var userId = GetCurrentUserId();
            var timestamp = GetCurrentUnixTimeSeconds();

            foreach (var entry in changeTracker.Entries())
            {
                // Only process entities that are not owned and have a primary key
                if (entry.Entity == null || entry.Metadata.IsOwned() || entry.Metadata.FindPrimaryKey() == null)
                {
                    continue;
                }

                if (entry.State == EntityState.Added)
                {
                    if (entry.Property("CreatedOn").Metadata != null)
                    {
                        entry.Property("CreatedOn").CurrentValue = timestamp;
                    }

                    if (entry.Property("CreatedBy").Metadata != null)
                    {
                        entry.Property("CreatedBy").CurrentValue = userId;
                    }
                }

                if (entry.State == EntityState.Modified)
                {
                    if (entry.Property("ModifiedOn").Metadata != null)
                    {
                        entry.Property("ModifiedOn").CurrentValue = timestamp;
                    }

                    if (entry.Property("ModifiedBy").Metadata != null)
                    {
                        entry.Property("ModifiedBy").CurrentValue = userId;
                    }
                }
            }
        }
    }
}

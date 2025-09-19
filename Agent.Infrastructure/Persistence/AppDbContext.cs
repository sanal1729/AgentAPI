// <copyright file="AppDbContext.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Infrastructure.Persistence
{
    using System;
    using Agent.Domain.Aggregates.Organization;
    using Agent.Domain.Aggregates.Organization.Entities;
    using Agent.Domain.Common.Models;
    using Agent.Domain.Entities;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;

    public class AppDbContext : IdentityDbContext<User, Role, string,
                        UserClaim, UserRole,
                        IdentityUserLogin<string>, IdentityRoleClaim<string>, UserToken>
    {
        private const string AuthSchema = "auth";

        private readonly IEnumerable<SaveChangesInterceptor> _interceptors;

        // Constructor accepting interceptor and options
        public AppDbContext(
            DbContextOptions<AppDbContext> options,
            IEnumerable<SaveChangesInterceptor> interceptors)
            : base(options)
        {
            _interceptors = interceptors;
        }

        public DbSet<Organization> Organizations { get; set; } = null!;

        public DbSet<Branch> Branches { get; set; } = null!;

        public DbSet<Entitlement> Entitlements { get; set; } = null!;

        public DbSet<Area> Areas { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            foreach (var interceptor in _interceptors)
            {
                optionsBuilder.AddInterceptors(interceptor);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("agnt");
            modelBuilder.Ignore<List<IDomainEvent>>()
            .ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            // Add audit properties
            ApplyAuditableProperties(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users", AuthSchema);
                entity.HasKey(u => u.Id);
                entity.HasMany(e => e.UserRoles)
                .WithOne(e => e.User)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Roles", AuthSchema);
                entity.HasKey(r => r.Id);
                entity.HasMany(e => e.UserRoles)
                .WithOne(e => e.Role)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.ToTable("UserRoles", AuthSchema);
                entity.HasKey(ur => new { ur.UserId, ur.RoleId, ur.AreaId });

                entity.HasOne(ur => ur.User)
           .WithMany(u => u.UserRoles)
           .HasForeignKey(ur => ur.UserId);

                entity.HasOne(ur => ur.Role)
           .WithMany(r => r.UserRoles)
           .HasForeignKey(ur => ur.RoleId);

                entity.HasOne(ur => ur.Area)
           .WithMany()
           .HasForeignKey(ur => ur.AreaId);
            });

            modelBuilder.Entity<UserClaim>(entity =>
{
    entity.ToTable("UserClaims", AuthSchema);

    entity.HasKey(e => new { e.Id, e.UserId, e.AreaId, e.ClaimType, e.ClaimValue });

    entity.HasOne(e => e.Area)
          .WithMany()
          .HasForeignKey(e => e.AreaId);

    entity.HasOne(e => e.User) // 👈 This is the missing part
          .WithMany(u => u.Claims)
          .HasForeignKey(e => e.UserId)
          .IsRequired();
});

            // modelBuilder.Entity<UserClaim>(entity =>
            // {
            //     entity.ToTable("UserClaims", AuthSchema);
            //     entity.HasKey(e => new { e.Id, e.UserId, e.AreaId, e.ClaimType, e.ClaimValue });

            // // Optional FK constraints
            //     entity.HasOne(e => e.Area)
            //         .WithMany()
            //         .HasForeignKey(e => e.AreaId);
            // });
            modelBuilder.Entity<IdentityRoleClaim<string>>(entity =>
            {
                entity.ToTable("RoleClaims", AuthSchema);
                entity.HasKey(rc => rc.Id);

                entity.Property(rc => rc.Id)
                            .ValueGeneratedNever();
            });

            modelBuilder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.ToTable("UserLogins", AuthSchema);
                entity.HasKey(ul => new { ul.UserId, ul.LoginProvider, ul.ProviderKey });
            });

            modelBuilder.Entity<UserToken>(entity =>
            {
                entity.ToTable("UserTokens", AuthSchema);
                entity.HasKey(ut => new { ut.UserId, ut.LoginProvider, ut.Name });

                // Map custom property
                entity.Property(e => e.Expires)
                    .HasColumnType("bigint")
                    .HasColumnName("Expires");
            });
        }

        private static void ApplyAuditableProperties(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var clrType = entityType.ClrType;

                // Only process root entities with keys (i.e., skip value objects and owned types)
                if (entityType.IsOwned() || entityType.FindPrimaryKey() == null)
                {
                    continue;
                }

                modelBuilder.Entity(clrType).Property<long>("CreatedOn");
                modelBuilder.Entity(clrType).Property<string>("CreatedBy");
                modelBuilder.Entity(clrType).Property<string?>("ModifiedBy");
                modelBuilder.Entity(clrType).Property<long?>("ModifiedOn");
            }

            // Ensure shadow properties are added to Identity-related entities
            ApplyAuditPropertiesToIdentityEntities(modelBuilder);
        }

        private static void ApplyAuditPropertiesToIdentityEntities(ModelBuilder modelBuilder)
        {
            var identityEntities = new List<Type>
    {
        typeof(User),
        typeof(Role),
        typeof(UserRole),
        typeof(UserClaim),
        typeof(IdentityRoleClaim<string>),
        typeof(IdentityUserLogin<string>),
        typeof(UserToken),
    };

            foreach (var entityType in identityEntities)
            {
                modelBuilder.Entity(entityType)
                    .Property<string>("CreatedBy");
                modelBuilder.Entity(entityType)
                    .Property<long>("CreatedOn");
                modelBuilder.Entity(entityType)
                    .Property<string?>("ModifiedBy")
                    .IsRequired(false);
                modelBuilder.Entity(entityType)
                    .Property<long?>("ModifiedOn")
                    .IsRequired(false);
            }
        }
    }
}

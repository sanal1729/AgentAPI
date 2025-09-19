// <copyright file="EntitlementConfigurations.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Infrastructure.Persistence.Configurations
{
       using Agent.Domain.Entities;
       using Microsoft.EntityFrameworkCore;
       using Microsoft.EntityFrameworkCore.Metadata.Builders;

       public class EntitlementConfigurations : IEntityTypeConfiguration<Entitlement>
       {
              private const string AuthSchema = "auth";

              public void Configure(EntityTypeBuilder<Entitlement> builder)
              {
                     builder.ToTable("Claims", schema: AuthSchema);

                     builder.HasKey(e => e.Id);

                     builder.Property(e => e.Id)
                            .ValueGeneratedNever();

                     builder.Property(e => e.Code)
                            .IsRequired()
                            .HasMaxLength(10);

                     builder.Property(e => e.Name)
                            .IsRequired()
                            .HasMaxLength(50);

                     builder.Property(e => e.Description)
                            .HasMaxLength(200);

                     builder.Property(e => e.Type)
                            .IsRequired()
                            .HasMaxLength(20);

                     builder.Property(e => e.Value)
                            .IsRequired()
                            .HasMaxLength(50);

                     // Seed with negative Ids to avoid collisions
       //               builder.HasData(
       //       new
       //       {
       //              Id = -1L,
       //              Code = "R",
       //              Name = "Read",
       //              Description = "Allows user to read resources",
       //              Type = "Permission",
       //              Value = "CanRead",
       //              CreatedOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
       //              CreatedBy = "system",
       //       },
       //       new
       //       {
       //              Id = -2L,
       //              Code = "C",
       //              Name = "Create",
       //              Description = "Allows user to create resources",
       //              Type = "Permission",
       //              Value = "CanCreate",
       //              CreatedOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
       //              CreatedBy = "system",
       //       },
       //       new
       //       {
       //              Id = -3L,
       //              Code = "U",
       //              Name = "Update",
       //              Description = "Allows user to update resources",
       //              Type = "Permission",
       //              Value = "CanUpdate",
       //              CreatedOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
       //              CreatedBy = "system",
       //       },
       //       new
       //       {
       //              Id = -4L,
       //              Code = "D",
       //              Name = "Delete",
       //              Description = "Allows user to delete resources",
       //              Type = "Permission",
       //              Value = "CanDelete",
       //              CreatedOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
       //              CreatedBy = "system",
       //       },
       //       new
       //       {
       //              Id = -5L,
       //              Code = "CS",
       //              Name = "ConfigureSettings",
       //              Description = "Allows user to configure system settings",
       //              Type = "Feature",
       //              Value = "ConfigureSettings",
       //              CreatedOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
       //              CreatedBy = "system",
       //       });
               }
       }
}

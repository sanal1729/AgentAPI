// <copyright file="OrganizationConfigurations.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Infrastructure.Persistence.Configurations
{
    using Agent.Domain.Aggregates.Organization;
    using Agent.Domain.Aggregates.Organization.ValueObjects;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class OrganizationConfigurations : IEntityTypeConfiguration<Organization>
    {
        public void Configure(EntityTypeBuilder<Organization> builder)
        {
            ConfigureOrganizations(builder);
            ConfigureBranches(builder);
        }

        private void ConfigureBranches(EntityTypeBuilder<Organization> builder)
        {
            builder.HasMany(o => o.Branches)
                   .WithOne()
                   .HasForeignKey(ae => ae.OrganizationId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(o => o.Branches)
                   .Metadata.SetField("_branches");

            builder.Navigation(o => o.Branches)
                   .Metadata.SetPropertyAccessMode(PropertyAccessMode.Field);
        }

        private void ConfigureOrganizations(EntityTypeBuilder<Organization> builder)
        {
            builder.ToTable("Organization");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .HasColumnName("OrganizationId")
                .ValueGeneratedNever()
                .HasConversion(
                    id => id.Value,
                    value => OrganizationId.FromGuid(value));

            builder.Property(o => o.Name)
                .HasColumnName("OrganizationName")
                .IsRequired()
                .HasMaxLength(250);

            builder.Property(o => o.CountryCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(o => o.CurrencyCode)
                .IsRequired()
                .HasMaxLength(50);

            // Relationship: Organization -> Branches
            builder.HasMany(o => o.Branches)
                .WithOne()
                .HasForeignKey(a => a.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(o => o.Branches).Metadata.SetField("_branches");
            builder.Navigation(o => o.Branches).UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}

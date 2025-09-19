// <copyright file="BranchConfiguration.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Infrastructure.Persistence.Configurations
{
    using Agent.Domain.Aggregates.Organization.Entities;
    using Agent.Domain.Aggregates.Organization.ValueObjects;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class BranchConfiguration : IEntityTypeConfiguration<Branch>
    {
        public void Configure(EntityTypeBuilder<Branch> builder)
        {
            builder.ToTable("Branch");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id)
                .HasColumnName("BranchId")
                .HasConversion(id => id.Value, value => BranchId.FromGuid(value));

            builder.Property(a => a.Name)
                .HasColumnName("BranchName")
                .IsRequired()
                .HasMaxLength(250);

            builder.Property(a => a.Code)
                .HasColumnName("BranchCode")
                .IsRequired()
                .HasMaxLength(250);

            builder.Property(a => a.OrganizationId)
                .IsRequired();
        }
    }
}

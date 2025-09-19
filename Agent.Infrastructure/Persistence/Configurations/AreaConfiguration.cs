// <copyright file="AreaConfiguration.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Infrastructure.Persistence.Configurations
{
    using Agent.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

       // EF Core configuration for Area entity
    public class AreaConfiguration : IEntityTypeConfiguration<Area>
    {
        public void Configure(EntityTypeBuilder<Area> builder)
        {
            builder.ToTable("Area");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id)
                   .ValueGeneratedNever(); // or ValueGeneratedOnAdd if auto-generated

            builder.Property(a => a.Name)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(a => a.Code)
                   .HasMaxLength(50);

            builder.Property(a => a.Path)
                   .IsRequired()
                   .HasMaxLength(1000);

            builder.Property(a => a.Level)
                   .IsRequired();

            builder.HasOne(a => a.Parent)
                   .WithMany(p => p.AreaNodes)
                   .HasForeignKey(a => a.ParentId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Navigation(a => a.AreaNodes)
                   .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
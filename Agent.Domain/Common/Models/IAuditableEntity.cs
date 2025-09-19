// <copyright file="IAuditableEntity.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Domain.Common.Models
{
    public interface IAuditableEntity
    {
        long CreatedAt { get; set; }

        long? ModifiedAt { get; set; }

        string CreatedBy { get; set; }

        string? ModifiedBy { get; set; }
    }
}
// <copyright file="UserRole.cs" company="Agent">
// © Agent 2025
// </copyright>
namespace Agent.Domain.Entities
{
    using Microsoft.AspNetCore.Identity;

    public class UserRole : IdentityUserRole<string>
    {
        public long AreaId { get; set; }

          // Navigation properties
        public virtual User? User { get; set; }

        public virtual Role? Role { get; set; }

        public virtual Area? Area { get; set; }
    }
}
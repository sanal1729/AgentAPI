// <copyright file="User.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Domain.Entities
{
    using Microsoft.AspNetCore.Identity;

    public class User : IdentityUser
    {
        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public long? DefaultAreaId { get; set; }

        // Navigation property for your custom UserRole
        public virtual ICollection<UserRole>? UserRoles { get; set; }

            // ✅ This is needed for the FK mapping to work
        public virtual ICollection<UserClaim> Claims { get; set; } = new List<UserClaim>();
    }
}
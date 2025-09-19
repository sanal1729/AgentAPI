// <copyright file="UserClaim.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Domain.Entities
{
    using Microsoft.AspNetCore.Identity;

    public class UserClaim : IdentityUserClaim<string>
    {
        public long AreaId { get; set; }

        // Navigation properties
        public virtual User? User { get; set; }

        public virtual Entitlement? Entitlement { get; set; }

        public virtual Area? Area { get; set; }
    }
}
// <copyright file="Role.cs" company="Agent">
// © Agent 2025
// </copyright>
namespace Agent.Domain.Entities
{
    using Microsoft.AspNetCore.Identity;

    public class Role : IdentityRole<string>
    {
        public virtual ICollection<UserRole>? UserRoles { get; set; }
    }
}
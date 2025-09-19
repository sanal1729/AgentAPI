// <copyright file="UserToken.cs" company="Agent">
// © Agent 2025
// </copyright>
namespace Agent.Domain.Entities
{
    using Microsoft.AspNetCore.Identity;

    public class UserToken : IdentityUserToken<string>
    {
        public long? Expires { get; set; }
    }
}
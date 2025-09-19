// <copyright file="Entitlement.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Domain.Entities
{
    public class Entitlement
    {
        public long Id { get; set; }

        public string? Code { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? Type { get; set; }

        public string? Value { get; set; }

        public Entitlement(long id, string? code, string? name, string? description, string? type, string? value)
        {
            Id = id;
            Code = code;
            Name = name;
            Description = description;
            Type = type;
            Value = value;
        }
    }
}
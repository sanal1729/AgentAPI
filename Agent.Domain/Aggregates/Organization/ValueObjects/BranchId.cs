// <copyright file="BranchId.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Domain.Aggregates.Organization.ValueObjects
{
    using System;
    using Agent.Domain.Common.Models;

    public class BranchId : ValueObject
    {
        public Guid Value { get; private set; }

        private BranchId(Guid value)
        {
            this.Value = value;
        }

        public static BranchId CreateUnique()
        {
            return new(Guid.CreateVersion7());
        }

        public static BranchId FromGuid(Guid value)
        {
            return new BranchId(value);
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return this.Value;
        }
    }
}

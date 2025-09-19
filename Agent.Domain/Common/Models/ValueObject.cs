// <copyright file="ValueObject.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Domain.Common.Models
{
    public abstract class ValueObject : IEquatable<ValueObject>
    {
        public ValueObject()
        {
        }

        public abstract IEnumerable<object> GetEqualityComponents();

        public override bool Equals(object? obj)
        {
            if (obj is null || obj.GetType() != this.GetType())
            {
                return false;
            }

            var valueObject = (ValueObject)obj;

            return this.GetEqualityComponents()
                .SequenceEqual(valueObject.GetEqualityComponents());
        }

        public static bool operator ==(ValueObject left, ValueObject right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ValueObject left, ValueObject right)
        {
            return !Equals(left, right);
        }

        public override int GetHashCode()
        {
            return this.GetEqualityComponents().Select(x => x?.GetHashCode() ?? 0).Aggregate((x, y) => x ^ y);
        }

        public bool Equals(ValueObject? other)
        {
            return this.Equals((object?)other);
        }
    }
}

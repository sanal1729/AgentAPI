// <copyright file="TypeExtensions.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Api.ModelBinders
{
    public static class TypeExtensions
    {
        // A method to check if a type is a simple type (primitive or string, etc.)
        public static bool IsSimpleType(this Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            return underlyingType.IsPrimitive
                || underlyingType.IsEnum
                || underlyingType == typeof(string)
                || underlyingType == typeof(decimal)
                || underlyingType == typeof(DateTime)
                || underlyingType == typeof(Guid)
                || underlyingType == typeof(DateTimeOffset)
                || underlyingType == typeof(TimeSpan);
        }
    }
}
// <copyright file="IQueryBuilder.cs" company="Agent">
// © Agent 2025
// </copyright>
namespace Agent.Application.Common.Interfaces.Services
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// Interface for building LINQ queries using dynamic filter and sort expressions.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public interface IQueryBuilder<T>
    {
        /// <summary>
        /// Gets the parsed filter expression as a predicate (used in Where).
        /// </summary>
        Expression<Func<T, bool>>? Predicate { get; }

        /// <summary>
        /// Gets the parsed ordering function (used in OrderBy).
        /// </summary>
        Func<IQueryable<T>, IOrderedQueryable<T>>? OrderBy { get; }

        /// <summary>
        /// Gets the original filter string (optional).
        /// </summary>
        string? Filter { get; }

        /// <summary>
        /// Gets the original sort string (optional).
        /// </summary>
        string? Sort { get; }

        (Expression<Func<T, bool>>? Predicate, Func<IQueryable<T>, IOrderedQueryable<T>>? OrderBy) GetExpression(string? filter, string? sort);
    }
}

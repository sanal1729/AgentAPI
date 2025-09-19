// <copyright file="QueryBuilder.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Infrastructure.Services
{
    using System;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Linq.Dynamic.Core.Exceptions;
    using System.Linq.Expressions;
    using System.Reflection;
    using Agent.Application.Common.Interfaces.Services;

    /// <summary>
    /// A generic query builder that parses dynamic filter and sort expressions into LINQ-compatible delegates.
    /// Designed for clean, composable, and high-performance querying across repositories.
    /// </summary>
    /// <typeparam name="T">Entity type to query.</typeparam>
    public sealed class QueryBuilder<T> : IQueryBuilder<T>
        where T : class
    {
        /// <summary>
        /// Gets the parsed filter expression as a predicate (used in Where).
        /// </summary>
        public Expression<Func<T, bool>>? Predicate { get; }

        /// <summary>
        /// Gets the parsed ordering function (used in OrderBy).
        /// </summary>
        public Func<IQueryable<T>, IOrderedQueryable<T>>? OrderBy { get; }

        /// <summary>
        /// Gets the original filter string (optional).
        /// </summary>
        public string? Filter { get; }

        /// <summary>
        /// Gets the original sort string (optional).
        /// </summary>
        public string? Sort { get; }

        private static readonly ParsingConfig DefaultParsingConfig = new();
        private readonly ParsingConfig _parsingConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryBuilder{T}"/> class.
        /// </summary>
        /// <param name="filter">Dynamic LINQ filter string or plain search term.</param>
        /// <param name="sort">Dynamic LINQ sort string (e.g., "Name descending").</param>
        /// <param name="config">Optional parsing configuration (defaults to static shared config).</param>
        public QueryBuilder(string? filter = null, string? sort = null, ParsingConfig? config = null)
        {
            _parsingConfig = config ?? DefaultParsingConfig;

            Filter = filter;
            Sort = sort;

            (Predicate, OrderBy) = InitializeExpressions(Filter, Sort);
        }

        /// <summary>
        /// Gets the parsed filter and sort expressions as a tuple.
        /// </summary>
        public (Expression<Func<T, bool>>? Predicate, Func<IQueryable<T>, IOrderedQueryable<T>>? OrderBy) GetExpression(string? filter, string? sort)
        {
            return InitializeExpressions(filter, sort);
        }

        private (Expression<Func<T, bool>>? Predicate, Func<IQueryable<T>, IOrderedQueryable<T>>? OrderBy) InitializeExpressions(string? filter, string? sort)
        {
            Expression<Func<T, bool>>? predicate = null;
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null;

            if (!string.IsNullOrWhiteSpace(filter))
            {
                if (IsDynamicExpression(filter))
                {
                    predicate = ParseFilter(filter);
                }
                else
                {
                    predicate = BuildSearchPredicate(filter);
                }
            }

            if (!string.IsNullOrWhiteSpace(sort))
            {
                orderBy = ParseSort(sort);
            }
            else
            {
                // Default: first string property
                var firstStringProp = typeof(T)
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(p => p.PropertyType == typeof(string));

                if (firstStringProp != null)
                {
                    var defaultSort = firstStringProp.Name + " ascending";
                    orderBy = ParseSort(defaultSort);
                }
            }

            return (predicate, orderBy);
        }

        private Expression<Func<T, bool>> ParseFilter(string filter)
        {
            try
            {
                return DynamicExpressionParser.ParseLambda<T, bool>(_parsingConfig, false, filter);
            }
            catch (ParseException ex)
            {
                throw new ArgumentException($"Invalid filter expression: '{filter}'", ex);
            }
        }

        private Func<IQueryable<T>, IOrderedQueryable<T>> ParseSort(string sort)
        {
            try
            {
                return source => DynamicQueryableExtensions.OrderBy(source, _parsingConfig, sort);
            }
            catch (ParseException ex)
            {
                throw new ArgumentException($"Invalid sort expression: '{sort}'", ex);
            }
        }
        /// <summary>
        /// Builds a predicate that searches across all public properties of T.
        /// Supports strings (case-insensitive), numeric, bool, Guid, and DateTime.
        /// </summary>
        private Expression<Func<T, bool>> BuildSearchPredicate(string search)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            Expression? body = null;

            var searchLower = Expression.Constant(search.ToLower());

            foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.GetIndexParameters().Length > 0) continue; // skip indexers

                var propertyExpr = Expression.Property(parameter, prop);
                Expression? condition = null;

                if (prop.PropertyType == typeof(string))
                {
                    // x.Prop != null && x.Prop.ToLower().Contains(searchLower)
                    var notNull = Expression.NotEqual(propertyExpr, Expression.Constant(null, typeof(string)));
                    var toLower = Expression.Call(propertyExpr, nameof(string.ToLower), Type.EmptyTypes);
                    var contains = Expression.Call(toLower, nameof(string.Contains), Type.EmptyTypes, searchLower);
                    condition = Expression.AndAlso(notNull, contains);
                }
                else if (prop.PropertyType == typeof(int) && int.TryParse(search, out var intVal))
                {
                    // x.Prop == intVal
                    condition = Expression.Equal(propertyExpr, Expression.Constant(intVal));
                }
                else if (prop.PropertyType == typeof(long) && long.TryParse(search, out var longVal))
                {
                    condition = Expression.Equal(propertyExpr, Expression.Constant(longVal));
                }
                else if (prop.PropertyType == typeof(decimal) && decimal.TryParse(search, out var decVal))
                {
                    condition = Expression.Equal(propertyExpr, Expression.Constant(decVal));
                }
                else if (prop.PropertyType == typeof(double) && double.TryParse(search, out var dblVal))
                {
                    condition = Expression.Equal(propertyExpr, Expression.Constant(dblVal));
                }
                else if (prop.PropertyType == typeof(bool) && bool.TryParse(search, out var boolVal))
                {
                    condition = Expression.Equal(propertyExpr, Expression.Constant(boolVal));
                }
                else if (prop.PropertyType == typeof(DateTime) && DateTime.TryParse(search, out var dateVal))
                {
                    condition = Expression.Equal(propertyExpr, Expression.Constant(dateVal));
                }
                else if (prop.PropertyType == typeof(Guid) && Guid.TryParse(search, out var guidVal))
                {
                    condition = Expression.Equal(propertyExpr, Expression.Constant(guidVal));
                }

                if (condition != null)
                {
                    body = body == null ? condition : Expression.OrElse(body, condition);
                }
            }

            if (body == null)
            {
                body = Expression.Constant(false);
            }

            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        private static bool IsDynamicExpression(string filter)
        {
            return filter.Contains("==") || filter.Contains(">") || filter.Contains("<") || filter.Contains("&&") || filter.Contains("||");
        }

        /// <summary>
        /// Applies the predicate and sort to an IQueryable.
        /// </summary>
        public IQueryable<T> Apply(IQueryable<T> source)
        {
            if (Predicate != null)
            {
                source = source.Where(Predicate);
            }

            if (OrderBy != null)
            {
                source = OrderBy(source);
            }

            return source;
        }
    }
}

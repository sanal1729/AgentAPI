// <copyright file="QueryBuilderFactory.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Infrastructure.Services
{
    using System.Linq.Dynamic.Core;
    using Agent.Application.Common.Interfaces.Services;

    public sealed class QueryBuilderFactory : IQueryBuilderFactory
    {
        private readonly ParsingConfig _defaultParsingConfig;

        public QueryBuilderFactory()
        {
            _defaultParsingConfig = new ParsingConfig(); // You can customize if needed
        }

        public IQueryBuilder<T> Create<T>(string? filter = null, string? sort = null, params object[] values)
            where T : class
        {
            return new QueryBuilder<T>(filter, sort, _defaultParsingConfig);
        }
    }
}
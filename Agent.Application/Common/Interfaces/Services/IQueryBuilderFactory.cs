// <copyright file="IQueryBuilderFactory.cs" company="Agent">
// © Agent 2025
// </copyright>
namespace Agent.Application.Common.Interfaces.Services
{
    using Agent.Application.Common.Interfaces.Services;

    public interface IQueryBuilderFactory
    {
        IQueryBuilder<T> Create<T>(string? filter = null, string? sort = null, params object[] values)
            where T : class;
    }
}
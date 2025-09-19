// <copyright file="IRepository.cs" company="Agent">
// © Agent 2025
// </copyright>
namespace Agent.Application.Common.Interfaces.Persistence;

using System.Linq.Expressions;

public interface IRepository<T>
    where T : class
{
    Task<(IReadOnlyList<T> Items, long TotalCount)> GetPagedAsync(
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        long pageNumber = 1,
        long pageSize = long.MaxValue,
        bool includeNavigations = true,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> GetByPredicateAsync(
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        bool includeNavigations = true,
        CancellationToken cancellationToken = default);

    Task<bool> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : class;

    Task<bool> AddRangeAsync<TEntity>(IQueryable<TEntity> entities, bool includeNavigations = true, CancellationToken cancellationToken = default)
        where TEntity : class;

    Task<bool> AddRangeAsync<TEntity>(IEnumerable<TEntity> entities, bool includeNavigations = true, CancellationToken cancellationToken = default)
        where TEntity : class;

    Task<TEntity?> GetByIdAsync<TEntity>(TEntity entity, bool includeNavigations = true, CancellationToken cancellationToken = default)
        where TEntity : class;

    Task<bool> DeleteByIdAsync<TEntity>(TEntity entity, bool includeNavigations = true, CancellationToken cancellationToken = default)
    where TEntity : class;

    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync<TEntity>(TEntity entity, bool includeNavigations = true, CancellationToken cancellationToken = default)
    where TEntity : class;
}
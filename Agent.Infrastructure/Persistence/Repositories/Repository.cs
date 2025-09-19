// <copyright file="Repository.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Infrastructure.Persistence.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Agent.Application.Common.Interfaces.Persistence;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata;
    using Microsoft.Extensions.Logging;

    public class Repository<T> : IRepository<T>
        where T : class
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;
        private readonly ILogger<Repository<T>> _logger;

        public Repository(AppDbContext context, ILogger<Repository<T>> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<(IReadOnlyList<T> Items, long TotalCount)> GetPagedAsync(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            long pageNumber = 1,
            long pageSize = long.MaxValue,
            bool includeNavigations = true,
            CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than or equal to 1.");
            }

            if (pageSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than or equal to 1.");
            }

            cancellationToken.ThrowIfCancellationRequested();

            IQueryable<T> query = _dbSet.AsNoTracking();

            if (includeNavigations)
            {
                var entityType = _context.Model.FindEntityType(typeof(T));
                if (entityType != null)
                {
                    foreach (var navigation in entityType.GetNavigations())
                    {
                        query = query.Include(navigation.Name);
                    }
                }
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

            if (totalCount == 0)
            {
                return (Array.Empty<T>(), 0);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            var items = await query
            .Skip((int)((pageNumber - 1) * pageSize))
            .Take((int)pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

            return (items, totalCount);
        }

        public async Task<bool> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await _context.Set<TEntity>().AddAsync(entity, cancellationToken).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();

                // await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding an entity of type {EntityType}.", typeof(TEntity).Name);
                return false;
            }
        }

        public async Task<bool> DeleteByIdAsync<TEntity>(TEntity entity, bool includeNavigations = true, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var entityType = _context.Model.FindEntityType(typeof(TEntity));
            if (entityType == null)
            {
                throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} is not part of the model.");
            }

            var keyProperties = entityType.FindPrimaryKey()?.Properties;
            if (keyProperties == null || !keyProperties.Any())
            {
                throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} does not have a primary key defined.");
            }

            IQueryable<TEntity> query = _context.Set<TEntity>();

            if (includeNavigations)
            {
                var navigations = entityType.GetNavigations();
                foreach (var navigation in navigations)
                {
                    query = query.Include(navigation.Name);
                }
            }

            var parameter = Expression.Parameter(typeof(TEntity), "e");
            Expression? predicate = null;

            foreach (var keyProperty in keyProperties)
            {
                var propertyValue = keyProperty.PropertyInfo?.GetValue(entity);
                if (propertyValue == null)
                {
                    throw new InvalidOperationException($"Key property {keyProperty.Name} cannot be null.");
                }

                var propertyExpression = Expression.Property(parameter, keyProperty.Name);
                var equalsExpression = Expression.Equal(propertyExpression, Expression.Constant(propertyValue));
                predicate = predicate == null ? equalsExpression : Expression.AndAlso(predicate, equalsExpression);
            }

            if (predicate == null)
            {
                throw new InvalidOperationException("Failed to construct a valid predicate for the primary key.");
            }

            var lambda = Expression.Lambda<Func<TEntity, bool>>(predicate, parameter);
            cancellationToken.ThrowIfCancellationRequested();
            var entityToDelete = await query.FirstOrDefaultAsync(lambda, cancellationToken).ConfigureAwait(false);

            if (entityToDelete == null)
            {
                _logger.LogWarning("Entity of type {EntityType} with the specified key was not found.", typeof(TEntity).Name);
                return false;
            }

            try
            {
                _context.Set<TEntity>().Remove(entityToDelete);

                // await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting an entity of type {EntityType}.", typeof(TEntity).Name);
                return false;
            }
        }

        public async Task<TEntity?> GetByIdAsync<TEntity>(TEntity entity, bool includeNavigations = false, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var entityType = _context.Model.FindEntityType(typeof(TEntity));
            if (entityType == null)
            {
                throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} is not part of the model.");
            }

            var keyProperties = entityType.FindPrimaryKey()?.Properties;
            if (keyProperties == null || !keyProperties.Any())
            {
                throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} does not have a primary key defined.");
            }

            IQueryable<TEntity> query = _context.Set<TEntity>().AsNoTracking();

            if (includeNavigations)
            {
                var navigations = entityType.GetNavigations();
                foreach (var navigation in navigations)
                {
                    query = query.Include(navigation.Name);
                }
            }

            var parameter = Expression.Parameter(typeof(TEntity), "e");
            Expression? predicate = null;

            foreach (var keyProperty in keyProperties)
            {
                var propertyValue = keyProperty.PropertyInfo?.GetValue(entity);
                if (propertyValue == null)
                {
                    throw new InvalidOperationException($"Key property {keyProperty.Name} cannot be null.");
                }

                var propertyExpression = Expression.Property(parameter, keyProperty.Name);
                var equalsExpression = Expression.Equal(propertyExpression, Expression.Constant(propertyValue));
                predicate = predicate == null ? equalsExpression : Expression.AndAlso(predicate, equalsExpression);
            }

            if (predicate == null)
            {
                throw new InvalidOperationException("Failed to construct a valid predicate for the primary key.");
            }

            var lambda = Expression.Lambda<Func<TEntity, bool>>(predicate, parameter);
            return await query.FirstOrDefaultAsync(lambda, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> UpdateAsync<TEntity>(TEntity entity, bool includeNavigations = true, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var entityType = _context.Model.FindEntityType(typeof(TEntity));
            if (entityType == null)
            {
                throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} is not part of the model.");
            }

            var keyProperties = entityType.FindPrimaryKey()?.Properties;
            if (keyProperties == null || !keyProperties.Any())
            {
                throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} does not have a primary key defined.");
            }

            IQueryable<TEntity> query = _context.Set<TEntity>();

            if (includeNavigations)
            {
                var navigations = entityType.GetNavigations();
                foreach (var navigation in navigations)
                {
                    query = query.Include(navigation.Name);
                }
            }

            var parameter = Expression.Parameter(typeof(TEntity), "e");
            Expression? predicate = null;

            foreach (var keyProperty in keyProperties)
            {
                var propertyValue = keyProperty.PropertyInfo?.GetValue(entity);
                if (propertyValue == null)
                {
                    throw new InvalidOperationException($"Key property {keyProperty.Name} cannot be null.");
                }

                var propertyExpression = Expression.Property(parameter, keyProperty.Name);
                var equalsExpression = Expression.Equal(propertyExpression, Expression.Constant(propertyValue));
                predicate = predicate == null ? equalsExpression : Expression.AndAlso(predicate, equalsExpression);
            }

            if (predicate == null)
            {
                throw new InvalidOperationException("Failed to construct a valid predicate for the primary key.");
            }

            var lambda = Expression.Lambda<Func<TEntity, bool>>(predicate, parameter);
            var existingEntity = await query.FirstOrDefaultAsync(lambda, cancellationToken).ConfigureAwait(false);

            if (existingEntity == null)
            {
                _logger.LogWarning("Entity of type {EntityType} with the specified key was not found.", typeof(TEntity).Name);
                return false;
            }

            try
            {
                // Update scalar properties
                _context.Entry(existingEntity).CurrentValues.SetValues(entity);

                if (includeNavigations)
                {
                    var navigations = entityType.GetNavigations();
                    foreach (var navigation in navigations)
                    {
                        var navigationProperty = entityType.FindNavigation(navigation.Name);
                        if (navigationProperty != null && navigationProperty.IsCollection)
                        {
                            var currentValue = navigationProperty.PropertyInfo?.GetValue(existingEntity) as IEnumerable<object>;
                            var newValue = navigationProperty.PropertyInfo?.GetValue(entity) as IEnumerable<object>;

                            if (currentValue != null && newValue != null)
                            {
                                var currentList = currentValue.ToList();
                                var newList = newValue.ToList();

                                // Find items to remove
                                var itemsToRemove = currentList
                                    .Where(c =>
                                    {
                                        var keyProperties = navigation.TargetEntityType.FindPrimaryKey()?.Properties;
                                        return keyProperties != null && !newList.Any(n => AreCompositeKeysEqual(c, n, keyProperties));
                                    })
                                    .ToList();

                                // Find items to add
                                var itemsToAdd = newList
                                    .Where(n =>
                                    {
                                        var keyProperties = navigation.TargetEntityType.FindPrimaryKey()?.Properties;
                                        return keyProperties != null && !currentList.Any(c => AreCompositeKeysEqual(c, n, keyProperties));
                                    })
                                    .ToList();

                                // Update navigation properties
                                foreach (var item in currentList)
                                {
                                    var matchingItem = newList.FirstOrDefault(n =>
                                    {
                                        var keyProperties = navigation.TargetEntityType.FindPrimaryKey()?.Properties;
                                        return keyProperties != null && AreCompositeKeysEqual(item, n, keyProperties);
                                    });
                                    if (matchingItem != null)
                                    {
                                        _context.Entry(item).CurrentValues.SetValues(matchingItem);
                                    }
                                }

                                foreach (var item in itemsToRemove)
                                {
                                    _context.Remove(item);
                                }

                                foreach (var item in itemsToAdd)
                                {
                                    _context.Add(item);
                                }
                            }
                        }
                    }
                }

                // await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating an entity of type {EntityType}.", typeof(TEntity).Name);
                return false;
            }
        }

        public async Task<bool> AddRangeAsync<TEntity>(IEnumerable<TEntity> entities, bool includeNavigations = true, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            if (entities == null || !entities.Any())
            {
                throw new ArgumentNullException(nameof(entities), "Entities must be provided.");
            }

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                if (includeNavigations)
                {
                    var entityType = _context.Model.FindEntityType(typeof(TEntity));
                    if (entityType != null)
                    {
                        foreach (var navigation in entityType.GetNavigations())
                        {
                            foreach (var entity in entities)
                            {
                                var navigationProperty = entityType.FindNavigation(navigation.Name);
                                if (navigationProperty != null && navigationProperty.IsCollection)
                                {
                                    var navigationValue = navigationProperty.PropertyInfo?.GetValue(entity) as IEnumerable<object>;
                                    if (navigationValue != null)
                                    {
                                        foreach (var item in navigationValue)
                                        {
                                            _context.Attach(item);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                await _context.Set<TEntity>().AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);

                // await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding a range of entities of type {EntityType}.", typeof(TEntity).Name);
                return false;
            }
        }

        public async Task<bool> AddRangeAsync<TEntity>(IQueryable<TEntity> entities, bool includeNavigations = true, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities), "Entities must be provided.");
            }

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var entityList = await entities.ToListAsync(cancellationToken).ConfigureAwait(false);
                return await AddRangeAsync(entityList, includeNavigations, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding a range of entities of type {EntityType}.", typeof(TEntity).Name);
                return false;
            }
        }

        public async Task<bool> DeleteRangeAsync<TEntity>(IQueryable<TEntity> entities, bool includeNavigations = true, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities), "Entities must be provided.");
            }

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var entityList = await entities.ToListAsync(cancellationToken).ConfigureAwait(false);
                return await DeleteRangeAsync(entityList, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting a range of entities of type {EntityType}.", typeof(TEntity).Name);
                return false;
            }
        }

        public async Task<bool> DeleteRangeAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            if (entities == null || !entities.Any())
            {
                throw new ArgumentNullException(nameof(entities), "Entities must be provided.");
            }

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                _context.Set<TEntity>().RemoveRange(entities);
                await Task.CompletedTask; // Simulate async behavior since SaveChanges will be handled by UnitOfWork
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting a range of entities of type {EntityType}.", typeof(TEntity).Name);
                return false;
            }
        }

        public async Task<bool> UpdateRangeAsync<TEntity>(IQueryable<TEntity> entities, bool includeNavigations = true, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities), "Entities must be provided.");
            }

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var entityList = await entities.ToListAsync(cancellationToken).ConfigureAwait(false);
                return await UpdateRangeAsync(entityList, includeNavigations, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating a range of entities of type {EntityType}.", typeof(TEntity).Name);
                return false;
            }
        }

        public Task<bool> UpdateRangeAsync<TEntity>(IEnumerable<TEntity> entities, bool includeNavigations = true, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            if (entities == null || !entities.Any())
            {
                throw new ArgumentNullException(nameof(entities), "Entities must be provided.");
            }

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                foreach (var entity in entities)
                {
                    _context.Entry(entity).State = EntityState.Modified;
                }

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating a range of entities of type {EntityType}.", typeof(TEntity).Name);
                return Task.FromResult(false);
            }
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(predicate, cancellationToken);
        }

        public async Task<IReadOnlyList<T>> GetByPredicateAsync(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            bool includeNavigations = true,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            IQueryable<T> query = _dbSet.AsNoTracking();

            if (includeNavigations)
            {
                var entityType = _context.Model.FindEntityType(typeof(T));
                if (entityType != null)
                {
                    foreach (var navigation in entityType.GetNavigations())
                    {
                        query = query.Include(navigation.Name);
                    }
                }
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        private bool AreCompositeKeysEqual(object entity1, object entity2, IReadOnlyList<IProperty> keyProperties)
        {
            foreach (var keyProperty in keyProperties)
            {
                var value1 = keyProperty.PropertyInfo?.GetValue(entity1);
                var value2 = keyProperty.PropertyInfo?.GetValue(entity2);

                if (!Equals(value1, value2))
                {
                    return false;
                }
            }

            return true;
        }
    }
}

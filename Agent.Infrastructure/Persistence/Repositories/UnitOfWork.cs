// <copyright file="UnitOfWork.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Infrastructure.Persistence.Repositories;

using System.Collections.Concurrent;
using Agent.Application.Common.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

public sealed class UnitOfWork : IUnitOfWork, IAsyncDisposable, IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<UnitOfWork> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ConcurrentDictionary<Type, object> _repositories = new();
    private IDbContextTransaction? _currentTransaction;

    public UnitOfWork(AppDbContext dbContext, ILogger<UnitOfWork> logger, ILoggerFactory loggerFactory)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    /// <summary>
    /// Gets a value indicating whether a transaction is currently in progress.
    /// </summary>
    public bool HasActiveTransaction => _currentTransaction != null;

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "An error occurred while saving changes to the database.");

            if (_currentTransaction != null)
            {
                await RollbackTransactionAsync(cancellationToken);
            }

            throw;
        }
    }

    /// <inheritdoc />
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        _currentTransaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        _logger.LogInformation("Database transaction started.");
    }

    /// <inheritdoc />
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("No transaction in progress to commit.");
        }

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _currentTransaction.CommitAsync(cancellationToken);
            _logger.LogInformation("Database transaction committed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while committing the transaction. Rolling back...");
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    /// <inheritdoc />
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("No transaction in progress to roll back.");
        }

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
            _logger.LogWarning("Database transaction rolled back.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while rolling back the transaction.");
            throw;
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    /// <inheritdoc />
    public IRepository<TEntity> GetRepository<TEntity>()
        where TEntity : class
    {
        var entityType = typeof(TEntity);

        if (_repositories.TryGetValue(entityType, out var repo))
        {
            return (IRepository<TEntity>)repo;
        }

        var logger = _loggerFactory.CreateLogger<Repository<TEntity>>();
        var repositoryInstance = new Repository<TEntity>(_dbContext, logger);

        if (!_repositories.TryAdd(entityType, repositoryInstance))
        {
            throw new InvalidOperationException($"Repository for type {entityType.Name} could not be added.");
        }

        return repositoryInstance;
    }

    /// <summary>
    /// Optionally supports resolving custom repositories via DI.
    /// </summary>
    /// <typeparam name="TRepository">The type of the custom repository to resolve.</typeparam>
    /// <returns>The resolved custom repository of type <typeparamref name="TRepository"/>.</returns>
    public TRepository GetCustomRepository<TRepository>()
        where TRepository : notnull
    {
        if (_dbContext.GetService(typeof(TRepository)) is not TRepository repository)
        {
            throw new InvalidOperationException($"Custom repository of type {typeof(TRepository).Name} is not registered.");
        }

        return repository;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _logger.LogDebug("Disposing UnitOfWork synchronously.");
        DisposeTransactionAsync().GetAwaiter().GetResult();
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        _logger.LogDebug("Disposing UnitOfWork asynchronously.");
        await DisposeTransactionAsync();
        await _dbContext.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    private async Task DisposeTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }
}

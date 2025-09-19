// <copyright file="EfSqlService.cs" company="Agent">
// © Agent 2025
// </copyright>

using System.Data;
using Agent.Application.Common.Interfaces.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class EfSqlService<T> : IEfSqlService<T>, IAsyncDisposable
    where T : class
{
    private readonly DbContext _context;
    private readonly ILogger<EfSqlService<T>> _logger;
    private IDbConnection? _connection;

    public EfSqlService(DbContext context, ILogger<EfSqlService<T>> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    async Task<List<TResult>> IEfSqlService<T>.ExecuteQueryAsync<TResult>(
        string sqlQuery,
        IEnumerable<SqlParameter>? parameters,
        CancellationToken cancellationToken)
    {
        try
        {
            var sqlParams = parameters?.ToArray() ?? Array.Empty<SqlParameter>();
            _logger.LogInformation("Executing SQL Query: {SqlQuery}", sqlQuery);

            return await _context.Set<TResult>()
                .FromSqlRaw(sqlQuery, sqlParams)
                .ToListAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw new TaskCanceledException("The operation was canceled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing query.");
            throw new InvalidOperationException("An error occurred while executing the query.", ex);
        }
    }

    async Task<TResult?> IEfSqlService<T>.ExecuteSingleAsync<TResult>(
        string sqlQuery,
        IEnumerable<SqlParameter>? parameters,
        CancellationToken cancellationToken)
        where TResult : class
    {
        try
        {
            var sqlParams = parameters?.ToArray() ?? Array.Empty<SqlParameter>();
            _logger.LogInformation("Executing SQL Query: {SqlQuery}", sqlQuery);

            return await _context.Set<TResult>()
                .FromSqlRaw(sqlQuery, sqlParams)
                .FirstOrDefaultAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw new TaskCanceledException("The operation was canceled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing single query.");
            throw new InvalidOperationException("An error occurred while executing the single query.", ex);
        }
    }

    public async Task<TResult> ExecuteScalarAsync<TResult>(
        string sqlQuery,
        IEnumerable<SqlParameter>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _connection = _context.Database.GetDbConnection();
            using var command = _connection.CreateCommand();
            command.CommandText = sqlQuery;
            command.CommandType = CommandType.Text;

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.Add(param);
                }
            }

            _logger.LogInformation("Executing scalar SQL Query: {SqlQuery}", sqlQuery);

            await _context.Database.OpenConnectionAsync(cancellationToken);
            var result = await Task.Run(() => command.ExecuteScalar(), cancellationToken);

            if (result == null)
            {
                throw new InvalidOperationException("The scalar query returned a null value.");
            }

            return (TResult)Convert.ChangeType(result, typeof(TResult));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing scalar query.");
            throw new InvalidOperationException("An error occurred while executing the scalar query.", ex);
        }
        finally
        {
            if (_connection?.State == ConnectionState.Open)
            {
                _connection.Close();
            }
        }
    }

    public async Task<int> ExecuteNonQueryAsync(
        string sqlQuery,
        IEnumerable<SqlParameter>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _connection = _context.Database.GetDbConnection();
            using var command = _connection.CreateCommand();
            command.CommandText = sqlQuery;
            command.CommandType = CommandType.Text;

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.Add(param);
                }
            }

            _logger.LogInformation("Executing non-query SQL Query: {SqlQuery}", sqlQuery);

            await _context.Database.OpenConnectionAsync(cancellationToken);
            return await Task.Run(() => command.ExecuteNonQuery(), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing non-query.");
            throw new InvalidOperationException("An error occurred while executing the non-query.", ex);
        }
        finally
        {
            if (_connection?.State == ConnectionState.Open)
            {
                _connection.Close();
            }
        }
    }

    public async Task ExecuteWithTransactionAsync(
        Func<IEfSqlService<T>, Task> operation,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                _logger.LogInformation("Beginning transaction.");
                await operation(this);
                await transaction.CommitAsync(cancellationToken);
                _logger.LogInformation("Transaction committed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transaction failed, rolling back.");
                await transaction.RollbackAsync(cancellationToken);
                throw new InvalidOperationException("An error occurred during the transaction.", ex);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting transaction.");
            throw new InvalidOperationException("An error occurred while managing the transaction.", ex);
        }
    }

    public ValueTask DisposeAsync()
    {
        if (_connection?.State == ConnectionState.Open)
        {
            _connection.Close();
        }

        _connection?.Dispose();
        return ValueTask.CompletedTask;
    }
}

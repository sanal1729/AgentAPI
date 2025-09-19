// <copyright file="ISqlService.cs" company="Agent">
// © Agent 2025
// </copyright>
using System.Data;
using Microsoft.Data.SqlClient;

/// <summary>
/// Interface for SQL Service operations.
/// </summary>
/// <typeparam name="T">The type of entity.</typeparam>
public interface ISqlService<T>
    where T : class
{
    /// <summary>
    /// Executes a SQL query and maps the result to a list of type TResult.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="query">The SQL query to execute.</param>
    /// <param name="parameters">Optional parameters for the query.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <param name="commandType">Type of SQL command (Text or StoredProcedure).</param>
    /// <returns>A list of results of type TResult.</returns>
    Task<List<TResult>> ExecuteQueryAsync<TResult>(
        string query,
        IEnumerable<SqlParameter>? parameters = null,
        CancellationToken cancellationToken = default,
        CommandType commandType = CommandType.Text);

    /// <summary>
    /// Executes a SQL query and returns a single result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="query">The SQL query to execute.</param>
    /// <param name="parameters">Optional parameters for the query.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <param name="commandType">Type of SQL command (Text or StoredProcedure).</param>
    /// <returns>A single result of type TResult or null.</returns>
    Task<TResult?> ExecuteSingleAsync<TResult>(
        string query,
        IEnumerable<SqlParameter>? parameters = null,
        CancellationToken cancellationToken = default,
        CommandType commandType = CommandType.Text)
        where TResult : class;

    /// <summary>
    /// Executes a scalar SQL query and returns a single value.
    /// </summary>
    /// <typeparam name="TResult">The type of the scalar value.</typeparam>
    /// <param name="query">The SQL query to execute.</param>
    /// <param name="parameters">Optional parameters for the query.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <param name="commandType">Type of SQL command (Text or StoredProcedure).</param>
    /// <returns>The scalar result of type TResult.</returns>
    Task<TResult> ExecuteScalarAsync<TResult>(
        string query,
        IEnumerable<SqlParameter>? parameters = null,
        CancellationToken cancellationToken = default,
        CommandType commandType = CommandType.Text);

    /// <summary>
    /// Executes a non-query SQL command (e.g., INSERT, UPDATE, DELETE).
    /// </summary>
    /// <param name="query">The SQL command to execute.</param>
    /// <param name="parameters">Optional parameters for the command.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <param name="commandType">Type of SQL command (Text or StoredProcedure).</param>
    /// <returns>The number of rows affected.</returns>
    Task<int> ExecuteNonQueryAsync(
        string query,
        IEnumerable<SqlParameter>? parameters = null,
        CancellationToken cancellationToken = default,
        CommandType commandType = CommandType.Text);

    /// <summary>
    /// Executes a series of operations within a transaction.
    /// </summary>
    /// <param name="operation">A function representing the operations to execute.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ExecuteWithTransactionAsync(
        Func<ISqlService<T>, Task> operation,
        CancellationToken cancellationToken = default);
}

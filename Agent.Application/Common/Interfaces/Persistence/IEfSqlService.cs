// <copyright file="IEfSqlService.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Common.Interfaces.Persistence
{
    using Microsoft.Data.SqlClient;

    /// <summary>
    /// Interface for EF SQL Service.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    public interface IEfSqlService<T> : IAsyncDisposable
    {
        /// <summary>
        /// Executes a raw SQL query and returns a list of results.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="sqlQuery">The SQL query to execute.</param>
        /// <param name="parameters">The parameters for the SQL query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A list of results of type <typeparamref name="T"/>.</returns>
        Task<List<TResult>> ExecuteQueryAsync<TResult>(
            string sqlQuery,
            IEnumerable<SqlParameter>? parameters = null,
            CancellationToken cancellationToken = default)
            where TResult : class;

        /// <summary>
        /// Executes a raw SQL query and returns a single result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="sqlQuery">The SQL query to execute.</param>
        /// <param name="parameters">The parameters for the SQL query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A single result of type <typeparamref name="T"/> or null.</returns>
        Task<TResult?> ExecuteSingleAsync<TResult>(
            string sqlQuery,
            IEnumerable<SqlParameter>? parameters = null,
            CancellationToken cancellationToken = default)
            where TResult : class;

        /// <summary>
        /// Executes a scalar query (returns a single value).
        /// </summary>
        /// <typeparam name="TResult">The type of the scalar value.</typeparam>
        /// <param name="sqlQuery">The SQL query to execute.</param>
        /// <param name="parameters">The parameters for the SQL query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The scalar value returned by the query.</returns>
        Task<TResult> ExecuteScalarAsync<TResult>(
            string sqlQuery,
            IEnumerable<SqlParameter>? parameters = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a non-query (insert, update, delete).
        /// </summary>
        /// <param name="sqlQuery">The SQL query to execute.</param>
        /// <param name="parameters">The parameters for the SQL query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The number of rows affected.</returns>
        Task<int> ExecuteNonQueryAsync(
            string sqlQuery,
            IEnumerable<SqlParameter>? parameters = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a set of SQL commands within a transaction.
        /// </summary>
        /// <param name="operation">The operation to execute within the transaction.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ExecuteWithTransactionAsync(
            Func<IEfSqlService<T>, Task> operation,
            CancellationToken cancellationToken = default);
    }
}

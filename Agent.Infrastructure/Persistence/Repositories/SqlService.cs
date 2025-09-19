// <copyright file="SqlService.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Infrastructure.Persistence.Repositories
{
    using System.Data;
    using Microsoft.Data.SqlClient;

    /// <summary>
    /// Implementation of the ISqlService interface for SQL operations.
    /// </summary>
    /// <typeparam name="T">The type of entity.</typeparam>
    public class SqlService<T> : ISqlService<T>
        where T : class
    {
        private readonly string _connectionString;

        public SqlService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<TResult>> ExecuteQueryAsync<TResult>(
            string query,
            IEnumerable<SqlParameter>? parameters = null,
            CancellationToken cancellationToken = default,
            CommandType commandType = CommandType.Text)
        {
            var results = new List<TResult>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection)
            {
                CommandType = commandType,
            };

            if (parameters != null)
            {
                command.Parameters.AddRange(parameters.ToArray());
            }

            await connection.OpenAsync(cancellationToken);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                // Assuming TResult has a parameterless constructor and properties matching the columns
                var result = Activator.CreateInstance<TResult>();
                foreach (var property in typeof(TResult).GetProperties())
                {
                    if (!reader.IsDBNull(reader.GetOrdinal(property.Name)))
                    {
                        property.SetValue(result, reader[property.Name]);
                    }
                }

                results.Add(result);
            }

            return results;
        }

        public async Task<TResult?> ExecuteSingleAsync<TResult>(
            string query,
            IEnumerable<SqlParameter>? parameters = null,
            CancellationToken cancellationToken = default,
            CommandType commandType = CommandType.Text)
            where TResult : class
        {
            var results = await ExecuteQueryAsync<TResult>(query, parameters, cancellationToken, commandType);
            return results.FirstOrDefault();
        }

        public async Task<TResult> ExecuteScalarAsync<TResult>(
            string query,
            IEnumerable<SqlParameter>? parameters = null,
            CancellationToken cancellationToken = default,
            CommandType commandType = CommandType.Text)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection)
            {
                CommandType = commandType,
            };

            if (parameters != null)
            {
                command.Parameters.AddRange(parameters.ToArray());
            }

            await connection.OpenAsync(cancellationToken);
            var result = await command.ExecuteScalarAsync(cancellationToken);
            return (TResult)result!;
        }

        public async Task<int> ExecuteNonQueryAsync(
            string query,
            IEnumerable<SqlParameter>? parameters = null,
            CancellationToken cancellationToken = default,
            CommandType commandType = CommandType.Text)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection)
            {
                CommandType = commandType,
            };

            if (parameters != null)
            {
                command.Parameters.AddRange(parameters.ToArray());
            }

            await connection.OpenAsync(cancellationToken);
            return await command.ExecuteNonQueryAsync(cancellationToken);
        }

        public async Task ExecuteWithTransactionAsync(
            Func<ISqlService<T>, Task> operation,
            CancellationToken cancellationToken = default)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            try
            {
                var transactionalService = new SqlService<T>(_connectionString);

                await operation(transactionalService);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
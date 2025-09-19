// <copyright file="IUnitOfWork.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Common.Interfaces.Persistence
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        IRepository<TEntity> GetRepository<TEntity>()
            where TEntity : class;

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}

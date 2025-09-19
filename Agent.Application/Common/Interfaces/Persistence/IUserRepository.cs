// <copyright file="IUserRepository.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Common.Interfaces.Persistence
{
    using Agent.Domain.Entities;

    public interface IUserRepository
    {
        User? GetUserByEmail(string email);

        void AddUser(User user);
    }
}

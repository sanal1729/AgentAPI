// <copyright file="UserRepository.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Infrastructure.Persistence.Repositories
{
    using Agent.Application.Common.Interfaces.Persistence;
    using Agent.Domain.Entities;

    public class UserRepository : IUserRepository
    {
        private static readonly List<User> User = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRepository"/> class.
        /// </summary>
        public UserRepository()
        {
        }

        public void AddUser(User user)
        {
            UserRepository.User.Add(user)
;
        }

        public User? GetUserByEmail(string email)
        {
            return User.SingleOrDefault(x => x.Email == email);
        }
    }
}

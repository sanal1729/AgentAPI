// <copyright file="AuthQueryService.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Services.Authentication.Queries
{
    using Agent.Application.Common.Interfaces.Authentication;
    using Agent.Application.Common.Interfaces.Persistence;
    using Agent.Application.Services.Authentication.Common;
    using Agent.Domain.Common.Errors;
    using Agent.Domain.Entities;
    using ErrorOr;

    public class AuthQueryService(IJwtTokenHandler jwtTokenHandler, IUserRepository userRepository) : IAuthQueryService
    {
        private readonly IJwtTokenHandler _jwtTokenHandler = jwtTokenHandler;
        private readonly IUserRepository _userRepository = userRepository;

        public ErrorOr<AuthResult> Login(string email, string password)
        {
            if (this._userRepository.GetUserByEmail(email) is not User user)
            {
                return new[] { Errors.Authentication.InvalidEmail };
            }

            // 2. validate password is correct
            if (user.PasswordHash != password)
            {
                // throw new Exception("password invalid");
                return new[] { Errors.Authentication.InvalidCredentials };
            }

            var token = this._jwtTokenHandler.GenerateAccessToken(user);

            return new AuthResult(user, token);
        }
    }
}

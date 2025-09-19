// <copyright file="AuthCommandService.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Services.Authentication.Commands
{
    using Agent.Application.Common.Interfaces.Authentication;
    using Agent.Application.Common.Interfaces.Persistence;
    using Agent.Application.Services.Authentication.Common;
    using Agent.Domain.Common.Errors;
    using Agent.Domain.Entities;
    using ErrorOr;

    public class AuthCommandService : IAuthCommandService
    {
        private readonly IJwtTokenHandler _jwtTokenHandler;
        private readonly IUserRepository _userRepository;

        public AuthCommandService(IJwtTokenHandler jwtTokenHandler, IUserRepository userRepository)

                                    => (this._jwtTokenHandler, this._userRepository) = (jwtTokenHandler, userRepository);

        public ErrorOr<AuthResult> Register(string fName, string lName, string email, string password)
        {
            if (this._userRepository.GetUserByEmail(email) is not null)
            {
                return new[] { Errors.Authentication.DuplicateEmailError };

                // return FluentResults.Result.Fail<AuthResult>(new[] {new DuplicateEmailError() });//Exception("User with given email already exist");
            }

            var user = new User
            {
               // Id = Guid.NewGuid(),
                FirstName = fName,
                LastName = lName,
                Email = email,
                PasswordHash = password,
            };

            this._userRepository.AddUser(user);

            // create user
            var token = this._jwtTokenHandler.GenerateAccessToken(user);

            return new AuthResult(user, token);
        }
    }
}

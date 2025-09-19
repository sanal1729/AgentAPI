// <copyright file="Errors.Authentication.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Domain.Common.Errors
{
    using ErrorOr;

    public static class Errors
    {
        public static class Authentication
        {
            public static Error InvalidCredentials => Error.Failure(
                code: "User.InvalidCredentials", description: "Invalid Credentials");

            public static Error InvalidEmail => Error.NotFound(
                code: "User.InvalidEmail", description: "User with given email not exist");

            public static Error DuplicateEmailError => Error.Failure(
                code: "User.DuplicateEmail", description: "User with given email exists");

            public static Error InvalidRefreshToken => Error.Failure(
                code: "User.InvalidRefreshToken", description: "User access expired, please login again");
        }
    }
}

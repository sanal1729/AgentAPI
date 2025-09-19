// <copyright file="IAuthCommandService.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Services.Authentication.Commands
{
    using Agent.Application.Services.Authentication.Common;
    using ErrorOr;

    public interface IAuthCommandService
    {
        ErrorOr<AuthResult> Register(
             string fName,
             string lName,
             string email,
             string password);
    }
}

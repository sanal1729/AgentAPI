// <copyright file="IAuthQueryService.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Services.Authentication.Queries
{
    using Agent.Application.Services.Authentication.Common;
    using ErrorOr;

    public interface IAuthQueryService
    {
        ErrorOr<AuthResult> Login(
             string email,
             string password);
    }
}

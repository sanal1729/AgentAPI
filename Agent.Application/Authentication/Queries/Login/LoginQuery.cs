// <copyright file="LoginQuery.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Authentication.Queries.Login
{
    using Agent.Application.Authentication.Common;
    using ErrorOr;
    using MediatR;

    public record LoginQuery(
        string Email,
        string Password) : IRequest<ErrorOr<AuthResult>>;
}
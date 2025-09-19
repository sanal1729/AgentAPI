// <copyright file="RegisterCommand.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Authentication.Commands.Register
{
    using Agent.Application.Authentication.Common;
    using ErrorOr;
    using MediatR;

    public record RegisterCommand(
        string FirstName,
        string LastName,
        string Email,
        string Password,
        string? PhoneNumber) : IRequest<ErrorOr<AuthResult>>;
}
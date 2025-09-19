// <copyright file="RenewalQuery.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Authentication.Queries.Renewal
{
    using Agent.Application.Authentication.Common;
    using ErrorOr;
    using MediatR;

    public record RenewalQuery(string RefreshToken) : IRequest<ErrorOr<AuthResult>>;
}
// <copyright file="ICurrentUserService.cs" company="Agent">
// © Agent 2025
// </copyright>
namespace Agent.Application.Common.Interfaces.Services
{
    public interface ICurrentUserService
    {
        string UserId { get; }

        string IpAddress { get; }

        string UserAgent { get; }
    }
}
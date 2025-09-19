// <copyright file="IServiceException.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Common.Errors
{
    using System.Net;

    public interface IServiceException
    {
        public HttpStatusCode StatusCode { get; }

        public string ErrorMessage { get; }
    }
}

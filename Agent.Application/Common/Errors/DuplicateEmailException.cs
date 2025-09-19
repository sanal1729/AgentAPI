// <copyright file="DuplicateEmailException.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Common.Errors
{
    using System;
    using System.Net;

    public class DuplicateEmailException : Exception, IServiceException
    {
        public DuplicateEmailException()
        {
        }

        public HttpStatusCode StatusCode => HttpStatusCode.Conflict;

        public string ErrorMessage => "Email already exists";
    }
}

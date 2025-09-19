// <copyright file="ErrorController.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Api.Controllers
{
    using System;
    using Agent.Application.Common.Errors;
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Mvc;

    [ApiExplorerSettings(IgnoreApi = true)]

    public class ErrorController : ControllerBase
    {
        [Route("/error")]

        public IActionResult Error()
        {
            Exception? exception = this.HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;

            var (statusCode, message) = exception switch
            {
                IServiceException serviceException => ((int)serviceException.StatusCode, serviceException.ErrorMessage),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occured"),

            };

            return this.Problem(statusCode: statusCode, title: message);
        }
    }
}

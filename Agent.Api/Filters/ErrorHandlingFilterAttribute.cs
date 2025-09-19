// <copyright file="ErrorHandlingFilterAttribute.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Api.Filters
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;

    public class ErrorHandlingFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext exceptionContext)
        {
            var exception = exceptionContext.Exception;

            exceptionContext.Result = new ObjectResult(
                new
                {
                    error = "An error occured while processing your request",
                });

            exceptionContext.ExceptionHandled = true;
        }
    }
}

// <copyright file="ErrorHandler.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Api.Middleware
{
    using System;
    using System.Net;
    using Newtonsoft.Json;

    public class ErrorHandler(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await this._next(context);
            }
            catch (Exception ex)
            {
                await this.HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var code = HttpStatusCode.InternalServerError;
            var result = JsonConvert.SerializeObject(new
            {
                error = // ex.Message

                                                                   "An error occured while processing your request",
            });
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            return context.Response.WriteAsync(result);
        }
    }
}

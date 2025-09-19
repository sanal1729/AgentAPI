// <copyright file="ApiController.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Api.Controllers
{
    using Agent.Api.Common.Http;
    using ErrorOr;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ModelBinding;

    [Authorize]

    public class ApiController : ControllerBase
    {
        protected IActionResult Problem(List<Error> errors)
        {
            if (errors.Count is 0)
            {
                return this.Problem();
            }

            if (errors.All(error => error.Type == ErrorType.Validation))
            {
                return this.ValidationProblem(errors);
            }

            this.HttpContext.Items[HttpContextItemKeys.Errors] = errors;

            return this.Problem(errors[0]);
        }

        private IActionResult Problem(Error error)
        {
            var statusCode = error.Type switch
            {
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError,
            };

            return this.Problem(statusCode: statusCode, title: error.Description);
        }

        private IActionResult ValidationProblem(List<Error> errors)
        {
            var modelStateDictionary = new ModelStateDictionary();
            foreach (var e in errors)
            {
                modelStateDictionary.AddModelError(
                e.Code, e.Description);
            }

            return this.ValidationProblem(modelStateDictionary);
        }
    }
}

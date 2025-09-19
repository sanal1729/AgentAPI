// <copyright file="ValidationBehaviors.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Common.Behaviors
{
    using ErrorOr;
    using FluentValidation;
    using MediatR;

    public class ValidationBehaviors<TRequest, TResponse>(IValidator<TRequest>? validator = null) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : IErrorOr
    {
        private readonly IValidator<TRequest>? _validator = validator;

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (this._validator is null)
            {
                return await next();
            }

            var validationResult = await this._validator.ValidateAsync(request, cancellationToken);
            if (validationResult.IsValid)
            {
                return await next();
            }

            var errors = validationResult.Errors.ConvertAll(validationFailure => Error.Validation(
                    validationFailure.PropertyName, validationFailure.ErrorMessage));

            return (dynamic)errors;
        }
    }
}

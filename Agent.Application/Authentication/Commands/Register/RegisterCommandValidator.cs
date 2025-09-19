// <copyright file="RegisterCommandValidator.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Authentication.Commands.Register
{
    using FluentValidation;

    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            this.RuleFor(x => x.FirstName).NotEmpty();

            this.RuleFor(x => x.LastName).NotEmpty();

            this.RuleFor(x => x.Email).NotEmpty();

            this.RuleFor(x => x.Password).NotEmpty();
        }
    }
}

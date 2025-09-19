// <copyright file="LoginQueryValidator.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Authentication.Queries.Login
{
    using FluentValidation;

    public class LoginQueryValidator : AbstractValidator<LoginQuery>
    {
        public LoginQueryValidator()
        {
            this.RuleFor(x => x.Email).NotEmpty();
            this.RuleFor(x => x.Password).NotEmpty()
            .WithMessage("The password must not be empty")
            .WithErrorCode("Password.NotNull");
        }
    }
}

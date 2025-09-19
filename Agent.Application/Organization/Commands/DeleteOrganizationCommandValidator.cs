// <copyright file="DeleteOrganizationCommandValidator.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Organization.Commands
{
    using FluentValidation;

    public class DeleteOrganizationCommandValidator : AbstractValidator<DeleteOrganizationCommand>
    {
        public DeleteOrganizationCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("OrganizationId is required.");
        }
    }
}
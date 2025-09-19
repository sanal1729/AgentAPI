// <copyright file="CreateBranchCommandValidator.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Organization.Commands;

using FluentValidation;

public class CreateBranchCommandValidator : AbstractValidator<CreateBranchCommand>
{
    public CreateBranchCommandValidator()
    {
        this.RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Branch name is required.");

        this.RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Branch code is required.");
    }
}

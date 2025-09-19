// <copyright file="CreateOrganizationCommandValidator.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Organization.Commands;

using FluentValidation;

public class CreateOrganizationCommandValidator : AbstractValidator<CreateOrganizationCommand>
{
    public CreateOrganizationCommandValidator()
    {
        this.RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Organization name is required.");

        this.RuleFor(x => x.CurrencyCode)
            .NotEmpty()
            .WithMessage("Currency code is required.");

        this.RuleFor(x => x.CountryCode)
            .NotEmpty()
            .WithMessage("Country code is required.");

        this.RuleFor(x => x.Branches)
            .NotEmpty()
            .WithMessage("At least one branch is required.")
            .ForEach(branch =>
            {
                branch.SetValidator(new CreateBranchCommandValidator());
            });
    }
}

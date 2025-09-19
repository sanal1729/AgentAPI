// <copyright file="UpdateOrganizationCommandValidator.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Organization.Commands
{
    using FluentValidation;

    public class UpdateOrganizationCommandValidator : AbstractValidator<UpdateOrganizationCommand>
    {
        public UpdateOrganizationCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Organization Id is required.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Organization Name is required.")
                .MaximumLength(100).WithMessage("Organization Name must not exceed 100 characters.");

            RuleFor(x => x.CountryCode)
                .NotEmpty().WithMessage("Country Code is required.");
            RuleFor(x => x.CurrencyCode)
                .NotEmpty().WithMessage("Currency Code is required.");

            RuleForEach(x => x.Branches)
                .SetValidator(new UpdateBranchCommandValidator());
        }
    }
}
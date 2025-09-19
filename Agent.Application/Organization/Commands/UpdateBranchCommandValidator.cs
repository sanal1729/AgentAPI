// <copyright file="UpdateBranchCommandValidator.cs" company="Agent">
// © Agent 2025
// </copyright>

namespace Agent.Application.Organization.Commands
{
    using FluentValidation;

    public class UpdateBranchCommandValidator : AbstractValidator<UpdateBranchCommand>
    {
        public UpdateBranchCommandValidator()
        {
            RuleFor(x => x.Id);

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Branch Name is required.")
                .MaximumLength(100).WithMessage("Branch Name must not exceed 100 characters.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Branch Code is required.")
                .MaximumLength(10).WithMessage("Branch Code must not exceed 10 characters.");
        }
    }
}
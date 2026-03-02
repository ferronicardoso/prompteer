using FluentValidation;
using Prompteer.Application.DTOs;

namespace Prompteer.Application.Validators;

public class ArchitecturalPatternValidator : AbstractValidator<ArchitecturalPatternFormDto>
{
    public ArchitecturalPatternValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(100).WithMessage("O nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("A descrição é obrigatória.")
            .MaximumLength(500).WithMessage("A descrição deve ter no máximo 500 caracteres.");
    }
}

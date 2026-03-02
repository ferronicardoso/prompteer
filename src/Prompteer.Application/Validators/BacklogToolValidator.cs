using FluentValidation;
using Prompteer.Application.DTOs;

namespace Prompteer.Application.Validators;

public class BacklogToolValidator : AbstractValidator<BacklogToolFormDto>
{
    public BacklogToolValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(100).WithMessage("O nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.DefaultInstructions)
            .NotEmpty().WithMessage("As instruções padrão são obrigatórias.");
    }
}

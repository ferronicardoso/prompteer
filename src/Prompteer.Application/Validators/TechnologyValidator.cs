using FluentValidation;
using Prompteer.Application.DTOs;

namespace Prompteer.Application.Validators;

public class TechnologyValidator : AbstractValidator<TechnologyFormDto>
{
    public TechnologyValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(100).WithMessage("O nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Version)
            .MaximumLength(50).WithMessage("A versão deve ter no máximo 50 caracteres.")
            .When(x => x.Version is not null);

        RuleFor(x => x.ShortDescription)
            .MaximumLength(300).WithMessage("A descrição deve ter no máximo 300 caracteres.")
            .When(x => x.ShortDescription is not null);
    }
}

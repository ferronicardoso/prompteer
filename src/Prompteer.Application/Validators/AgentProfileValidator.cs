using FluentValidation;
using Prompteer.Application.DTOs;

namespace Prompteer.Application.Validators;

public class AgentProfileValidator : AbstractValidator<AgentProfileFormDto>
{
    public AgentProfileValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(100).WithMessage("O nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("O papel é obrigatório.")
            .MaximumLength(500).WithMessage("O papel deve ter no máximo 500 caracteres.");

        RuleFor(x => x.KnowledgeDomain)
            .NotEmpty().WithMessage("O domínio de conhecimento é obrigatório.")
            .MaximumLength(300).WithMessage("O domínio deve ter no máximo 300 caracteres.");

        RuleFor(x => x.DefaultConstraints)
            .NotEmpty().WithMessage("As restrições padrão são obrigatórias.");
    }
}

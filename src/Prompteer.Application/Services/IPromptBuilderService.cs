using Prompteer.Application.Wizard;
using Prompteer.Domain.Entities;

namespace Prompteer.Application.Services;

public interface IPromptBuilderService
{
    Task<string> BuildAsync(WizardSessionData data);
}

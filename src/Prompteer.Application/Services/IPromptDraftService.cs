using Prompteer.Application.Wizard;

namespace Prompteer.Application.Services;

public interface IPromptDraftService
{
    Task<(Guid DraftId, WizardSessionData Data)> GetOrCreateAsync(Guid? draftId);
    Task<WizardSessionData> GetDataAsync(Guid draftId);
    Task UpdateAsync(Guid draftId, WizardSessionData data, int currentStep);
    Task DeleteAsync(Guid draftId);
}

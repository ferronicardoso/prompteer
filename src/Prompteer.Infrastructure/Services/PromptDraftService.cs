using Prompteer.Application.Services;
using Microsoft.EntityFrameworkCore;
using Prompteer.Application.Wizard;
using Prompteer.Domain.Entities;
using Prompteer.Infrastructure.Data;
using System.Text.Json;

namespace Prompteer.Infrastructure.Services;

public class PromptDraftService : IPromptDraftService
{
    private readonly AppDbContext _db;

    public PromptDraftService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(Guid DraftId, WizardSessionData Data)> GetOrCreateAsync(Guid? draftId)
    {
        if (draftId.HasValue)
        {
            var existing = await _db.PromptDrafts.FindAsync(draftId.Value);
            if (existing is not null)
            {
                var data = JsonSerializer.Deserialize<WizardSessionData>(existing.WizardDataJson) ?? new WizardSessionData();
                return (existing.Id, data);
            }
        }

        var draft = new PromptDraft();
        _db.PromptDrafts.Add(draft);
        await _db.SaveChangesAsync();
        return (draft.Id, new WizardSessionData());
    }

    public async Task<WizardSessionData> GetDataAsync(Guid draftId)
    {
        var draft = await _db.PromptDrafts.FindAsync(draftId)
            ?? throw new KeyNotFoundException("Rascunho não encontrado.");
        return JsonSerializer.Deserialize<WizardSessionData>(draft.WizardDataJson) ?? new WizardSessionData();
    }

    public async Task UpdateAsync(Guid draftId, WizardSessionData data, int currentStep)
    {
        var draft = await _db.PromptDrafts.FindAsync(draftId)
            ?? throw new KeyNotFoundException("Rascunho não encontrado.");

        draft.WizardDataJson = JsonSerializer.Serialize(data);
        draft.CurrentStep = currentStep;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid draftId)
    {
        var draft = await _db.PromptDrafts.FindAsync(draftId);
        if (draft is not null)
        {
            _db.PromptDrafts.Remove(draft);
            await _db.SaveChangesAsync();
        }
    }
}

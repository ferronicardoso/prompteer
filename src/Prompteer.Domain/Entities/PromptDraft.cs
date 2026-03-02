namespace Prompteer.Domain.Entities;

/// <summary>
/// Rascunho do wizard — persiste o estado entre steps sem ser um template finalizado.
/// Não possui soft delete; é deletado ao converter em PromptTemplate.
/// </summary>
public class PromptDraft
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Name { get; set; }
    public string WizardDataJson { get; set; } = "{}";
    public int CurrentStep { get; set; } = 1;
    public Guid? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

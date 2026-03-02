using Prompteer.Application.DTOs;
using Prompteer.Application.Wizard;

namespace Prompteer.Web.Models;

public class WizardViewModel
{
    public Guid DraftId { get; set; }
    public int CurrentStep { get; set; }
    public WizardSessionData Data { get; set; } = new();

    // Step 1 options
    public IEnumerable<AgentProfileDto> AgentProfiles { get; set; } = [];

    // Step 2 options
    public IEnumerable<BacklogToolDto> BacklogTools { get; set; } = [];

    // Step 3 options
    public IEnumerable<TechnologySelectDto> Technologies { get; set; } = [];

    // Step 4 options
    public IEnumerable<ArchitecturalPatternDto> ArchitecturalPatterns { get; set; } = [];

    // Step 9
    public string? GeneratedPrompt { get; set; }
}

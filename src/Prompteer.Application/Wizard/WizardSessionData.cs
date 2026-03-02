namespace Prompteer.Application.Wizard;

/// <summary>
/// Dados do wizard armazenados como JSON em PromptDraft.WizardDataJson.
/// </summary>
public class WizardSessionData
{
    // Step 1 — Agente
    public Guid? AgentProfileId { get; set; }

    // Step 2 — Backlog
    public Guid? BacklogToolId { get; set; }
    public string? BacklogInstructions { get; set; }

    // Step 3 — Projeto
    public string? ProjectName { get; set; }
    public string? ProjectDescription { get; set; }
    public List<Guid> TechnologyIds { get; set; } = new();

    // Step 4 — Arquitetura
    public List<Guid> ArchitecturalPatternIds { get; set; } = new();
    public List<string> RequiredPackages { get; set; } = new();
    public string? CodeConventions { get; set; }

    // Step 5 — Ambiente (opcional)
    public bool IncludeEnvironment { get; set; } = true;
    public List<string> DeploymentTargets { get; set; } = new();
    public string? GitStrategy { get; set; }
    public bool IncludeCICD { get; set; }
    public string? CICDTool { get; set; }

    // Step 6 — Testes (opcional)
    public bool IncludeTests { get; set; } = true;
    public List<string> TestTypes { get; set; } = new();
    public string? TestFramework { get; set; }
    public int? MinCoverage { get; set; }
    public string? TestObservations { get; set; }

    // Step 7 — Módulos
    public List<WizardModule> Modules { get; set; } = new();

    // Step 8 — Regras
    public List<string> RuleFlags { get; set; } = new();
    public string? AdditionalRules { get; set; }
}

public class WizardModule
{
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public List<string> SubItems { get; set; } = new();
}

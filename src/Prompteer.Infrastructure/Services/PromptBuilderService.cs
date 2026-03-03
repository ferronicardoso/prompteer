using Prompteer.Application.Services;
using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Prompteer.Application.Wizard;
using Prompteer.Infrastructure.Data;

namespace Prompteer.Infrastructure.Services;

public class PromptBuilderService : IPromptBuilderService
{
    private readonly AppDbContext _db;

    public PromptBuilderService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<string> BuildAsync(WizardSessionData data)
    {
        var sb = new StringBuilder();

        await AppendAgentSectionAsync(sb, data);
        await AppendBacklogSectionAsync(sb, data);
        AppendProjectSection(sb, data);
        await AppendArchitectureSectionAsync(sb, data);

        if (data.IncludeEnvironment)
            AppendEnvironmentSection(sb, data);

        if (data.IncludeTests)
            AppendTestsSection(sb, data);

        AppendModulesSection(sb, data);
        AppendRulesSection(sb, data);

        return sb.ToString().Trim();
    }

    // ─── Step 1: Agente ──────────────────────────────────────────────────────
    private async Task AppendAgentSectionAsync(StringBuilder sb, WizardSessionData data)
    {
        if (!data.AgentProfileId.HasValue) return;

        var profile = await _db.AgentProfiles.FindAsync(data.AgentProfileId.Value);
        if (profile is null) return;

        sb.AppendLine($"# {T("AI Agent Instructions", "Instruções para Agente de IA")}");
        sb.AppendLine();
        sb.AppendLine($"## {T("Agent Profile", "Perfil do Agente")}");
        sb.AppendLine();
        sb.AppendLine($"{profile.Role}, {T("with deep knowledge in", "com profundo conhecimento em")} **{profile.KnowledgeDomain}**.");
        sb.AppendLine();
        sb.AppendLine($"**{T("Tone", "Tom")}:** {GetToneDisplay(profile.Tone)}");
        sb.AppendLine();
        sb.AppendLine($"### {T("Behavior Constraints", "Restrições de Comportamento")}");
        sb.AppendLine();
        sb.AppendLine(profile.DefaultConstraints);
        sb.AppendLine();
    }

    // ─── Step 2: Backlog ─────────────────────────────────────────────────────
    private async Task AppendBacklogSectionAsync(StringBuilder sb, WizardSessionData data)
    {
        if (!data.BacklogToolId.HasValue) return;

        var tool = await _db.BacklogTools.FindAsync(data.BacklogToolId.Value);
        if (tool is null) return;

        var instructions = string.IsNullOrWhiteSpace(data.BacklogInstructions)
            ? tool.DefaultInstructions
            : data.BacklogInstructions;

        sb.AppendLine($"---");
        sb.AppendLine();
        sb.AppendLine(instructions.Trim());
        sb.AppendLine();
    }

    // ─── Step 3: Projeto ─────────────────────────────────────────────────────
    private void AppendProjectSection(StringBuilder sb, WizardSessionData data)
    {
        if (string.IsNullOrWhiteSpace(data.ProjectName)) return;

        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine($"## {T("Project", "Projeto")}: {data.ProjectName}");
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(data.ProjectDescription))
        {
            sb.AppendLine($"### {T("General Description", "Descrição Geral")}");
            sb.AppendLine();
            sb.AppendLine(data.ProjectDescription.Trim());
            sb.AppendLine();
        }
    }

    // ─── Step 4: Arquitetura ─────────────────────────────────────────────────
    private async Task AppendArchitectureSectionAsync(StringBuilder sb, WizardSessionData data)
    {
        var hasTechs        = data.TechnologyIds.Any();
        var hasPatterns     = data.ArchitecturalPatternIds.Any();
        var hasPackages     = data.RequiredPackages.Any();
        var hasConventions  = !string.IsNullOrWhiteSpace(data.CodeConventions);

        if (!hasTechs && !hasPatterns && !hasPackages && !hasConventions) return;

        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine($"## {T("Tech Stack and Architecture", "Stack Tecnológica e Arquitetura")}");
        sb.AppendLine();

        if (data.TechnologyIds.Any())
        {
            var techs = await _db.Technologies
                .Where(t => data.TechnologyIds.Contains(t.Id))
                .OrderBy(t => t.Category).ThenBy(t => t.Name)
                .ToListAsync();

            sb.AppendLine($"### {T("Technologies", "Tecnologias")}");
            sb.AppendLine();
            foreach (var tech in techs)
            {
                var version = tech.Version is not null ? $" {tech.Version}" : string.Empty;
                sb.AppendLine($"- **{tech.Name}{version}**{(tech.ShortDescription is not null ? $" — {tech.ShortDescription}" : string.Empty)}");
            }
            sb.AppendLine();
        }

        if (data.ArchitecturalPatternIds.Any())
        {
            var patterns = await _db.ArchitecturalPatterns
                .Where(p => data.ArchitecturalPatternIds.Contains(p.Id))
                .OrderBy(p => p.Name)
                .ToListAsync();

            sb.AppendLine($"### {T("Architectural Patterns", "Padrões Arquiteturais")}");
            sb.AppendLine();
            foreach (var p in patterns)
                sb.AppendLine($"- **{p.Name}** — {p.Description}");
            sb.AppendLine();
        }

        if (data.RequiredPackages.Any())
        {
            sb.AppendLine($"### {T("Required Packages and Libraries", "Pacotes e Bibliotecas Obrigatórios")}");
            sb.AppendLine();
            foreach (var pkg in data.RequiredPackages)
                sb.AppendLine($"- `{pkg}`");
            sb.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(data.CodeConventions))
        {
            sb.AppendLine($"### {T("Code Conventions", "Convenções de Código")}");
            sb.AppendLine();
            sb.AppendLine(data.CodeConventions.Trim());
            sb.AppendLine();
        }
    }

    // ─── Step 5: Ambiente ────────────────────────────────────────────────────
    private void AppendEnvironmentSection(StringBuilder sb, WizardSessionData data)
    {
        var hasDeployment = data.DeploymentTargets.Any();
        var hasGit        = !string.IsNullOrWhiteSpace(data.GitStrategy);
        var hasCICD       = data.IncludeCICD && !string.IsNullOrWhiteSpace(data.CICDTool);

        if (!hasDeployment && !hasGit && !hasCICD) return;

        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine($"## {T("Environment and Infrastructure", "Ambiente e Infraestrutura")}");
        sb.AppendLine();

        if (data.DeploymentTargets.Any())
        {
            sb.AppendLine($"**{T("Deployment target", "Destino de deploy")}:** {string.Join(", ", data.DeploymentTargets)}");
            sb.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(data.GitStrategy))
        {
            sb.AppendLine($"**{T("Git strategy", "Estratégia Git")}:** {data.GitStrategy}");
            sb.AppendLine();
        }

        if (data.IncludeCICD && !string.IsNullOrWhiteSpace(data.CICDTool))
        {
            sb.AppendLine($"**CI/CD:** {data.CICDTool}");
            sb.AppendLine();
        }
    }

    // ─── Step 6: Testes ──────────────────────────────────────────────────────
    private void AppendTestsSection(StringBuilder sb, WizardSessionData data)
    {
        var hasTypes        = data.TestTypes.Any();
        var hasFramework    = !string.IsNullOrWhiteSpace(data.TestFramework);
        var hasCoverage     = data.MinCoverage.HasValue;
        var hasObservations = !string.IsNullOrWhiteSpace(data.TestObservations);

        if (!hasTypes && !hasFramework && !hasCoverage && !hasObservations) return;

        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine($"## {T("Testing Strategy", "Estratégia de Testes")}");
        sb.AppendLine();

        if (data.TestTypes.Any())
        {
            sb.AppendLine($"**{T("Test types", "Tipos de testes")}:** {string.Join(", ", data.TestTypes)}");
            sb.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(data.TestFramework))
        {
            sb.AppendLine($"**Framework:** {data.TestFramework}");
            sb.AppendLine();
        }

        if (data.MinCoverage.HasValue)
        {
            sb.AppendLine($"**{T("Minimum coverage", "Cobertura mínima")}:** {data.MinCoverage}%");
            sb.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(data.TestObservations))
        {
            sb.AppendLine($"**{T("Notes", "Observações")}:**");
            sb.AppendLine();
            sb.AppendLine(data.TestObservations.Trim());
            sb.AppendLine();
        }
    }

    // ─── Step 7: Módulos ─────────────────────────────────────────────────────
    private void AppendModulesSection(StringBuilder sb, WizardSessionData data)
    {
        if (!data.Modules.Any()) return;

        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine($"## {T("Project Modules", "Módulos do Projeto")}");
        sb.AppendLine();
        sb.AppendLine(T("Implement the modules in the following order:", "Implemente os módulos na seguinte ordem:"));
        sb.AppendLine();

        foreach (var module in data.Modules.OrderBy(m => m.Order))
        {
            sb.AppendLine($"### {module.Order + 1}. {module.Name}");
            sb.AppendLine();
            if (module.SubItems.Any())
            {
                foreach (var item in module.SubItems)
                    sb.AppendLine($"- {item}");
                sb.AppendLine();
            }
        }
    }

    // ─── Step 8: Regras ──────────────────────────────────────────────────────
    private void AppendRulesSection(StringBuilder sb, WizardSessionData data)
    {
        var hasFlags = data.RuleFlags.Any();
        var hasCustom = !string.IsNullOrWhiteSpace(data.AdditionalRules);
        if (!hasFlags && !hasCustom) return;

        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine($"## {T("Rules and Guidelines", "Regras e Diretrizes")}");
        sb.AppendLine();

        if (hasFlags)
        {
            foreach (var flag in data.RuleFlags)
                sb.AppendLine($"- {flag}");
            sb.AppendLine();
        }

        if (hasCustom)
        {
            sb.AppendLine(data.AdditionalRules!.Trim());
            sb.AppendLine();
        }
    }

    private static string GetToneDisplay(Domain.Enums.ToneType tone) => tone switch
    {
        Domain.Enums.ToneType.Technical => T("Technical", "Técnico"),
        Domain.Enums.ToneType.Didactic  => T("Didactic",  "Didático"),
        Domain.Enums.ToneType.Direct    => T("Direct",    "Direto"),
        Domain.Enums.ToneType.Detailed  => T("Detailed",  "Detalhista"),
        _                               => tone.ToString()
    };

    private static string T(string en, string ptBr) =>
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
            .Equals("pt", StringComparison.OrdinalIgnoreCase) ? ptBr : en;
}

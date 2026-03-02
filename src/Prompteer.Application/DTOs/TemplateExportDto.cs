namespace Prompteer.Application.DTOs;

/// <summary>
/// Formato portável de exportação/importação de um template.
/// Contém o prompt gerado + WizardData + nomes resolvidos de tecnologias e padrões.
/// </summary>
public class TemplateExportDto
{
    /// <summary>Versão do schema, para compatibilidade futura.</summary>
    public string SchemaVersion { get; set; } = "1.0";
    public DateTime ExportedAt { get; set; } = DateTime.UtcNow;
    public string ExportedBy { get; set; } = string.Empty;

    public List<TemplateExportItemDto> Templates { get; set; } = new();
}

public class TemplateExportItemDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int VersionNumber { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>O prompt gerado (markdown).</summary>
    public string GeneratedPrompt { get; set; } = string.Empty;

    /// <summary>Dados estruturados do wizard (JSON inline).</summary>
    public string WizardDataJson { get; set; } = string.Empty;

    /// <summary>Nomes das tecnologias (para resolução na importação).</summary>
    public List<string> TechnologyNames { get; set; } = new();

    /// <summary>Nomes dos padrões arquiteturais (para resolução na importação).</summary>
    public List<string> PatternNames { get; set; } = new();
}

public class ImportResultDto
{
    public int Imported { get; set; }
    public int Skipped { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> ImportedNames { get; set; } = new();
}

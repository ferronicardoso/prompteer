namespace Prompteer.Application.Services;

public interface IAIService
{
    /// <summary>Generates text for a specific field given contextual information.</summary>
    /// <param name="fieldType">Identifies which field to generate (e.g. "AgentRole", "TechDescription")</param>
    /// <param name="context">Key-value pairs providing context (e.g. entity name, category)</param>
    Task<string> GenerateFieldAsync(string fieldType, Dictionary<string, string> context);

    /// <summary>Returns true if AI is configured and ready to use.</summary>
    Task<bool> IsConfiguredAsync();
}

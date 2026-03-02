using Prompteer.Application.DTOs;

namespace Prompteer.Application.Services;

public interface IAIService
{
    /// <summary>Generates text for a specific field given contextual information.</summary>
    Task<string> GenerateFieldAsync(string fieldType, Dictionary<string, string> context);

    /// <summary>Returns true if AI is configured and ready to use.</summary>
    Task<bool> IsConfiguredAsync();

    /// <summary>Fetches available models for the given provider using the given API key.</summary>
    Task<IEnumerable<AIModelDto>> GetModelsAsync(string provider, string apiKey);
}

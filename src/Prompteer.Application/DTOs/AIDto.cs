namespace Prompteer.Application.DTOs;

public class AISettingsDto
{
    public string Provider { get; set; } = "OpenAI";
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4o-mini";
    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey);
}

public class AIModelDto
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

public class AIGenerateRequestDto
{
    public string FieldType { get; set; } = string.Empty;
    public Dictionary<string, string> Context { get; set; } = new();
}

public class AIGenerateResponseDto
{
    public bool Success { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? Error { get; set; }
}

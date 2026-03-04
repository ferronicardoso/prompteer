using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using OpenAI.Chat;
using Prompteer.Application.DTOs;
using Prompteer.Application.Services;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Prompteer.Infrastructure.Services;

public class OpenAIService(IAppSettingService settings, IHttpClientFactory httpClientFactory) : IAIService
{
    private static readonly Dictionary<string, string> Prompts = new()
    {
        ["AgentRole"] = """
            You are an expert in AI prompt engineering.
            Given the agent profile name below, write a clear and concise role sentence in the format "You are a [role]..." with 2-3 lines.
            Write only the role text, without quotes, without introduction.
            """,

        ["AgentKnowledgeDomain"] = """
            You are an expert in prompt engineering.
            Concisely list (in one line, comma-separated) the main knowledge domains and relevant technologies for the given agent profile.
            Write only the list, without introduction.
            """,

        ["AgentConstraints"] = """
            You are an expert in AI prompt engineering.
            For the given agent profile, write a clear set of behavioral constraints and guidelines (3 to 5 items in bullets or flowing text).
            Focus on behaviors such as: when to ask for confirmation, when not to assume, expected response formats.
            Write only the constraints, without introduction.
            """,

        ["TechDescription"] = """
            You are a software technology expert.
            Write a short description (maximum 2 lines) about the given technology, suitable as a selection tooltip.
            Write only the description, without quotes, without introduction.
            """,

        ["PatternDescription"] = """
            You are a software architecture expert.
            Write a concise description (2-4 lines) about the given architectural pattern: what it is, when to use it, and its main benefit.
            Write only the description, without introduction.
            """,

        ["BacklogInstructions"] = """
            You are an expert in software project management.
            Write the standard usage instructions for the given backlog tool for use by an AI agent.
            Include: how to register items, expected format, best practices. Use markdown with bullets. Maximum 10 lines.
            Write only the instructions, without introduction.
            """,

        ["ProjectDescription"] = """
            You are a software documentation expert.
            Based on the project name and technologies provided, write a general project description paragraph (3-5 lines).
            Mention the purpose, main stack and target audience if possible.
            Write only the description, without introduction.
            """,

        ["CodeConventions"] = """
            You are a software engineering expert.
            Based on the project name, architectural patterns and packages provided, write a set of code conventions and best practices (5-8 items as bullets).
            Focus on: naming conventions, code organization, formatting, documentation and patterns specific to the stack.
            Write only the conventions list, without introduction.
            """,

        ["TestObservations"] = """
            You are a software testing expert.
            Based on the project and test configuration provided, write key observations and guidelines for the testing strategy (4-6 items as bullets).
            Focus on: test structure, mocking strategies, test data management and coverage priorities.
            Write only the observations, without introduction.
            """,

        ["Modules"] = """
            You are a software architect.
            Based on the project information provided, define the main modules and their sub-items for the application.
            Return ONLY a valid JSON array — no markdown fences, no explanation — in this exact format:
            [{"name":"ModuleName","subItems":["SubItem1","SubItem2"]}]
            Suggest 4-8 modules with 2-4 sub-items each. Module and sub-item names should be concise (1-3 words).
            """,

        ["AdditionalRules"] = """
            You are a software development expert.
            Based on the project context provided, write additional development rules and guidelines for an AI coding agent (4-6 items as bullets).
            Focus on: code quality, architecture adherence, error handling, documentation and testing requirements specific to this project.
            Write only the rules, without introduction.
            """,

        ["TemplateDescription"] = """
            You are a technical documentation expert.
            Based on the template name and project context provided, write a brief description (2-3 lines) summarising what this prompt template covers.
            Mention the main technology stack and purpose.
            Write only the description, without introduction.
            """,
    };

    public async Task<bool> IsConfiguredAsync()
    {
        var key = await settings.GetAsync("AI:ApiKey");
        return !string.IsNullOrWhiteSpace(key);
    }

    public async Task<string> GenerateFieldAsync(string fieldType, Dictionary<string, string> context)
    {
        var provider = await settings.GetAsync("AI:Provider") ?? "OpenAI";
        var apiKey = await settings.GetAsync("AI:ApiKey")
            ?? throw new InvalidOperationException(T(
                "AI API key not configured. Go to Settings to enter your API Key.",
                "Chave de API da IA não configurada. Acesse Configurações para inserir sua API Key."));

        var model = await settings.GetAsync("AI:Model") ?? (provider == "Anthropic" ? "claude-haiku-4-5" : "gpt-4o-mini");

        if (!Prompts.TryGetValue(fieldType, out var systemPrompt))
            throw new ArgumentException($"Unknown field type: {fieldType}");

        systemPrompt = systemPrompt + BuildLanguageInstruction();

        var userMessage = BuildUserMessage(fieldType, context);

        return provider == "Anthropic"
            ? await GenerateAnthropicAsync(apiKey, model, systemPrompt, userMessage)
            : await GenerateOpenAIAsync(apiKey, model, systemPrompt, userMessage);
    }

    public async Task<IEnumerable<AIModelDto>> GetModelsAsync(string provider, string apiKey)
    {
        return provider == "Anthropic"
            ? await GetAnthropicModelsAsync(apiKey)
            : await GetOpenAIModelsAsync(apiKey);
    }

    // ─── OpenAI generation ───────────────────────────────────────────────────
    private static async Task<string> GenerateOpenAIAsync(string apiKey, string model, string systemPrompt, string userMessage)
    {
        var client = new ChatClient(model: model, apiKey: apiKey);
        var completion = await client.CompleteChatAsync(
        [
            ChatMessage.CreateSystemMessage(systemPrompt),
            ChatMessage.CreateUserMessage(userMessage)
        ]);
        return completion.Value.Content[0].Text.Trim();
    }

    // ─── Anthropic generation ────────────────────────────────────────────────
    private async Task<string> GenerateAnthropicAsync(string apiKey, string model, string systemPrompt, string userMessage)
    {
        var http = httpClientFactory.CreateClient();
        http.DefaultRequestHeaders.Add("x-api-key", apiKey);
        http.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

        var body = JsonSerializer.Serialize(new
        {
            model,
            max_tokens = 1024,
            system = systemPrompt,
            messages = new[] { new { role = "user", content = userMessage } }
        });

        var response = await http.PostAsync(
            "https://api.anthropic.com/v1/messages",
            new StringContent(body, Encoding.UTF8, "application/json"));

        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("content")[0].GetProperty("text").GetString()!.Trim();
    }

    // ─── Model listing ───────────────────────────────────────────────────────
    private async Task<IEnumerable<AIModelDto>> GetOpenAIModelsAsync(string apiKey)
    {
        var http = httpClientFactory.CreateClient();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var response = await http.GetAsync("https://api.openai.com/v1/models");
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var prefixes = new[] { "gpt-4", "gpt-3.5", "o1", "o3", "o4" };

        return doc.RootElement.GetProperty("data")
            .EnumerateArray()
            .Select(m => m.GetProperty("id").GetString()!)
            .Where(id => prefixes.Any(p => id.StartsWith(p)))
            .OrderBy(id => id)
            .Select(id => new AIModelDto { Id = id, DisplayName = id })
            .ToList();
    }

    private async Task<IEnumerable<AIModelDto>> GetAnthropicModelsAsync(string apiKey)
    {
        var http = httpClientFactory.CreateClient();
        http.DefaultRequestHeaders.Add("x-api-key", apiKey);
        http.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

        var response = await http.GetAsync("https://api.anthropic.com/v1/models");
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        return doc.RootElement.GetProperty("data")
            .EnumerateArray()
            .Select(m => new AIModelDto
            {
                Id = m.GetProperty("id").GetString()!,
                DisplayName = m.TryGetProperty("display_name", out var dn) ? dn.GetString()! : m.GetProperty("id").GetString()!
            })
            .OrderBy(m => m.DisplayName)
            .ToList();
    }

    // Returns a language directive appended to system prompts so the AI responds in the user's language.
    // Currently supports Brazilian Portuguese (pt-*) and falls back to English for all other cultures.
    private static string BuildLanguageInstruction()
    {
        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
            .Equals("pt", StringComparison.OrdinalIgnoreCase)
            ? "\n\nRespond in Brazilian Portuguese."
            : "\n\nRespond in English.";
    }

    private static string BuildUserMessage(string fieldType, Dictionary<string, string> context)
    {
        var parts = new List<string>();

        if (context.TryGetValue("name", out var name) && !string.IsNullOrWhiteSpace(name))
            parts.Add($"Name: {name}");

        if (context.TryGetValue("category", out var cat) && !string.IsNullOrWhiteSpace(cat))
            parts.Add($"Category: {cat}");

        if (context.TryGetValue("ecosystem", out var eco) && !string.IsNullOrWhiteSpace(eco))
            parts.Add($"Ecosystem: {eco}");

        if (context.TryGetValue("tone", out var tone) && !string.IsNullOrWhiteSpace(tone))
            parts.Add($"Tone: {tone}");

        if (context.TryGetValue("technologies", out var techs) && !string.IsNullOrWhiteSpace(techs))
            parts.Add($"Technologies: {techs}");

        if (context.TryGetValue("extra", out var extra) && !string.IsNullOrWhiteSpace(extra))
            parts.Add(extra);

        return string.Join("\n", parts);
    }

    // Selects en or ptBr string based on current UI culture — same pattern as PromptBuilderService.
    private static string T(string en, string ptBr) =>
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
            .Equals("pt", StringComparison.OrdinalIgnoreCase) ? ptBr : en;
}

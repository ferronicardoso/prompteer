using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using OpenAI.Chat;
using Prompteer.Application.DTOs;
using Prompteer.Application.Services;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Prompteer.Infrastructure.Services;

public class OpenAIService(IAppSettingService settings, IHttpClientFactory httpClientFactory) : IAIService
{
    private static readonly Dictionary<string, string> Prompts = new()
    {
        ["AgentRole"] = """
            Você é um especialista em prompt engineering para agentes de IA.
            Dado o nome do perfil de agente abaixo, escreva uma frase de papel clara e objetiva no formato "Você é um(a) [papel]..." com 2-3 linhas.
            Escreva apenas o texto do papel, sem aspas, sem introdução.
            """,

        ["AgentKnowledgeDomain"] = """
            Você é um especialista em prompt engineering.
            Liste de forma concisa (em uma linha, separado por vírgulas) os principais domínios de conhecimento e tecnologias relevantes para o perfil de agente informado.
            Escreva apenas a lista, sem introdução.
            """,

        ["AgentConstraints"] = """
            Você é um especialista em prompt engineering para agentes de IA.
            Para o perfil de agente informado, escreva um conjunto de restrições e diretrizes comportamentais claras (3 a 5 itens em bullets ou texto corrido).
            Foque em comportamentos como: quando pedir confirmação, quando não assumir, formatos de resposta esperados.
            Escreva apenas as restrições, sem introdução.
            """,

        ["TechDescription"] = """
            Você é um especialista em tecnologia de software.
            Escreva uma descrição curta (máximo 2 linhas) e objetiva sobre a tecnologia informada, adequada para tooltip de seleção.
            Escreva apenas a descrição, sem aspas, sem introdução.
            """,

        ["PatternDescription"] = """
            Você é um especialista em arquitetura de software.
            Escreva uma descrição concisa (2-4 linhas) sobre o padrão arquitetural informado: o que é, quando usar e principal benefício.
            Escreva apenas a descrição, sem introdução.
            """,

        ["BacklogInstructions"] = """
            Você é um especialista em gestão de projetos de software.
            Escreva as instruções padrão de uso da ferramenta de backlog informada para uso por um agente de IA.
            Inclua: como registrar itens, formato esperado, boas práticas. Use markdown com bullets. Máximo 10 linhas.
            Escreva apenas as instruções, sem introdução.
            """,

        ["ProjectDescription"] = """
            Você é um especialista em documentação de software.
            Com base no nome e tecnologias do projeto informados, escreva um parágrafo de descrição geral do projeto (3-5 linhas).
            Mencione o propósito, stack principal e público-alvo se possível.
            Escreva apenas a descrição, sem introdução.
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
            ?? throw new InvalidOperationException("Chave de API da IA não configurada. Acesse Configurações para inserir sua API Key.");

        var model = await settings.GetAsync("AI:Model") ?? (provider == "Anthropic" ? "claude-haiku-4-5" : "gpt-4o-mini");

        if (!Prompts.TryGetValue(fieldType, out var systemPrompt))
            throw new ArgumentException($"Tipo de campo desconhecido: {fieldType}");

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

    private static string BuildUserMessage(string fieldType, Dictionary<string, string> context)
    {
        var parts = new List<string>();

        if (context.TryGetValue("name", out var name) && !string.IsNullOrWhiteSpace(name))
            parts.Add($"Nome: {name}");

        if (context.TryGetValue("category", out var cat) && !string.IsNullOrWhiteSpace(cat))
            parts.Add($"Categoria: {cat}");

        if (context.TryGetValue("ecosystem", out var eco) && !string.IsNullOrWhiteSpace(eco))
            parts.Add($"Ecossistema: {eco}");

        if (context.TryGetValue("tone", out var tone) && !string.IsNullOrWhiteSpace(tone))
            parts.Add($"Tom: {tone}");

        if (context.TryGetValue("technologies", out var techs) && !string.IsNullOrWhiteSpace(techs))
            parts.Add($"Tecnologias: {techs}");

        if (context.TryGetValue("extra", out var extra) && !string.IsNullOrWhiteSpace(extra))
            parts.Add(extra);

        return string.Join("\n", parts);
    }
}

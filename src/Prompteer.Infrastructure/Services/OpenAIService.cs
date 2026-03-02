using OpenAI;
using OpenAI.Chat;
using Prompteer.Application.Services;

namespace Prompteer.Infrastructure.Services;

public class OpenAIService(IAppSettingService settings) : IAIService
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
        var apiKey = await settings.GetAsync("AI:ApiKey")
            ?? throw new InvalidOperationException("Chave de API da IA não configurada. Acesse Configurações para inserir sua API Key.");

        var model = await settings.GetAsync("AI:Model") ?? "gpt-4o-mini";

        if (!Prompts.TryGetValue(fieldType, out var systemPrompt))
            throw new ArgumentException($"Tipo de campo desconhecido: {fieldType}");

        var userMessage = BuildUserMessage(fieldType, context);

        var client = new ChatClient(model: model, apiKey: apiKey);
        var completion = await client.CompleteChatAsync(
        [
            ChatMessage.CreateSystemMessage(systemPrompt),
            ChatMessage.CreateUserMessage(userMessage)
        ]);

        return completion.Value.Content[0].Text.Trim();
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

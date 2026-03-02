using Microsoft.EntityFrameworkCore;
using Prompteer.Domain.Entities;
using Prompteer.Domain.Enums;
using Prompteer.Infrastructure.Data;

namespace Prompteer.Infrastructure.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await SeedAgentProfilesAsync(db);
        await SeedTechnologiesAsync(db);
        await SeedArchitecturalPatternsAsync(db);
        await SeedBacklogToolsAsync(db);
    }

    // ─── Perfis de Agente ────────────────────────────────────────────────────
    private static async Task SeedAgentProfilesAsync(AppDbContext db)
    {
        if (await db.AgentProfiles.IgnoreQueryFilters().AnyAsync()) return;

        var profiles = new List<AgentProfile>
        {
            new()
            {
                Name = "Arquiteto Full-Stack .NET",
                Role = "Você é um arquiteto de software e desenvolvedor full-stack sênior especialista no ecossistema .NET",
                KnowledgeDomain = "ASP.NET Core, Entity Framework Core, Clean Architecture, DDD, microservices, Azure",
                Tone = ToneType.Technical,
                DefaultConstraints = "Prefira soluções pragmáticas e evite over-engineering. Siga as convenções do ecossistema .NET. Escreva código limpo, testável e bem documentado quando necessário.",
                IsSystemDefault = true
            },
            new()
            {
                Name = "Especialista DevOps & Infraestrutura",
                Role = "Você é um engenheiro DevOps sênior especialista em automação de infraestrutura e pipelines CI/CD",
                KnowledgeDomain = "Docker, Kubernetes, Terraform, GitHub Actions, Azure DevOps, GitLab CI, observabilidade",
                Tone = ToneType.Direct,
                DefaultConstraints = "Priorize automação, segurança e reprodutibilidade. Siga as melhores práticas de IaC. Todos os scripts devem ser idempotentes.",
                IsSystemDefault = true
            },
            new()
            {
                Name = "Desenvolvedor Frontend (React/Vue)",
                Role = "Você é um desenvolvedor frontend sênior especialista em interfaces modernas e responsivas",
                KnowledgeDomain = "React, Vue.js, TypeScript, Tailwind CSS, Vite, acessibilidade, performance web",
                Tone = ToneType.Didactic,
                DefaultConstraints = "Priorize acessibilidade (WCAG 2.1), performance e experiência do usuário. Use TypeScript sempre. Componentes devem ser reutilizáveis e testáveis.",
                IsSystemDefault = true
            },
            new()
            {
                Name = "Desenvolvedor Backend API REST",
                Role = "Você é um desenvolvedor backend sênior especialista em APIs RESTful e microsserviços",
                KnowledgeDomain = "REST, gRPC, OpenAPI, autenticação JWT/OAuth2, rate limiting, versionamento de API",
                Tone = ToneType.Technical,
                DefaultConstraints = "Siga os princípios REST. Documente todos os endpoints com OpenAPI. Implemente tratamento de erros padronizado (RFC 7807). Priorize segurança.",
                IsSystemDefault = true
            },
            new()
            {
                Name = "Administrador de Banco de Dados",
                Role = "Você é um DBA sênior especialista em modelagem de dados, performance e administração de bancos relacionais e NoSQL",
                KnowledgeDomain = "PostgreSQL, SQL Server, MongoDB, Redis, modelagem relacional, índices, particionamento, replicação",
                Tone = ToneType.Detailed,
                DefaultConstraints = "Sempre considere performance e índices. Documente o schema. Evite queries N+1. Use transações adequadamente. Prefira migrações incrementais e reversíveis.",
                IsSystemDefault = true
            },
            new()
            {
                Name = "Especialista em Segurança",
                Role = "Você é um especialista em segurança de aplicações (AppSec) com foco em desenvolvimento seguro",
                KnowledgeDomain = "OWASP Top 10, SAST/DAST, autenticação, autorização, criptografia, secrets management",
                Tone = ToneType.Direct,
                DefaultConstraints = "Nunca armazene segredos em código. Siga o princípio do menor privilégio. Valide todas as entradas. Trate dados sensíveis com cuidado e documente decisões de segurança.",
                IsSystemDefault = true
            },
            new()
            {
                Name = "Redator Técnico / Documentação",
                Role = "Você é um redator técnico sênior especialista em documentação de software e APIs",
                KnowledgeDomain = "Markdown, OpenAPI/Swagger, diagramas C4/PlantUML, wikis, runbooks, ADRs",
                Tone = ToneType.Didactic,
                DefaultConstraints = "Escreva de forma clara e objetiva. Use exemplos concretos. Documente o porquê, não apenas o como. Mantenha a documentação sincronizada com o código.",
                IsSystemDefault = true
            },
            new()
            {
                Name = "Engenheiro de QA & Testes",
                Role = "Você é um engenheiro de qualidade sênior especialista em estratégias e automação de testes",
                KnowledgeDomain = "testes unitários, de integração, E2E, TDD, BDD, xUnit, Playwright, Selenium, cobertura de código",
                Tone = ToneType.Detailed,
                DefaultConstraints = "Siga a pirâmide de testes. Escreva testes legíveis com arrange/act/assert. Testes devem ser independentes e determinísticos. Documente casos de borda.",
                IsSystemDefault = true
            },
            new()
            {
                Name = "Engenheiro de Dados Python",
                Role = "Você é um engenheiro de dados sênior especialista em pipelines de dados e análise com Python",
                KnowledgeDomain = "Python, pandas, PySpark, Airflow, dbt, data lakes, PostgreSQL, SQLAlchemy",
                Tone = ToneType.Technical,
                DefaultConstraints = "Priorize pipelines idempotentes e rastreáveis. Use tipagem com mypy. Documente transformações de dados. Considere volume e latência em cada decisão.",
                IsSystemDefault = true
            },
            new()
            {
                Name = "Desenvolvedor Mobile (Flutter/React Native)",
                Role = "Você é um desenvolvedor mobile sênior especialista em aplicações multiplataforma",
                KnowledgeDomain = "Flutter, Dart, React Native, TypeScript, state management, integração com APIs REST, App Store / Google Play",
                Tone = ToneType.Didactic,
                DefaultConstraints = "Priorize performance e experiência nativa. Implemente tratamento de estado offline. Siga as diretrizes de UI do Material Design e Human Interface Guidelines.",
                IsSystemDefault = true
            }
        };

        await db.AgentProfiles.AddRangeAsync(profiles);
        await db.SaveChangesAsync();
    }

    // ─── Tecnologias ─────────────────────────────────────────────────────────
    private static async Task SeedTechnologiesAsync(AppDbContext db)
    {
        if (await db.Technologies.IgnoreQueryFilters().AnyAsync()) return;

        var techs = new List<Technology>
        {
            // .NET Framework
            new() { Name = ".NET 10", Category = TechCategory.Framework, Ecosystem = TechEcosystem.DotNet, Version = "10", ShortDescription = "Plataforma .NET de alta performance da Microsoft" },
            new() { Name = "ASP.NET Core MVC", Category = TechCategory.Framework, Ecosystem = TechEcosystem.DotNet, Version = "10", ShortDescription = "Framework web MVC para .NET" },
            new() { Name = "ASP.NET Core Web API", Category = TechCategory.Framework, Ecosystem = TechEcosystem.DotNet, Version = "10", ShortDescription = "Framework para APIs RESTful em .NET" },
            new() { Name = "Blazor", Category = TechCategory.Frontend, Ecosystem = TechEcosystem.DotNet, ShortDescription = "Framework web interativo com C# para .NET" },
            // ORM
            new() { Name = "Entity Framework Core", Category = TechCategory.ORM, Ecosystem = TechEcosystem.DotNet, Version = "10", ShortDescription = "ORM Code First para .NET" },
            new() { Name = "Dapper", Category = TechCategory.ORM, Ecosystem = TechEcosystem.DotNet, ShortDescription = "Micro ORM leve para .NET" },
            // Banco de dados
            new() { Name = "PostgreSQL", Category = TechCategory.Database, Ecosystem = TechEcosystem.Agnostic, ShortDescription = "Banco relacional open-source robusto" },
            new() { Name = "SQL Server", Category = TechCategory.Database, Ecosystem = TechEcosystem.DotNet, ShortDescription = "Banco relacional da Microsoft" },
            new() { Name = "MySQL", Category = TechCategory.Database, Ecosystem = TechEcosystem.Agnostic, ShortDescription = "Banco relacional open-source amplamente usado" },
            new() { Name = "MongoDB", Category = TechCategory.Database, Ecosystem = TechEcosystem.Agnostic, ShortDescription = "Banco NoSQL orientado a documentos" },
            new() { Name = "Redis", Category = TechCategory.Cache, Ecosystem = TechEcosystem.Agnostic, ShortDescription = "Cache in-memory e store de dados chave-valor" },
            // Frontend
            new() { Name = "Tailwind CSS", Category = TechCategory.Frontend, Ecosystem = TechEcosystem.Agnostic, Version = "3", ShortDescription = "Framework CSS utility-first" },
            new() { Name = "React", Category = TechCategory.Frontend, Ecosystem = TechEcosystem.Node, ShortDescription = "Biblioteca JavaScript para UIs componentizadas" },
            new() { Name = "Vue.js", Category = TechCategory.Frontend, Ecosystem = TechEcosystem.Node, Version = "3", ShortDescription = "Framework JavaScript progressivo" },
            new() { Name = "Angular", Category = TechCategory.Frontend, Ecosystem = TechEcosystem.Node, ShortDescription = "Framework SPA completo da Google" },
            new() { Name = "Alpine.js", Category = TechCategory.Frontend, Ecosystem = TechEcosystem.Agnostic, Version = "3", ShortDescription = "Framework JS leve para interatividade inline" },
            // Node
            new() { Name = "Node.js", Category = TechCategory.Framework, Ecosystem = TechEcosystem.Node, ShortDescription = "Runtime JavaScript server-side" },
            new() { Name = "NestJS", Category = TechCategory.Framework, Ecosystem = TechEcosystem.Node, ShortDescription = "Framework Node.js progressivo com TypeScript" },
            // Python
            new() { Name = "Python", Category = TechCategory.Framework, Ecosystem = TechEcosystem.Python, ShortDescription = "Linguagem de programação versátil" },
            new() { Name = "FastAPI", Category = TechCategory.Framework, Ecosystem = TechEcosystem.Python, ShortDescription = "Framework web moderno e rápido para Python" },
            // Mensageria
            new() { Name = "RabbitMQ", Category = TechCategory.Messaging, Ecosystem = TechEcosystem.Agnostic, ShortDescription = "Message broker AMQP" },
            new() { Name = "Azure Service Bus", Category = TechCategory.Messaging, Ecosystem = TechEcosystem.Agnostic, ShortDescription = "Serviço de mensageria gerenciado da Azure" },
            new() { Name = "Kafka", Category = TechCategory.Messaging, Ecosystem = TechEcosystem.Agnostic, ShortDescription = "Plataforma de streaming de eventos distribuída" },
            // DevOps
            new() { Name = "Docker", Category = TechCategory.DevOps, Ecosystem = TechEcosystem.Agnostic, ShortDescription = "Plataforma de containerização" },
            new() { Name = "Kubernetes", Category = TechCategory.DevOps, Ecosystem = TechEcosystem.Agnostic, ShortDescription = "Orquestrador de containers" },
            new() { Name = "GitHub Actions", Category = TechCategory.DevOps, Ecosystem = TechEcosystem.Agnostic, ShortDescription = "CI/CD nativo do GitHub" },
            new() { Name = "Azure DevOps", Category = TechCategory.DevOps, Ecosystem = TechEcosystem.Agnostic, ShortDescription = "Plataforma DevOps completa da Microsoft" },
            new() { Name = "Terraform", Category = TechCategory.DevOps, Ecosystem = TechEcosystem.Agnostic, ShortDescription = "Infraestrutura como código (IaC)" },
            // Testes
            new() { Name = "xUnit", Category = TechCategory.Testing, Ecosystem = TechEcosystem.DotNet, ShortDescription = "Framework de testes unitários para .NET" },
            new() { Name = "NUnit", Category = TechCategory.Testing, Ecosystem = TechEcosystem.DotNet, ShortDescription = "Framework de testes para .NET" },
            new() { Name = "Playwright", Category = TechCategory.Testing, Ecosystem = TechEcosystem.Agnostic, ShortDescription = "Automação de testes E2E para web" },
            new() { Name = "Elasticsearch", Category = TechCategory.Database, Ecosystem = TechEcosystem.Agnostic, ShortDescription = "Motor de busca e analytics distribuído" },
        };

        await db.Technologies.AddRangeAsync(techs);
        await db.SaveChangesAsync();
    }

    // ─── Padrões Arquiteturais ────────────────────────────────────────────────
    private static async Task SeedArchitecturalPatternsAsync(AppDbContext db)
    {
        if (await db.ArchitecturalPatterns.IgnoreQueryFilters().AnyAsync()) return;

        var patterns = new List<ArchitecturalPattern>
        {
            new() { Name = "Clean Architecture", Description = "Separação em camadas (Domain, Application, Infrastructure, Presentation) com dependências apontando para o centro.", Ecosystem = TechEcosystem.Agnostic },
            new() { Name = "DDD (Domain-Driven Design)", Description = "Modelagem do software em torno do domínio de negócio com Aggregates, Entities, Value Objects e Domain Events.", Ecosystem = TechEcosystem.Agnostic },
            new() { Name = "CQRS", Description = "Separação entre comandos (escrita) e consultas (leitura), com handlers dedicados para cada operação.", Ecosystem = TechEcosystem.Agnostic },
            new() { Name = "Repository Pattern", Description = "Abstração da camada de acesso a dados atrás de interfaces, desacoplando a lógica de negócio da persistência.", Ecosystem = TechEcosystem.Agnostic },
            new() { Name = "Unit of Work", Description = "Agrupa múltiplas operações de banco em uma única transação, garantindo consistência.", Ecosystem = TechEcosystem.Agnostic },
            new() { Name = "Microservices", Description = "Arquitetura baseada em serviços independentes, com deploy autônomo, comunicando via APIs ou mensagens.", Ecosystem = TechEcosystem.Agnostic },
            new() { Name = "Event Sourcing", Description = "Persiste o estado da aplicação como uma sequência de eventos imutáveis, permitindo reconstrução do estado.", Ecosystem = TechEcosystem.Agnostic },
            new() { Name = "SAGA Pattern", Description = "Gerencia transações distribuídas através de uma sequência de transações locais com compensações em caso de falha.", Ecosystem = TechEcosystem.Agnostic },
            new() { Name = "BFF (Backend for Frontend)", Description = "Camada backend dedicada a cada tipo de cliente (web, mobile), otimizando as respostas para cada interface.", Ecosystem = TechEcosystem.Agnostic },
            new() { Name = "Hexagonal Architecture", Description = "Isola o núcleo da aplicação de infraestrutura externa via portas (interfaces) e adaptadores (implementações).", Ecosystem = TechEcosystem.Agnostic },
        };

        await db.ArchitecturalPatterns.AddRangeAsync(patterns);
        await db.SaveChangesAsync();
    }

    // ─── Ferramentas de Backlog ───────────────────────────────────────────────
    private static async Task SeedBacklogToolsAsync(AppDbContext db)
    {
        if (await db.BacklogTools.IgnoreQueryFilters().AnyAsync()) return;

        var tools = new List<BacklogTool>
        {
            new()
            {
                Name = "Backlog.md",
                IsSystemDefault = true,
                DefaultInstructions = """
                    ## Gestão de Backlog (Backlog.md)

                    Use o MCP do **Backlog.md** para todo o planejamento e gestão de tarefas:

                    - Crie tarefas para cada módulo ou funcionalidade antes de iniciar a implementação
                    - Registre épicos para agrupamentos lógicos de funcionalidades
                    - Atualize o status das tarefas a cada avanço: To Do → In Progress → Done
                    - Nunca inicie a implementação de um módulo sem que sua tarefa esteja criada
                    - Ao concluir um módulo, marque a tarefa como Done antes de prosseguir
                    - Use o campo `description` para registrar detalhes de implementação e decisões
                    """
            },
            new()
            {
                Name = "GitHub Issues",
                IsSystemDefault = true,
                DefaultInstructions = """
                    ## Gestão de Backlog (GitHub Issues)

                    Use o **GitHub Issues** para rastreamento de tarefas:

                    - Crie uma issue para cada funcionalidade ou módulo
                    - Use labels para categorizar: `feature`, `bug`, `enhancement`, `documentation`
                    - Associe issues a milestones para organizar por versão/sprint
                    - Referencie issues nos commits com `closes #123` para fechamento automático
                    - Use o GitHub Project Board para visualizar o fluxo (To Do / In Progress / Done)
                    """
            },
            new()
            {
                Name = "Jira",
                IsSystemDefault = true,
                DefaultInstructions = """
                    ## Gestão de Backlog (Jira)

                    Use o **Jira** para gerenciamento do projeto:

                    - Crie Epics para módulos principais e Stories para funcionalidades
                    - Use o fluxo: Backlog → To Do → In Progress → Code Review → Done
                    - Estime usando story points (Fibonacci)
                    - Vincule commits e PRs às issues usando a chave do ticket (ex: PROJ-123)
                    - Atualize o status da issue antes de iniciar e ao concluir cada tarefa
                    """
            },
            new()
            {
                Name = "Linear",
                IsSystemDefault = true,
                DefaultInstructions = """
                    ## Gestão de Backlog (Linear)

                    Use o **Linear** para rastreamento ágil de tarefas:

                    - Crie issues no Linear para cada funcionalidade
                    - Organize por ciclos (cycles) para sprints iterativas
                    - Use prioridades: Urgent, High, Medium, Low
                    - Vincule branches Git seguindo o padrão gerado pelo Linear
                    - Atualize o status a cada mudança significativa no desenvolvimento
                    """
            },
            new()
            {
                Name = "Trello",
                IsSystemDefault = false,
                DefaultInstructions = """
                    ## Gestão de Backlog (Trello)

                    Use o **Trello** para organização visual:

                    - Crie cards no board para cada tarefa ou funcionalidade
                    - Use as listas: Backlog, Em Andamento, Em Revisão, Concluído
                    - Adicione checklists dentro dos cards para sub-tarefas
                    - Use etiquetas (labels) para categorizar por tipo ou prioridade
                    - Mova os cards conforme o progresso do desenvolvimento
                    """
            },
            new()
            {
                Name = "Azure DevOps Boards",
                IsSystemDefault = false,
                DefaultInstructions = """
                    ## Gestão de Backlog (Azure DevOps Boards)

                    Use o **Azure DevOps Boards** para gestão de trabalho:

                    - Crie Work Items do tipo Epic, Feature e User Story conforme a hierarquia
                    - Use o fluxo: New → Active → Resolved → Closed
                    - Estime Story Points no campo de esforço
                    - Vincule commits e PRs aos Work Items com `#<ID>` nas mensagens
                    - Configure o board com colunas refletindo o fluxo da equipe
                    """
            },
        };

        await db.BacklogTools.AddRangeAsync(tools);
        await db.SaveChangesAsync();
    }
}

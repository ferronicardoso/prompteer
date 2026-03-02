# Prompteer

Ferramenta pessoal para geração de prompts estruturados destinados a agentes de IA (Claude Code, GitHub Copilot CLI e similares). O sistema guia o usuário por um wizard de 9 etapas, coleta contexto sobre o projeto, stack, arquitetura, módulos e regras comportamentais, e gera um prompt Markdown pronto para uso.

---

## Sumário

- [Visão Geral](#visão-geral)
- [Funcionalidades](#funcionalidades)
- [Arquitetura](#arquitetura)
- [Stack Tecnológica](#stack-tecnológica)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Modelo de Domínio](#modelo-de-domínio)
- [Pré-requisitos](#pré-requisitos)
- [Executando com Docker](#executando-com-docker)
- [Executando localmente](#executando-localmente)
- [Variáveis de Ambiente](#variáveis-de-ambiente)
- [Migrations](#migrations)
- [Integração com IA](#integração-com-ia)
- [Dados Seed](#dados-seed)
- [Testes](#testes)

---

## Visão Geral

O Prompteer resolve o problema de criar prompts genéricos e pouco eficazes para agentes de IA. Em vez de escrever do zero, o usuário preenche um formulário estruturado com perfil do agente, ferramentas de backlog, stack tecnológica, padrões arquiteturais, módulos do projeto e regras de comportamento — e o sistema monta automaticamente um prompt otimizado em Markdown.

Os prompts gerados podem ser salvos como **templates**, versionados, comparados e reutilizados ou clonados para novos projetos.

---

## Funcionalidades

### Gerador de Prompts (Wizard 9 etapas)

| Step | Conteúdo | Opcional |
|------|----------|----------|
| 1 | Perfil do agente (papel, domínio, tom, restrições) | Não |
| 2 | Ferramenta de backlog + instruções de uso | Não |
| 3 | Nome do projeto, descrição e stack tecnológica | Não |
| 4 | Padrões arquiteturais, pacotes obrigatórios e convenções de código | Não |
| 5 | Ambiente: destino de deploy, estratégia Git, CI/CD | **Sim** |
| 6 | Testes: tipos, framework, cobertura mínima | **Sim** |
| 7 | Módulos do projeto com sub-itens e ordenação drag-and-drop | Não |
| 8 | Regras de comportamento (flags + texto livre + chips de sugestão) | Não |
| 9 | Preview do prompt gerado, copiar e salvar como template | — |

### Cadastros Base

- **Perfis de Agente** — 10 perfis padrão do sistema (não editáveis/deletáveis), suporte a criação, clonagem e edição de perfis próprios
- **Tecnologias** — 31 tecnologias padrão categorizadas (Framework, ORM, Banco de Dados, Frontend, Auth, Mensageria, Cache, DevOps, Testes, IA e Outro), com ecossistema (.NET, Node, Python, Java, Agnóstico)
- **Padrões Arquiteturais** — 10 padrões padrão (Clean Architecture, DDD, CQRS, Repository Pattern, Microservices, Event Sourcing, etc.)
- **Ferramentas de Backlog** — Backlog.md, Jira, Linear, GitHub Issues, Trello, com instruções de uso em Markdown

### Templates

- Listagem com busca e filtro
- Histórico de versões (v1, v2, v3…)
- Comparação visual entre versões (diff)
- Clonar, editar, excluir
- Reabertura no wizard para edição

### Dashboard

- Totais: templates salvos, editados recentemente, tecnologias mais usadas
- Atalho rápido para novo prompt

### Integração com IA

- Suporte a **OpenAI** e **Anthropic** (configurável)
- Geração automática de campos: Papel do agente, Domínio de conhecimento, Restrições, Descrição de tecnologias, Padrões arquiteturais, Instruções de backlog, Descrição do projeto
- API Key e modelo armazenados no banco (tabela `AppSettings`)
- Listagem dinâmica de modelos disponíveis via API do provedor

---

## Arquitetura

O projeto segue **Clean Architecture** com 4 camadas:

```
Prompteer.Web (Apresentação)
    └── Prompteer.Application (Casos de Uso / Serviços / DTOs)
            └── Prompteer.Domain (Entidades / Interfaces / Enums)
    └── Prompteer.Infrastructure (EF Core / Repositórios / Serviços externos)
```

**Fluxo de dependências:** as camadas internas nunca referenciam as externas. A Infrastructure implementa as interfaces definidas no Domain/Application.

### Padrões utilizados

- **Repository Pattern** + **Unit of Work** — abstração da persistência via `IRepository<T>` e `IUnitOfWork`
- **CQRS leve** — serviços de leitura e escrita separados por responsabilidade
- **DTO / AutoMapper** — isolamento entre entidades de domínio e contratos da camada de apresentação
- **FluentValidation** — validação declarativa desacoplada dos modelos
- **Soft Delete** — entidades base com `IsDeleted` e filtro global no EF Core
- **Code First Migrations** — schema gerenciado pelo EF Core

---

## Stack Tecnológica

| Componente | Tecnologia |
|------------|------------|
| Framework | ASP.NET Core MVC (.NET 10) |
| ORM | Entity Framework Core 10 |
| Banco de Dados | PostgreSQL 17 |
| Frontend | Tailwind CSS (build via Node/PostCSS) |
| Interatividade | Alpine.js 3 |
| Ícones | Lucide Icons |
| Select avançado | Tom Select 2 |
| Drag-and-drop | SortableJS |
| Markdown render | marked.js |
| Syntax highlight | Highlight.js |
| Validação | FluentValidation 11 |
| Mapeamento | AutoMapper 16 |
| IA — OpenAI | OpenAI .NET SDK 2.2 |
| IA — Anthropic | HTTP Client (API v1) |
| Testes | xUnit + coverlet |
| Containerização | Docker + Docker Compose |

---

## Estrutura do Projeto

```
prompteer/
├── README.md
├── AGENTS.md
├── backlog/
└── src/
    ├── Prompteer.slnx                         # Solution
    ├── docker-compose.yml                     # Orquestração local
    │
    ├── Prompteer.Domain/                      # Camada de domínio
    │   ├── Entities/
    │   │   ├── BaseEntity.cs                  # Id, CreatedAt, UpdatedAt, IsDeleted
    │   │   ├── AgentProfile.cs
    │   │   ├── Technology.cs
    │   │   ├── ArchitecturalPattern.cs
    │   │   ├── BacklogTool.cs
    │   │   ├── PromptTemplate.cs
    │   │   ├── PromptTemplateVersion.cs
    │   │   ├── PromptModule.cs
    │   │   ├── PromptModuleItem.cs
    │   │   ├── PromptVersionTechnology.cs
    │   │   ├── PromptVersionPattern.cs
    │   │   ├── PromptDraft.cs
    │   │   └── AppSetting.cs
    │   ├── Enums/
    │   │   ├── ToneType.cs
    │   │   ├── TechCategory.cs
    │   │   └── TechEcosystem.cs
    │   └── Interfaces/
    │       ├── IRepository.cs
    │       └── IUnitOfWork.cs
    │
    ├── Prompteer.Application/                 # Casos de uso
    │   ├── DTOs/
    │   ├── Mappings/MappingProfile.cs
    │   ├── Services/                          # Interfaces dos serviços
    │   ├── Validators/                        # FluentValidation
    │   └── Wizard/WizardSessionData.cs        # Estado serializado do wizard
    │
    ├── Prompteer.Infrastructure/              # Implementações
    │   ├── Data/
    │   │   ├── AppDbContext.cs
    │   │   ├── Configurations/               # Fluent API por entidade
    │   │   ├── Migrations/
    │   │   └── Repositories/
    │   ├── Seed/DatabaseSeeder.cs
    │   └── Services/                         # AppSettingService, OpenAIService, etc.
    │
    ├── Prompteer.Web/                         # Camada de apresentação
    │   ├── Dockerfile
    │   ├── Controllers/
    │   ├── Views/
    │   │   ├── AgentProfiles/
    │   │   ├── ArchitecturalPatterns/
    │   │   ├── BacklogTools/
    │   │   ├── Dashboard/
    │   │   ├── PromptGenerator/               # Step1.cshtml … Step9.cshtml
    │   │   ├── Settings/
    │   │   ├── Technologies/
    │   │   ├── Templates/
    │   │   └── Shared/
    │   ├── Models/
    │   ├── Extensions/ServiceCollectionExtensions.cs
    │   ├── wwwroot/
    │   ├── tailwind.config.js
    │   └── appsettings.json
    │
    ├── Prompteer.Application.Tests/
    └── Prompteer.Domain.Tests/
```

---

## Modelo de Domínio

```
AgentProfile
  Id, Name, Role, KnowledgeDomain, Tone (enum), DefaultConstraints
  IsSystemDefault, IsDeleted

Technology
  Id, Name, Category (enum), Ecosystem (enum), Version?, ShortDescription?
  IsSystemDefault, IsDeleted

ArchitecturalPattern
  Id, Name, Description, Ecosystem (enum)
  IsSystemDefault, IsDeleted

BacklogTool
  Id, Name, DefaultInstructions
  IsSystemDefault, IsDeleted

PromptTemplate
  Id, Name, Description, CurrentVersionNumber
  ── PromptTemplateVersion (versões)
       Id, VersionNumber, PromptContent (Markdown), CreatedAt
       ── PromptVersionTechnology  (N:N com Technology)
       ── PromptVersionPattern     (N:N com ArchitecturalPattern)
       ── PromptModule
            Id, Name, Order
            ── PromptModuleItem (sub-itens)

PromptDraft
  Id, WizardDataJson, CurrentStep
  (rascunho temporário do wizard; deletado ao salvar template)

AppSetting
  Key (PK), Value, UpdatedAt
  (configurações: AI:Provider, AI:ApiKey, AI:Model)
```

**Enums:**

| Enum | Valores |
|------|---------|
| `ToneType` | Technical, Didactic, Direct, Detailed |
| `TechCategory` | Framework, Database, ORM, Frontend, Auth, Messaging, Cache, Observability, DevOps, Testing, AI, Other |
| `TechEcosystem` | DotNet, Node, Python, Java, Agnostic |

---

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org/) (build do Tailwind CSS)
- [Docker + Docker Compose](https://www.docker.com/) (para ambiente containerizado)
- PostgreSQL 17 (se executar localmente sem Docker)

---

## Executando com Docker

```bash
# A partir do diretório src/
cd src

# (Opcional) definir senha do banco
export POSTGRES_PASSWORD=minhasenha

# Subir todos os serviços
docker compose up -d --build

# Acompanhar logs
docker compose logs -f web
```

A aplicação ficará disponível em `http://localhost:8080`.

O Docker Compose sobe dois serviços:
- **`db`** — PostgreSQL 17 com healthcheck
- **`web`** — ASP.NET Core MVC (build multi-stage: Node → .NET SDK → runtime)

As migrations são aplicadas automaticamente na inicialização (`MigrateAsync()`), assim como o seed inicial de dados.

### Build multi-stage do Dockerfile

```
Stage 1 (node:22-alpine)   → compila Tailwind CSS  → app.css
Stage 2 (dotnet/sdk:10.0)  → restore + publish .NET
Stage 3 (dotnet/aspnet:10.0) → imagem final (non-root user, porta 8080)
```

---

## Executando localmente

```bash
# 1. Subir apenas o banco
cd src
docker compose up -d db

# 2. Instalar dependências CSS
cd Prompteer.Web
npm install
npm run build:css

# 3. Restaurar pacotes .NET
cd ..
dotnet restore Prompteer.slnx

# 4. Executar (as migrations rodam automaticamente)
dotnet run --project Prompteer.Web/Prompteer.Web.csproj
```

A aplicação ficará disponível em `https://localhost:7xxx` / `http://localhost:5xxx` (portas exibidas no terminal).

---

## Variáveis de Ambiente

| Variável | Descrição | Padrão |
|----------|-----------|--------|
| `ConnectionStrings__DefaultConnection` | Connection string PostgreSQL | `Host=localhost;Port=5432;Database=prompteer;Username=postgres;Password=postgres` |
| `ASPNETCORE_ENVIRONMENT` | Ambiente da aplicação | `Development` |
| `POSTGRES_PASSWORD` | Senha do PostgreSQL (Docker) | `postgres` |

As configurações de IA (provider, API key, modelo) são gerenciadas dentro da própria aplicação em **Configurações** e armazenadas na tabela `AppSettings`.

---

## Migrations

```bash
# A partir de src/
# Criar nova migration
dotnet ef migrations add NomeDaMigration \
  --project Prompteer.Infrastructure \
  --startup-project Prompteer.Web

# Aplicar migrations manualmente
dotnet ef database update \
  --project Prompteer.Infrastructure \
  --startup-project Prompteer.Web
```

### Histórico de migrations

| Migration | Descrição |
|-----------|-----------|
| `InitialCreate` | Schema completo inicial |
| `ExpandTextColumns` | Colunas de texto ampliadas para `text` |
| `AddAppSettings` | Tabela `AppSettings` para configurações de IA |
| `AddIsSystemDefaultToTechAndPattern` | Flag `IsSystemDefault` em Technology e ArchitecturalPattern |

---

## Integração com IA

A integração é configurada em **Configurações** na interface da aplicação.

### Provedores suportados

| Provedor | SDK / Protocolo | Endpoint de geração | Listagem de modelos |
|----------|-----------------|---------------------|---------------------|
| OpenAI | `OpenAI` .NET SDK 2.2 | Chat Completions | `GET /v1/models` |
| Anthropic | HTTP Client direto | `POST /v1/messages` | `GET /v1/models` |

### Campos com geração automática

| Campo | Tipo (`fieldType`) |
|-------|--------------------|
| Papel do agente | `AgentRole` |
| Domínio de conhecimento | `AgentKnowledgeDomain` |
| Restrições de comportamento | `AgentConstraints` |
| Descrição de tecnologia | `TechDescription` |
| Descrição de padrão arquitetural | `PatternDescription` |
| Instruções de backlog | `BacklogInstructions` |
| Descrição do projeto (Step 3) | `ProjectDescription` |

### Endpoint AJAX

```
POST /api/ai/generate
Headers: RequestVerificationToken: <antiforgery>
Body: { "fieldType": "AgentRole", "context": { "name": "...", "tone": "..." } }

GET /api/ai/status
→ { "configured": true }
```

---

## Dados Seed

Na primeira inicialização o `DatabaseSeeder` popula automaticamente:

- **10 Perfis de Agente** padrão (Arquiteto Full-Stack .NET, Especialista DevOps, Desenvolvedor Frontend, Backend API, DBA, Segurança, Redator Técnico, QA, Engenheiro de Dados, Mobile)
- **31 Tecnologias** padrão distribuídas nas categorias Framework, ORM, Banco de Dados, Frontend, Auth, Mensageria, Cache, Observabilidade, DevOps, Testes e IA
- **10 Padrões Arquiteturais** padrão (Clean Architecture, DDD, CQRS, Repository Pattern, Microservices, Event Sourcing, SAGA, Vertical Slice, MVC, Hexagonal)
- **5 Ferramentas de Backlog** padrão (Backlog.md, GitHub Issues, Jira, Linear, Trello) com instruções de uso em Markdown

Todos os itens padrão têm `IsSystemDefault = true` e não podem ser editados ou excluídos — apenas clonados.

---

## Testes

```bash
# A partir de src/
dotnet test Prompteer.slnx
```

Projetos de teste:

| Projeto | Framework | Cobertura |
|---------|-----------|-----------|
| `Prompteer.Domain.Tests` | xUnit + coverlet | Entidades de domínio |
| `Prompteer.Application.Tests` | xUnit + coverlet | Serviços de aplicação |

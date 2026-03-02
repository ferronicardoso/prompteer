# Prompteer

A personal tool for generating structured prompts for AI agents (Claude Code, GitHub Copilot CLI, and similar). Prompteer guides you through a 9-step wizard, collects context about your project, tech stack, architecture, modules, and behavioral rules, and generates a ready-to-use Markdown prompt.

---

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Domain Model](#domain-model)
- [Prerequisites](#prerequisites)
- [Running with Docker](#running-with-docker)
- [Running Locally](#running-locally)
- [Environment Variables](#environment-variables)
- [First Run & Setup Wizard](#first-run--setup-wizard)
- [Authentication](#authentication)
- [Migrations](#migrations)
- [AI Integration](#ai-integration)
- [Template Import & Export](#template-import--export)
- [Seed Data](#seed-data)
- [Tests](#tests)

---

## Overview

Prompteer solves the problem of writing generic, low-quality prompts for AI agents. Instead of starting from scratch, you fill out a structured form with the agent's profile, backlog tooling, tech stack, architectural patterns, project modules, and behavioral rules — and the system automatically assembles an optimised Markdown prompt.

Generated prompts can be saved as **templates**, versioned, compared, cloned, and reused across projects. Templates can also be exported as portable JSON files and re-imported into any Prompteer instance.

---

## Features

### Prompt Generator Wizard (9 steps)

| Step | Content | Optional |
|------|---------|----------|
| 1 | Agent profile (role, domain, tone, constraints) | No |
| 2 | Backlog tool + usage instructions | No |
| 3 | Project name, description, and tech stack | No |
| 4 | Architectural patterns, required packages, code conventions | No |
| 5 | Environment: deploy target, Git strategy, CI/CD | **Yes** |
| 6 | Testing: types, framework, minimum coverage | **Yes** |
| 7 | Project modules with sub-items and drag-and-drop ordering | No |
| 8 | Behavioral rules (flags + free text + suggestion chips) | No |
| 9 | Prompt preview, copy to clipboard, save as template | — |

### Base Registries

- **Agent Profiles** — 10 built-in system profiles (read-only), plus support for creating, cloning and editing custom profiles
- **Technologies** — 31 built-in technologies grouped by category (Framework, ORM, Database, Frontend, Auth, Messaging, Cache, DevOps, Testing, AI, Other) and ecosystem (.NET, Node, Python, Java, Agnostic)
- **Architectural Patterns** — 10 built-in patterns (Clean Architecture, DDD, CQRS, Repository Pattern, Microservices, Event Sourcing, etc.)
- **Backlog Tools** — Backlog.md, Jira, Linear, GitHub Issues, Trello — each with default Markdown usage instructions

### Templates

- Searchable listing with pagination
- Version history (v1, v2, v3 …)
- Visual diff between versions
- Clone, edit, delete
- Re-open in wizard for editing
- **Export** a single template or all templates as a portable JSON file
- **Import** templates from a JSON file; technologies and patterns are resolved by name and auto-created if missing

### Dashboard

- Totals: templates saved, recently edited, most-used technologies
- Quick shortcut to start a new prompt

### AI Integration

- **OpenAI** and **Anthropic** supported (user-configurable)
- Dynamic model listing from the provider's API
- Auto-generation for: agent role, knowledge domain, constraints, tech descriptions, architectural pattern descriptions, backlog instructions, project description
- API key and selected model stored in the database (`AppSettings` table)

### Authentication & Authorization

- **First-run setup wizard** — creates a local admin account (email + password) before any login is required
- **Microsoft Entra ID (Azure AD)** — optional corporate SSO configured by the admin inside the app
- **Roles from Entra App Roles** — define `Admin`, `Editor`, `Viewer` app roles in your Azure App Registration and assign users; roles are read from the `roles` JWT claim
- **Avatar from Microsoft Graph** — profile photos fetched from Graph API and cached in memory
- **Local admin fallback** — the bootstrap admin always has local password access, even after Entra is configured
- **Auto-linking** — when the local admin first logs in via Entra, the account is linked automatically by email

### User Management (Admin only)

- List all users with their role badge and last login
- Activate / deactivate users
- Roles are read-only (managed in Entra portal)

---

## Architecture

The project follows **Clean Architecture** with four layers:

```
Prompteer.Web          (Presentation)
    └── Prompteer.Application  (Use Cases / Services / DTOs)
            └── Prompteer.Domain       (Entities / Interfaces / Enums)
    └── Prompteer.Infrastructure  (EF Core / Repositories / External services)
```

**Dependency rule:** inner layers never reference outer layers. Infrastructure implements interfaces defined in Domain/Application.

### Patterns used

| Pattern | Details |
|---------|---------|
| Repository + Unit of Work | `IRepository<T>` and `IUnitOfWork` abstractions |
| Lightweight CQRS | Read and write responsibilities separated by service |
| DTO / AutoMapper | Domain entities isolated from presentation contracts |
| FluentValidation | Declarative, decoupled validation |
| Soft Delete | `IsDeleted` flag + global query filter in EF Core |
| Code First Migrations | Schema fully managed by EF Core |
| PBKDF2 password hashing | Built-in BCL only (`Rfc2898DeriveBytes.Pbkdf2`, 150k iterations) |

---

## Tech Stack

| Component | Technology |
|-----------|-----------|
| Framework | ASP.NET Core MVC (.NET 10) |
| ORM | Entity Framework Core 10 |
| Database | PostgreSQL 17 |
| CSS | Tailwind CSS (built via Node/PostCSS) |
| Interactivity | Alpine.js 3 |
| Icons | Lucide Icons |
| Advanced select | Tom Select 2 |
| Drag-and-drop | SortableJS |
| Markdown render | marked.js |
| Syntax highlight | Highlight.js |
| Validation | FluentValidation 11 |
| Mapping | AutoMapper 16 |
| Auth | Microsoft.Identity.Web 3.8 |
| AI — OpenAI | OpenAI .NET SDK 2.2 |
| AI — Anthropic | HTTP Client (API v1) |
| Testing | xUnit + coverlet |
| Containerisation | Docker + Docker Compose |

---

## Project Structure

```
prompteer/
├── README.md
├── AGENTS.md
├── CLAUDE.md
├── backlog/
└── src/
    ├── Prompteer.slnx                          # Solution file
    ├── docker-compose.yml                      # Local orchestration
    │
    ├── Prompteer.Domain/                       # Domain layer
    │   ├── Common/BaseEntity.cs                # Id, CreatedAt, UpdatedAt, IsDeleted
    │   ├── Entities/
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
    │   │   ├── AppSetting.cs
    │   │   └── ApplicationUser.cs
    │   ├── Enums/
    │   │   ├── ToneType.cs
    │   │   ├── TechCategory.cs
    │   │   ├── TechEcosystem.cs
    │   │   └── UserRole.cs
    │   └── Interfaces/
    │       ├── IRepository.cs
    │       ├── IUnitOfWork.cs
    │       └── ICurrentUserService.cs
    │
    ├── Prompteer.Application/                  # Use-case layer
    │   ├── DTOs/
    │   │   ├── PromptTemplateDto.cs
    │   │   ├── TemplateExportDto.cs            # Import/export schema
    │   │   └── AISettingsDto.cs
    │   ├── Mappings/MappingProfile.cs
    │   ├── Services/                           # Service interfaces
    │   ├── Validators/                         # FluentValidation
    │   └── Wizard/WizardSessionData.cs         # Serialised wizard state
    │
    ├── Prompteer.Infrastructure/               # Implementations
    │   ├── Data/
    │   │   ├── AppDbContext.cs
    │   │   ├── Configurations/                 # EF Fluent API per entity
    │   │   ├── Migrations/
    │   │   └── Repositories/
    │   ├── Seed/DatabaseSeeder.cs
    │   ├── Helpers/
    │   │   └── PasswordHasher.cs
    │   └── Services/
    │       ├── PromptTemplateService.cs        # Includes import/export
    │       ├── AppSettingService.cs
    │       ├── OpenAIService.cs
    │       ├── AnthropicService.cs
    │       └── CurrentUserService.cs
    │
    ├── Prompteer.Web/                          # Presentation layer
    │   ├── Dockerfile
    │   ├── Controllers/
    │   │   ├── AccountController.cs            # Login, Entra SSO, Sign-out, Photo
    │   │   ├── SetupController.cs              # First-run setup
    │   │   ├── UsersController.cs              # Admin user management
    │   │   ├── TemplatesController.cs          # Includes Import/Export
    │   │   ├── PromptGeneratorController.cs
    │   │   ├── DashboardController.cs
    │   │   ├── SettingsController.cs
    │   │   └── ...
    │   ├── Middleware/
    │   │   └── SetupRedirectMiddleware.cs      # Redirects to /Setup on first run
    │   ├── Helpers/
    │   │   ├── PasswordHasher.cs               # PBKDF2 hash/verify
    │   │   └── AppSettingsWriter.cs            # Runtime appsettings.json writer
    │   ├── Views/
    │   │   ├── Account/                        # Login, AccessDenied
    │   │   ├── Setup/                          # First-run setup wizard
    │   │   ├── Users/                          # User management
    │   │   ├── AgentProfiles/
    │   │   ├── ArchitecturalPatterns/
    │   │   ├── BacklogTools/
    │   │   ├── Dashboard/
    │   │   ├── PromptGenerator/                # Step1.cshtml … Step9.cshtml
    │   │   ├── Settings/
    │   │   ├── Technologies/
    │   │   ├── Templates/                      # Includes Import.cshtml
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

## Domain Model

```
ApplicationUser
  Id, EntraObjectId, DisplayName, Email
  Role (enum: Admin | Editor | Viewer)
  IsActive, LastLoginAt, PasswordHash?

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
  └── PromptTemplateVersion (immutable versions)
       Id, VersionNumber, GeneratedPrompt (Markdown), WizardDataJson, CreatedAt
       ├── PromptVersionTechnology  (N:N → Technology)
       ├── PromptVersionPattern     (N:N → ArchitecturalPattern)
       └── PromptModule
            Id, Name, DisplayOrder
            └── PromptModuleItem (sub-items)

PromptDraft
  Id, WizardDataJson, CurrentStep
  (temporary wizard state; deleted when template is saved)

AppSetting
  Key (PK), Value, UpdatedAt
  (stores AI:Provider, AI:ApiKey, AI:Model)
```

**Enums:**

| Enum | Values |
|------|--------|
| `ToneType` | Technical, Didactic, Direct, Detailed |
| `TechCategory` | Framework, Database, ORM, Frontend, Auth, Messaging, Cache, Observability, DevOps, Testing, AI, Other |
| `TechEcosystem` | DotNet, Node, Python, Java, Agnostic |
| `UserRole` | Admin = 2, Editor = 1, Viewer = 0 |

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org/) (Tailwind CSS build)
- [Docker + Docker Compose](https://www.docker.com/) (for containerised environment)
- PostgreSQL 17 (if running locally without Docker)

---

## Running with Docker

```bash
# From the src/ directory
cd src

# (Optional) set database password
export POSTGRES_PASSWORD=mypassword

# Start all services
docker compose up -d --build

# Follow logs
docker compose logs -f web
```

The application will be available at `http://localhost:8080`.

Docker Compose starts two services:
- **`db`** — PostgreSQL 17 with healthcheck
- **`web`** — ASP.NET Core MVC (multi-stage build: Node → .NET SDK → runtime)

Migrations are applied automatically on startup (`MigrateAsync()`), followed by seed data.

### Dockerfile multi-stage build

```
Stage 1 (node:22-alpine)      → compiles Tailwind CSS  → app.css
Stage 2 (dotnet/sdk:10.0)     → restore + publish .NET
Stage 3 (dotnet/aspnet:10.0)  → final runtime image (non-root user, port 8080)
```

---

## Running Locally

```bash
# 1. Start only the database
cd src
docker compose up -d db

# 2. Install CSS dependencies
cd Prompteer.Web
npm install
npm run build:css

# 3. Restore .NET packages
cd ..
dotnet restore Prompteer.slnx

# 4. Run (migrations apply automatically)
dotnet run --project Prompteer.Web/Prompteer.Web.csproj
```

The application will be available at `https://localhost:7xxx` / `http://localhost:5xxx` (ports shown in the terminal).

---

## Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | `Host=localhost;Port=5432;Database=prompteer;Username=postgres;Password=postgres` |
| `ASPNETCORE_ENVIRONMENT` | Application environment | `Development` |
| `POSTGRES_PASSWORD` | PostgreSQL password (Docker) | `postgres` |

AI settings (provider, API key, model) are managed inside the application under **Settings** and stored in the `AppSettings` table.

Microsoft Entra settings (TenantId, ClientId, ClientSecret) are configured by the admin inside the app under **Settings → Microsoft Entra ID** and written to `appsettings.json` at runtime.

---

## First Run & Setup Wizard

On the very first run — before any user exists — the application automatically redirects to `/Setup`.

1. Fill in your **display name**, **email**, and **password** to create the bootstrap admin account.
2. After login, go to **Settings → Microsoft Entra ID** to configure corporate SSO (optional).
3. Once Entra is configured, all users authenticate with their Microsoft accounts. The local password remains available as a fallback for the admin.

> **Entra account linking** — when the bootstrap admin logs in via Entra for the first time, the system matches by email and links the Entra Object ID automatically.

---

## Authentication

### Local admin (first run)

- Password hashed with PBKDF2-SHA256, 150 000 iterations, 16-byte salt (no external packages)
- Authentication cookie issued with `roles` claim compatible with all authorization policies

### Microsoft Entra ID

1. **Register an application** in Azure Portal → Microsoft Entra ID → App registrations
2. Set redirect URI to `https://yourdomain/signin-oidc`
3. Add API permissions: `User.Read`, `User.ReadBasic.All`
4. Create **App Roles**: `Admin`, `Editor`, `Viewer`
5. Assign users to roles in Enterprise Applications → Users and groups
6. Enter `TenantId`, `ClientId`, and `ClientSecret` in **Settings → Microsoft Entra ID**

Roles are read from the `roles` JWT claim. `User.IsInRole("Admin")` and `[Authorize(Policy = "AdminOnly")]` work identically for both local and Entra-authenticated users.

### Authorization policies

| Policy | Requirement |
|--------|-------------|
| `AdminOnly` | `UserRole.Admin` |
| `EditorOrAbove` | `UserRole.Editor` or `UserRole.Admin` |

---

## Migrations

```bash
# From src/
# Add a new migration
dotnet ef migrations add MigrationName \
  --project Prompteer.Infrastructure \
  --startup-project Prompteer.Web

# Apply manually
dotnet ef database update \
  --project Prompteer.Infrastructure \
  --startup-project Prompteer.Web
```

### Migration history

| Migration | Description |
|-----------|-------------|
| `InitialCreate` | Full initial schema |
| `ExpandTextColumns` | Text columns widened to `text` |
| `AddAppSettings` | `AppSettings` table for AI configuration |
| `AddIsSystemDefaultToTechAndPattern` | `IsSystemDefault` flag on Technology and ArchitecturalPattern |
| `AddApplicationUsers` | `ApplicationUsers` table + `CreatedByUserId` / `UpdatedByUserId` on all entities |
| `AddPasswordHashToUser` | `PasswordHash` column on `ApplicationUsers` |

---

## AI Integration

Configure the provider and API key in **Settings** inside the application.

### Supported providers

| Provider | SDK / Protocol | Generation endpoint | Model listing |
|----------|---------------|---------------------|---------------|
| OpenAI | `OpenAI` .NET SDK 2.2 | Chat Completions | `GET /v1/models` |
| Anthropic | HTTP Client (direct) | `POST /v1/messages` | `GET /v1/models` |

### Fields with AI auto-generation

| Field | `fieldType` value |
|-------|------------------|
| Agent role | `AgentRole` |
| Knowledge domain | `AgentKnowledgeDomain` |
| Behavioural constraints | `AgentConstraints` |
| Technology description | `TechDescription` |
| Architectural pattern description | `PatternDescription` |
| Backlog instructions | `BacklogInstructions` |
| Project description (Step 3) | `ProjectDescription` |

### AJAX endpoints

```
POST /api/ai/generate
Headers: RequestVerificationToken: <antiforgery>
Body: { "fieldType": "AgentRole", "context": { "name": "...", "tone": "..." } }

GET /api/ai/status
→ { "configured": true }
```

---

## Template Import & Export

Templates can be shared between Prompteer instances as portable JSON files.

### Export

| Route | Description |
|-------|-------------|
| `GET /Templates/Export/{id}` | Downloads a single template as JSON |
| `GET /Templates/ExportAll` | Downloads all templates as a single JSON bundle |

### Import

| Route | Description |
|-------|-------------|
| `GET /Templates/Import` | Import form |
| `POST /Templates/Import` | Uploads a `.json` file and creates templates |

**Import behaviour:**
- Technologies and architectural patterns are resolved by name; if a name is not found in the database it is created automatically.
- Duplicate detection by template name (case-insensitive). Toggle **Skip duplicates** to control behaviour.
- Errors per template are collected and displayed after the import — other templates in the file are still processed.

### Export format (`SchemaVersion: "1.0"`)

```json
{
  "schemaVersion": "1.0",
  "exportedAt": "2026-03-02T17:00:00Z",
  "exportedBy": "Admin Name",
  "templates": [
    {
      "name": "My Template",
      "description": "...",
      "versionNumber": 3,
      "createdAt": "...",
      "generatedPrompt": "# You are a...",
      "wizardDataJson": "{...}",
      "technologyNames": ["ASP.NET Core", "PostgreSQL"],
      "patternNames": ["Clean Architecture", "CQRS"]
    }
  ]
}
```

---

## Seed Data

On first startup `DatabaseSeeder` automatically populates:

- **10 Agent Profiles** — Full-Stack .NET Architect, DevOps Specialist, Frontend Developer, Backend API Developer, DBA, Security Specialist, Technical Writer, QA Engineer, Data Engineer, Mobile Developer
- **31 Technologies** — distributed across Framework, ORM, Database, Frontend, Auth, Messaging, Cache, Observability, DevOps, Testing and AI categories
- **10 Architectural Patterns** — Clean Architecture, DDD, CQRS, Repository Pattern, Microservices, Event Sourcing, SAGA, Vertical Slice, MVC, Hexagonal Architecture
- **5 Backlog Tools** — Backlog.md, GitHub Issues, Jira, Linear, Trello — each with Markdown usage instructions

All built-in items have `IsSystemDefault = true` and cannot be edited or deleted — only cloned.

---

## Tests

```bash
# From src/
dotnet test Prompteer.slnx
```

| Project | Framework | Coverage scope |
|---------|-----------|----------------|
| `Prompteer.Domain.Tests` | xUnit + coverlet | Domain entities |
| `Prompteer.Application.Tests` | xUnit + coverlet | Application services |


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

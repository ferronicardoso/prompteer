# Prompteer

[![MIT License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)](https://dotnet.microsoft.com/download)
[![PostgreSQL 17](https://img.shields.io/badge/PostgreSQL-17-336791?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-ready-2496ED?logo=docker&logoColor=white)](https://hub.docker.com/r/ferronicardoso/prompteer)
[![Docker Hub](https://img.shields.io/docker/v/ferronicardoso/prompteer?label=Docker%20Hub&logo=docker)](https://hub.docker.com/r/ferronicardoso/prompteer)
[![Docker Pulls](https://img.shields.io/docker/pulls/ferronicardoso/prompteer)](https://hub.docker.com/r/ferronicardoso/prompteer)
[![GitHub Actions](https://github.com/ferronicardoso/prompteer/actions/workflows/docker-publish.yml/badge.svg)](https://github.com/ferronicardoso/prompteer/actions/workflows/docker-publish.yml)

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
- [Quick Start — Docker Hub](#quick-start--docker-hub)
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

## Quick Start — Docker Hub

The easiest way to run Prompteer is by pulling the pre-built image directly from Docker Hub — no local build required.

```bash
docker pull ferronicardoso/prompteer:latest
```

### 1. Create a `docker-compose.yml`

```yaml
services:
  db:
    image: postgres:17-alpine
    restart: unless-stopped
    environment:
      POSTGRES_DB: prompteer
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres -d prompteer"]
      interval: 10s
      timeout: 5s
      retries: 5

  web:
    image: ferronicardoso/prompteer:latest
    restart: unless-stopped
    depends_on:
      db:
        condition: service_healthy
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      POSTGRES_HOST: db
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
      POSTGRES_DB: prompteer
    ports:
      - "8080:8080"

volumes:
  postgres_data:
```

### 2. Start the application

```bash
# Set a strong password (required)
export POSTGRES_PASSWORD=your_strong_password_here

docker compose up -d
```

The application will be available at `http://localhost:8080`.

On the first run, Prompteer redirects automatically to `/Setup` where you create the admin account.

### Available tags

| Tag | Description |
|-----|-------------|
| `latest` | Most recent stable release |
| `1.0.0` | Specific version (semver) |

---

## Running with Docker

```bash
# From the src/ directory
cd src

# Set a strong database password (required)
export POSTGRES_PASSWORD=your_strong_password_here

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
| `POSTGRES_HOST` | PostgreSQL host | *(must be set)* |
| `POSTGRES_USER` | PostgreSQL username | `postgres` |
| `POSTGRES_PASSWORD` | PostgreSQL password | *(must be set)* |
| `POSTGRES_DB` | PostgreSQL database name | `prompteer` |
| `ASPNETCORE_ENVIRONMENT` | Application environment | `Development` |
| `ConnectionStrings__DefaultConnection` | Full connection string (fallback if `POSTGRES_HOST` is not set) | — |

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



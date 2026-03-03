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
        await SeedTemplatesAsync(db);
    }

    // Stub — full seed implemented separately
    private static Task SeedTemplatesAsync(AppDbContext db) => Task.CompletedTask;
    private static async Task SeedAgentProfilesAsync(AppDbContext db)
    {
        if (await db.AgentProfiles.IgnoreQueryFilters().AnyAsync()) return;

        var profiles = new List<AgentProfile>
        {
            new()
            {
                Name = "Full-Stack .NET Architect",
                Role = "You are a senior software architect and full-stack developer specializing in the .NET ecosystem",
                KnowledgeDomain = "ASP.NET Core, Entity Framework Core, Clean Architecture, DDD, microservices, Azure",
                Tone = ToneType.Technical,
                DefaultConstraints = "Prefer pragmatic solutions and avoid over-engineering. Follow .NET ecosystem conventions. Write clean, testable and well-documented code when necessary.",
                IsSystemDefault = true
            },
            new()
            {
                Name = "DevOps & Infrastructure Specialist",
                Role = "You are a senior DevOps engineer specializing in infrastructure automation and CI/CD pipelines",
                KnowledgeDomain = "Docker, Kubernetes, Terraform, GitHub Actions, Azure DevOps, GitLab CI, observability",
                Tone = ToneType.Direct,
                DefaultConstraints = "Prioritize automation, security and reproducibility. Follow IaC best practices. All scripts must be idempotent.",
                IsSystemDefault = true
            },
            new()
            {
                Name = "Frontend Developer (React/Vue)",
                Role = "You are a senior frontend developer specializing in modern and responsive interfaces",
                KnowledgeDomain = "React, Vue.js, TypeScript, Tailwind CSS, Vite, accessibility, web performance",
                Tone = ToneType.Didactic,
                DefaultConstraints = "Prioritize accessibility (WCAG 2.1), performance and user experience. Always use TypeScript. Components must be reusable and testable.",
                IsSystemDefault = true
            },
            new()
            {
                Name = "Backend REST API Developer",
                Role = "You are a senior backend developer specializing in RESTful APIs and microservices",
                KnowledgeDomain = "REST, gRPC, OpenAPI, JWT/OAuth2 authentication, rate limiting, API versioning",
                Tone = ToneType.Technical,
                DefaultConstraints = "Follow REST principles. Document all endpoints with OpenAPI. Implement standardized error handling (RFC 7807). Prioritize security.",
                IsSystemDefault = true
            },
            new()
            {
                Name = "Database Administrator",
                Role = "You are a senior DBA specializing in data modeling, performance and administration of relational and NoSQL databases",
                KnowledgeDomain = "PostgreSQL, SQL Server, MongoDB, Redis, relational modeling, indexes, partitioning, replication",
                Tone = ToneType.Detailed,
                DefaultConstraints = "Always consider performance and indexes. Document the schema. Avoid N+1 queries. Use transactions appropriately. Prefer incremental and reversible migrations.",
                IsSystemDefault = true
            },
            new()
            {
                Name = "Security Specialist",
                Role = "You are an application security specialist (AppSec) focused on secure development",
                KnowledgeDomain = "OWASP Top 10, SAST/DAST, authentication, authorization, encryption, secrets management",
                Tone = ToneType.Direct,
                DefaultConstraints = "Never store secrets in code. Follow the principle of least privilege. Validate all inputs. Handle sensitive data carefully and document security decisions.",
                IsSystemDefault = true
            },
            new()
            {
                Name = "Technical Writer / Documentation",
                Role = "You are a senior technical writer specializing in software and API documentation",
                KnowledgeDomain = "Markdown, OpenAPI/Swagger, diagramas C4/PlantUML, wikis, runbooks, ADRs",
                Tone = ToneType.Didactic,
                DefaultConstraints = "Write clearly and objectively. Use concrete examples. Document the why, not just the how. Keep documentation in sync with the code.",
                IsSystemDefault = true
            },
            new()
            {
                Name = "QA & Testing Engineer",
                Role = "You are a senior quality engineer specializing in test strategies and automation",
                KnowledgeDomain = "unit tests, integration tests, E2E, TDD, BDD, xUnit, Playwright, Selenium, code coverage",
                Tone = ToneType.Detailed,
                DefaultConstraints = "Follow the testing pyramid. Write readable tests with arrange/act/assert. Tests must be independent and deterministic. Document edge cases.",
                IsSystemDefault = true
            },
            new()
            {
                Name = "Python Data Engineer",
                Role = "You are a senior data engineer specializing in data pipelines and analysis with Python",
                KnowledgeDomain = "Python, pandas, PySpark, Airflow, dbt, data lakes, PostgreSQL, SQLAlchemy",
                Tone = ToneType.Technical,
                DefaultConstraints = "Prioritize idempotent and traceable pipelines. Use typing with mypy. Document data transformations. Consider volume and latency in every decision.",
                IsSystemDefault = true
            },
            new()
            {
                Name = "Mobile Developer (Flutter/React Native)",
                Role = "You are a senior mobile developer specializing in cross-platform applications",
                KnowledgeDomain = "Flutter, Dart, React Native, TypeScript, state management, REST API integration, App Store / Google Play",
                Tone = ToneType.Didactic,
                DefaultConstraints = "Prioritize performance and native experience. Implement offline state handling. Follow Material Design and Human Interface Guidelines UI guidelines.",
                IsSystemDefault = true
            }
        };

        await db.AgentProfiles.AddRangeAsync(profiles);
        await db.SaveChangesAsync();
    }

    // ─── Technologies ─────────────────────────────────────────────────────────
    private static async Task SeedTechnologiesAsync(AppDbContext db)
    {
        if (await db.Technologies.IgnoreQueryFilters().AnyAsync()) return;

        var techs = new List<Technology>
        {
            // .NET Framework
            new() { Name = ".NET 10", Category = TechCategory.Framework, Ecosystem = TechEcosystem.DotNet, Version = "10", ShortDescription = "High-performance .NET platform by Microsoft", IsSystemDefault = true },
            new() { Name = "ASP.NET Core MVC", Category = TechCategory.Framework, Ecosystem = TechEcosystem.DotNet, Version = "10", ShortDescription = "MVC web framework for .NET", IsSystemDefault = true },
            new() { Name = "ASP.NET Core Web API", Category = TechCategory.Framework, Ecosystem = TechEcosystem.DotNet, Version = "10", ShortDescription = "Framework for RESTful APIs in .NET", IsSystemDefault = true },
            new() { Name = "Blazor", Category = TechCategory.Frontend, Ecosystem = TechEcosystem.DotNet, ShortDescription = "Interactive web framework with C# for .NET", IsSystemDefault = true },
            // ORM
            new() { Name = "Entity Framework Core", Category = TechCategory.ORM, Ecosystem = TechEcosystem.DotNet, Version = "10", ShortDescription = "Code First ORM for .NET", IsSystemDefault = true },
            new() { Name = "Dapper", Category = TechCategory.ORM, Ecosystem = TechEcosystem.DotNet, ShortDescription = "Lightweight Micro ORM for .NET", IsSystemDefault = true },
            // Banco de dados
            new() { Name = "PostgreSQL", Category = TechCategory.Database, Ecosystem = TechEcosystem.Agnostic, ShortDescription = "Robust open-source relational database", IsSystemDefault = true },
            new() { Name = "SQL Server", Category = TechCategory.Database, Ecosystem = TechEcosystem.DotNet, ShortDescription = "Microsoft relational database", IsSystemDefault = true },
            new() { Name = "MySQL", Category = TechCategory.Database, Ecosystem = TechEcosystem.Agnostic, ShortDescription = "Widely used open-source relational database", IsSystemDefault = true },
            new() { Name = "MongoDB", Category = TechCategory.Database, Ecosystem = TechEcosystem.Agnostic, ShortDescription = "Document-oriented NoSQL database", IsSystemDefault = true },
            new() { Name = "Redis", Category = TechCategory.Cache, Ecosystem = TechEcosystem.Agnostic, ShortDescription = "In-memory cache and key-value data store", IsSystemDefault = true },
            // Frontend
            new() { Name = "Tailwind CSS", Category = TechCategory.Frontend, Ecosystem = TechEcosystem.Agnostic, Version = "3", ShortDescription = "Utility-first CSS framework", IsSystemDefault = true },
            new() { Name = "React", Category = TechCategory.Frontend, Ecosystem = TechEcosystem.Node, ShortDescription = "JavaScript library for component-based UIs", IsSystemDefault = true },
            new() { Name = "Vue.js", Category = TechCategory.Frontend, Ecosystem = TechEcosystem.Node, Version = "3", ShortDescription = "Progressive JavaScript framework", IsSystemDefault = true },
            new() { Name = "Angular", Category = TechCategory.Frontend, Ecosystem = TechEcosystem.Node, ShortDescription = "Full-featured SPA framework by Google", IsSystemDefault = true },
            new() { Name = "Alpine.js", Category = TechCategory.Frontend, Ecosystem = TechEcosystem.Agnostic, Version = "3", ShortDescription = "Lightweight JS framework for inline interactivity", IsSystemDefault = true },
            // Node
            new() { Name = "Node.js", Category = TechCategory.Framework, Ecosystem = TechEcosystem.Node, ShortDescription = "Server-side JavaScript runtime", IsSystemDefault = true },
            new() { Name = "NestJS", Category = TechCategory.Framework, Ecosystem = TechEcosystem.Node, ShortDescription = "Progressive Node.js framework with TypeScript", IsSystemDefault = true },
            // Python
            new() { Name = "Python", Category = TechCategory.Framework, Ecosystem = TechEcosystem.Python, ShortDescription = "Versatile programming language", IsSystemDefault = true },
            new() { Name = "FastAPI", Category = TechCategory.Framework, Ecosystem = TechEcosystem.Python, ShortDescription = "Modern and fast web framework for Python", IsSystemDefault = true },
            // Mensageria
            new() { Name = "RabbitMQ", Category = TechCategory.Messaging, Ecosystem = TechEcosystem.Agnostic, ShortDescription = "AMQP message broker", IsSystemDefault = true },
            new() { Name = "Azure Service Bus", Category = TechCategory.Messaging, Ecosystem = TechEcosystem.Agnostic, ShortDescription = "Managed messaging service from Azure", IsSystemDefault = true },
            new() { Name = "Kafka", Category = TechCategory.Messaging, Ecosystem = TechEcosystem.Agnostic, ShortDescription = "Distributed event streaming platform", IsSystemDefault = true },
            // DevOps
            new() { Name = "Docker", Category = TechCategory.DevOps, Ecosystem = TechEcosystem.Agnostic, ShortDescription = "Containerization platform", IsSystemDefault = true },
            new() { Name = "Kubernetes", Category = TechCategory.DevOps, Ecosystem = TechEcosystem.Agnostic, ShortDescription = "Container orchestrator", IsSystemDefault = true },
            new() { Name = "GitHub Actions", Category = TechCategory.DevOps, Ecosystem = TechEcosystem.Agnostic, ShortDescription = "Native CI/CD for GitHub", IsSystemDefault = true },
            new() { Name = "Azure DevOps", Category = TechCategory.DevOps, Ecosystem = TechEcosystem.Agnostic, ShortDescription = "Complete DevOps platform by Microsoft", IsSystemDefault = true },
            new() { Name = "Terraform", Category = TechCategory.DevOps, Ecosystem = TechEcosystem.Agnostic, ShortDescription = "Infrastructure as Code (IaC)", IsSystemDefault = true },
            // Testes
            new() { Name = "xUnit", Category = TechCategory.Testing, Ecosystem = TechEcosystem.DotNet, ShortDescription = "Unit testing framework for .NET", IsSystemDefault = true },
            new() { Name = "NUnit", Category = TechCategory.Testing, Ecosystem = TechEcosystem.DotNet, ShortDescription = "Testing framework for .NET", IsSystemDefault = true },
            new() { Name = "Playwright", Category = TechCategory.Testing, Ecosystem = TechEcosystem.Agnostic, ShortDescription = "E2E test automation for web", IsSystemDefault = true },
            new() { Name = "Elasticsearch", Category = TechCategory.Database, Ecosystem = TechEcosystem.Agnostic, ShortDescription = "Distributed search and analytics engine", IsSystemDefault = true },
        };

        await db.Technologies.AddRangeAsync(techs);
        await db.SaveChangesAsync();
    }

    // ─── Architectural Patterns ─────────────────────────────────────────────
    private static async Task SeedArchitecturalPatternsAsync(AppDbContext db)
    {
        if (await db.ArchitecturalPatterns.IgnoreQueryFilters().AnyAsync()) return;

        var patterns = new List<ArchitecturalPattern>
        {
            new() { Name = "Clean Architecture", Description = "Layer separation (Domain, Application, Infrastructure, Presentation) with dependencies pointing inward.", Ecosystem = TechEcosystem.Agnostic, IsSystemDefault = true },
            new() { Name = "DDD (Domain-Driven Design)", Description = "Software modeling around the business domain with Aggregates, Entities, Value Objects and Domain Events.", Ecosystem = TechEcosystem.Agnostic, IsSystemDefault = true },
            new() { Name = "CQRS", Description = "Separation between commands (write) and queries (read), with dedicated handlers for each operation.", Ecosystem = TechEcosystem.Agnostic, IsSystemDefault = true },
            new() { Name = "Repository Pattern", Description = "Abstraction of the data access layer behind interfaces, decoupling business logic from persistence.", Ecosystem = TechEcosystem.Agnostic, IsSystemDefault = true },
            new() { Name = "Unit of Work", Description = "Groups multiple database operations into a single transaction, ensuring consistency.", Ecosystem = TechEcosystem.Agnostic, IsSystemDefault = true },
            new() { Name = "Microservices", Description = "Architecture based on independent services, with autonomous deployment, communicating via APIs or messages.", Ecosystem = TechEcosystem.Agnostic, IsSystemDefault = true },
            new() { Name = "Event Sourcing", Description = "Persists application state as a sequence of immutable events, allowing state reconstruction.", Ecosystem = TechEcosystem.Agnostic, IsSystemDefault = true },
            new() { Name = "SAGA Pattern", Description = "Manages distributed transactions through a sequence of local transactions with compensations on failure.", Ecosystem = TechEcosystem.Agnostic, IsSystemDefault = true },
            new() { Name = "BFF (Backend for Frontend)", Description = "Dedicated backend layer for each client type (web, mobile), optimizing responses for each interface.", Ecosystem = TechEcosystem.Agnostic, IsSystemDefault = true },
            new() { Name = "Hexagonal Architecture", Description = "Isolates the application core from external infrastructure via ports (interfaces) and adapters (implementations).", Ecosystem = TechEcosystem.Agnostic, IsSystemDefault = true },
        };

        await db.ArchitecturalPatterns.AddRangeAsync(patterns);
        await db.SaveChangesAsync();
    }

    // ─── Backlog Tools ───────────────────────────────────────────────────────
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
                    ## Backlog Management (Backlog.md)

                    Use the **Backlog.md** MCP for all planning and task management:

                    - Create tasks for each module or feature before starting implementation
                    - Register epics for logical groupings of features
                    - Update task status with each progress: To Do → In Progress → Done
                    - Never start implementing a module without its task being created
                    - When completing a module, mark the task as Done before proceeding
                    - Use the `description` field to record implementation details and decisions
                    """
            },
            new()
            {
                Name = "GitHub Issues",
                IsSystemDefault = true,
                DefaultInstructions = """
                    ## Backlog Management (GitHub Issues)

                    Use **GitHub Issues** for task tracking:

                    - Create an issue for each feature or module
                    - Use labels to categorize: `feature`, `bug`, `enhancement`, `documentation`
                    - Associate issues with milestones to organize by version/sprint
                    - Reference issues in commits with `closes #123` for automatic closing
                    - Use the GitHub Project Board to visualize the flow (To Do / In Progress / Done)
                    """
            },
            new()
            {
                Name = "Jira",
                IsSystemDefault = true,
                DefaultInstructions = """
                    ## Backlog Management (Jira)

                    Use **Jira** for project management:

                    - Create Epics for main modules and Stories for features
                    - Use the flow: Backlog → To Do → In Progress → Code Review → Done
                    - Estimate using story points (Fibonacci)
                    - Link commits and PRs to issues using the ticket key (ex: PROJ-123)
                    - Update the issue status before starting and when completing each task
                    """
            },
            new()
            {
                Name = "Linear",
                IsSystemDefault = true,
                DefaultInstructions = """
                    ## Backlog Management (Linear)

                    Use **Linear** for agile task tracking:

                    - Create issues in Linear for each feature
                    - Organize by cycles for iterative sprints
                    - Use priorities: Urgent, High, Medium, Low
                    - Link Git branches following the pattern generated by Linear
                    - Update the status with each significant change in development
                    """
            },
            new()
            {
                Name = "Trello",
                IsSystemDefault = false,
                DefaultInstructions = """
                    ## Backlog Management (Trello)

                    Use **Trello** for visual organization:

                    - Create cards on the board for each task or feature
                    - Use lists: Backlog, In Progress, In Review, Done
                    - Add checklists inside cards for sub-tasks
                    - Use labels to categorize by type or priority
                    - Move cards as development progresses
                    """
            },
            new()
            {
                Name = "Azure DevOps Boards",
                IsSystemDefault = false,
                DefaultInstructions = """
                    ## Backlog Management (Azure DevOps Boards)

                    Use **Azure DevOps Boards** for work management:

                    - Create Work Items of type Epic, Feature and User Story according to hierarchy
                    - Use the flow: New → Active → Resolved → Closed
                    - Estimate Story Points in the effort field
                    - Link commits and PRs to Work Items with `#<ID>` in messages
                    - Configure the board with columns reflecting the team flow
                    """
            },
        };

        await db.BacklogTools.AddRangeAsync(tools);
        await db.SaveChangesAsync();
    }
}

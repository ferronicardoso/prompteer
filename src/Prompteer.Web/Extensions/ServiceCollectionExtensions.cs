using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Prompteer.Application.Mappings;
using Prompteer.Application.Services;
using Prompteer.Application.Validators;
using Prompteer.Infrastructure.Data;
using Prompteer.Infrastructure.Data.Repositories;
using Prompteer.Infrastructure.Seed;
using Prompteer.Infrastructure.Services;
using Prompteer.Domain.Interfaces;

namespace Prompteer.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                BuildConnectionString(configuration),
                b => b.MigrationsAssembly("Prompteer.Infrastructure")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // AutoMapper — v16 usa AddAutoMapper com tipo do profile
        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

        // FluentValidation
        services.AddValidatorsFromAssemblyContaining<AgentProfileValidator>();
        services.AddFluentValidationAutoValidation();

        // Application services (CRUD via IUnitOfWork)
        services.AddScoped<IAgentProfileService, AgentProfileService>();
        services.AddScoped<ITechnologyService, TechnologyService>();
        services.AddScoped<IArchitecturalPatternService, ArchitecturalPatternService>();
        services.AddScoped<IBacklogToolService, BacklogToolService>();

        // Infrastructure services (usam AppDbContext diretamente)
        services.AddScoped<IPromptTemplateService, PromptTemplateService>();
        services.AddScoped<IPromptDraftService, PromptDraftService>();
        services.AddScoped<IPromptBuilderService, PromptBuilderService>();
        services.AddHttpClient();
        services.AddScoped<IAppSettingService, AppSettingService>();
        services.AddScoped<IAIService, OpenAIService>();

        return services;
    }

    public static async Task SeedDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        await DatabaseSeeder.SeedAsync(db);
    }

    private static string BuildConnectionString(IConfiguration configuration)
    {
        var host     = configuration["POSTGRES_HOST"];
        var user     = configuration["POSTGRES_USER"];
        var password = configuration["POSTGRES_PASSWORD"];
        var database = configuration["POSTGRES_DB"] ?? configuration["POSTGRES_DATABASE"];

        if (!string.IsNullOrWhiteSpace(host) && !string.IsNullOrWhiteSpace(password))
        {
            return $"Host={host};Port=5432;" +
                   $"Database={database ?? "prompteer"};" +
                   $"Username={user ?? "postgres"};" +
                   $"Password={password}";
        }

        return configuration.GetConnectionString("DefaultConnection")
               ?? throw new InvalidOperationException(
                   "Database connection is not configured. " +
                   "Set POSTGRES_HOST, POSTGRES_USER, POSTGRES_PASSWORD and POSTGRES_DB " +
                   "or provide ConnectionStrings__DefaultConnection.");
    }
}
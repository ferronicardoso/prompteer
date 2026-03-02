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
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("Prompteer.Infrastructure")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

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
}




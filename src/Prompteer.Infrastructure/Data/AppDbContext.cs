using Microsoft.EntityFrameworkCore;
using Prompteer.Domain.Entities;

namespace Prompteer.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AgentProfile> AgentProfiles => Set<AgentProfile>();
    public DbSet<Technology> Technologies => Set<Technology>();
    public DbSet<ArchitecturalPattern> ArchitecturalPatterns => Set<ArchitecturalPattern>();
    public DbSet<BacklogTool> BacklogTools => Set<BacklogTool>();
    public DbSet<PromptTemplate> PromptTemplates => Set<PromptTemplate>();
    public DbSet<PromptTemplateVersion> PromptTemplateVersions => Set<PromptTemplateVersion>();
    public DbSet<PromptVersionTechnology> PromptVersionTechnologies => Set<PromptVersionTechnology>();
    public DbSet<PromptVersionPattern> PromptVersionPatterns => Set<PromptVersionPattern>();
    public DbSet<PromptModule> PromptModules => Set<PromptModule>();
    public DbSet<PromptModuleItem> PromptModuleItems => Set<PromptModuleItem>();
    public DbSet<PromptDraft> PromptDrafts => Set<PromptDraft>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Global query filter — soft delete
        modelBuilder.Entity<AgentProfile>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Technology>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ArchitecturalPattern>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<BacklogTool>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<PromptTemplate>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<PromptModule>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<PromptModuleItem>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is Domain.Common.BaseEntity baseEntity)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        baseEntity.CreatedAt = now;
                        baseEntity.UpdatedAt = now;
                        break;
                    case EntityState.Modified:
                        baseEntity.UpdatedAt = now;
                        break;
                }
            }
            else if (entry.Entity is PromptDraft draft)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        draft.CreatedAt = now;
                        draft.UpdatedAt = now;
                        break;
                    case EntityState.Modified:
                        draft.UpdatedAt = now;
                        break;
                }
            }
            else if (entry.Entity is PromptTemplateVersion version && entry.State == EntityState.Added)
            {
                version.CreatedAt = now;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}

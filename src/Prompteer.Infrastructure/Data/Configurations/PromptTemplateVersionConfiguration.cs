using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prompteer.Domain.Entities;

namespace Prompteer.Infrastructure.Data.Configurations;

public class PromptTemplateVersionConfiguration : IEntityTypeConfiguration<PromptTemplateVersion>
{
    public void Configure(EntityTypeBuilder<PromptTemplateVersion> builder)
    {
        builder.ToTable("prompt_template_versions");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.GeneratedPrompt).HasColumnType("text").IsRequired();
        builder.Property(e => e.WizardDataJson).HasColumnType("text").IsRequired();
        builder.HasIndex(e => new { e.PromptTemplateId, e.VersionNumber }).IsUnique();

        builder.HasMany(e => e.Technologies)
               .WithOne(t => t.Version)
               .HasForeignKey(t => t.PromptTemplateVersionId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Patterns)
               .WithOne(p => p.Version)
               .HasForeignKey(p => p.PromptTemplateVersionId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Modules)
               .WithOne(m => m.Version)
               .HasForeignKey(m => m.PromptTemplateVersionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

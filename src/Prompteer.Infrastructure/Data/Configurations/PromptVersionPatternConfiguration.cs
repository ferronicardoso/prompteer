using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prompteer.Domain.Entities;

namespace Prompteer.Infrastructure.Data.Configurations;

public class PromptVersionPatternConfiguration : IEntityTypeConfiguration<PromptVersionPattern>
{
    public void Configure(EntityTypeBuilder<PromptVersionPattern> builder)
    {
        builder.ToTable("prompt_version_patterns");
        builder.HasKey(e => new { e.PromptTemplateVersionId, e.ArchitecturalPatternId });

        builder.HasOne(e => e.Pattern)
               .WithMany(p => p.PromptVersionPatterns)
               .HasForeignKey(e => e.ArchitecturalPatternId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

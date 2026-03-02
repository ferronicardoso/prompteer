using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prompteer.Domain.Entities;

namespace Prompteer.Infrastructure.Data.Configurations;

public class PromptVersionTechnologyConfiguration : IEntityTypeConfiguration<PromptVersionTechnology>
{
    public void Configure(EntityTypeBuilder<PromptVersionTechnology> builder)
    {
        builder.ToTable("prompt_version_technologies");
        builder.HasKey(e => new { e.PromptTemplateVersionId, e.TechnologyId });

        builder.HasOne(e => e.Technology)
               .WithMany(t => t.PromptVersionTechnologies)
               .HasForeignKey(e => e.TechnologyId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

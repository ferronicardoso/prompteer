using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prompteer.Domain.Entities;

namespace Prompteer.Infrastructure.Data.Configurations;

public class PromptModuleConfiguration : IEntityTypeConfiguration<PromptModule>
{
    public void Configure(EntityTypeBuilder<PromptModule> builder)
    {
        builder.ToTable("prompt_modules");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(e => new { e.PromptTemplateVersionId, e.DisplayOrder });

        builder.HasMany(e => e.Items)
               .WithOne(i => i.Module)
               .HasForeignKey(i => i.PromptModuleId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

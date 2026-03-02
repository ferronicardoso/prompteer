using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prompteer.Domain.Entities;

namespace Prompteer.Infrastructure.Data.Configurations;

public class PromptTemplateConfiguration : IEntityTypeConfiguration<PromptTemplate>
{
    public void Configure(EntityTypeBuilder<PromptTemplate> builder)
    {
        builder.ToTable("prompt_templates");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Description).HasColumnType("text");
        builder.Property(e => e.CurrentVersionNumber).HasDefaultValue(0);
        builder.Property(e => e.IsPublic).HasDefaultValue(true);
        builder.HasIndex(e => new { e.IsDeleted, e.UpdatedAt });

        builder.HasMany(e => e.Versions)
               .WithOne(v => v.Template)
               .HasForeignKey(v => v.PromptTemplateId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

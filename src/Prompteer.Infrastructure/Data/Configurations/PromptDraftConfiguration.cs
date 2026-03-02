using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prompteer.Domain.Entities;

namespace Prompteer.Infrastructure.Data.Configurations;

public class PromptDraftConfiguration : IEntityTypeConfiguration<PromptDraft>
{
    public void Configure(EntityTypeBuilder<PromptDraft> builder)
    {
        builder.ToTable("prompt_drafts");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(200);
        builder.Property(e => e.WizardDataJson).HasColumnType("text").IsRequired();
        builder.Property(e => e.CurrentStep).HasDefaultValue(1);
    }
}

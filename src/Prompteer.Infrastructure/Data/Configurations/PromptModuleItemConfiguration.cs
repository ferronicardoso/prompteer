using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prompteer.Domain.Entities;

namespace Prompteer.Infrastructure.Data.Configurations;

public class PromptModuleItemConfiguration : IEntityTypeConfiguration<PromptModuleItem>
{
    public void Configure(EntityTypeBuilder<PromptModuleItem> builder)
    {
        builder.ToTable("prompt_module_items");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(300).IsRequired();
    }
}

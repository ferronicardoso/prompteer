using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prompteer.Domain.Entities;

namespace Prompteer.Infrastructure.Data.Configurations;

public class BacklogToolConfiguration : IEntityTypeConfiguration<BacklogTool>
{
    public void Configure(EntityTypeBuilder<BacklogTool> builder)
    {
        builder.ToTable("backlog_tools");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(100).IsRequired();
        builder.Property(e => e.DefaultInstructions).HasColumnType("text").IsRequired();
        builder.Property(e => e.IsSystemDefault).HasDefaultValue(false);
        builder.HasIndex(e => e.Name).IsUnique();
        builder.HasIndex(e => e.IsDeleted);
    }
}

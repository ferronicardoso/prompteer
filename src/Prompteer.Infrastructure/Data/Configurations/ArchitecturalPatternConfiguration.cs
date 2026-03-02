using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prompteer.Domain.Entities;

namespace Prompteer.Infrastructure.Data.Configurations;

public class ArchitecturalPatternConfiguration : IEntityTypeConfiguration<ArchitecturalPattern>
{
    public void Configure(EntityTypeBuilder<ArchitecturalPattern> builder)
    {
        builder.ToTable("architectural_patterns");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Description).HasColumnType("text").IsRequired();
        builder.HasIndex(e => e.IsDeleted);
    }
}

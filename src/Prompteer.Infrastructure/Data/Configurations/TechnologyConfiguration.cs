using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prompteer.Domain.Entities;

namespace Prompteer.Infrastructure.Data.Configurations;

public class TechnologyConfiguration : IEntityTypeConfiguration<Technology>
{
    public void Configure(EntityTypeBuilder<Technology> builder)
    {
        builder.ToTable("technologies");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Version).HasMaxLength(50);
        builder.Property(e => e.ShortDescription).HasMaxLength(300);
        builder.HasIndex(e => new { e.Category, e.Ecosystem });
        builder.HasIndex(e => e.IsDeleted);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Prompteer.Domain.Entities;

namespace Prompteer.Infrastructure.Data.Configurations;

public class AgentProfileConfiguration : IEntityTypeConfiguration<AgentProfile>
{
    public void Configure(EntityTypeBuilder<AgentProfile> builder)
    {
        builder.ToTable("agent_profiles");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Role).HasColumnType("text").IsRequired();
        builder.Property(e => e.KnowledgeDomain).HasColumnType("text").IsRequired();
        builder.Property(e => e.Tone).IsRequired();
        builder.Property(e => e.DefaultConstraints).HasColumnType("text");
        builder.Property(e => e.IsSystemDefault).HasDefaultValue(false);
        builder.HasIndex(e => e.Name).IsUnique();
        builder.HasIndex(e => e.IsDeleted);
    }
}

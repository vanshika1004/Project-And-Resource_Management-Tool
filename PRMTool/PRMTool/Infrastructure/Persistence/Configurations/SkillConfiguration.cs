using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        builder.ToTable("Skills");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.SkillName).IsRequired().HasMaxLength(100);

        builder.Property(s => s.Category).IsRequired().HasMaxLength(100);

        builder.HasIndex(s => s.SkillName).IsUnique();
    }
}
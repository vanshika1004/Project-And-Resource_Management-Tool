using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ActivityTagConfiguration : IEntityTypeConfiguration<ActivityTag>
{
    public void Configure(EntityTypeBuilder<ActivityTag> builder)
    {
        builder.ToTable("ActivityTags");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.TagName)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(a => a.TagName).IsUnique();
    }
}
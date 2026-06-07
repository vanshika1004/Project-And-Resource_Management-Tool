using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class MilestoneConfiguration : IEntityTypeConfiguration<Milestone>
{
    public void Configure(EntityTypeBuilder<Milestone> builder)
    {
        builder.ToTable("Milestones");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title).IsRequired().HasMaxLength(200);

        builder.Property(m => m.Status).IsRequired();

        builder.HasOne(m => m.Project)
            .WithMany(p => p.Milestones)
            .HasForeignKey(m => m.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
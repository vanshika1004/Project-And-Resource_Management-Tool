using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.ProjectName).IsRequired().HasMaxLength(200);

        builder.Property(p => p.Description).HasMaxLength(1000);

        builder.Property(p => p.Status).IsRequired();

        builder.Property(p => p.HealthStatus).IsRequired();

        builder.Property(p => p.TotalStoryPoints).HasDefaultValue(0);

        builder.HasOne(p => p.Manager)
            .WithMany(u => u.ManagedProjects)
            .HasForeignKey(p => p.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
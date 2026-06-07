using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AllocationConfiguration : IEntityTypeConfiguration<Allocation>
{
    public void Configure(EntityTypeBuilder<Allocation> builder)
    {
        builder.ToTable("Allocations");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.UtilizationPercent).HasPrecision(5, 2);

        builder.HasOne(a => a.Employee)
            .WithMany(e => e.Allocations)
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Project)
            .WithMany(p => p.Allocations)
            .HasForeignKey(a => a.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
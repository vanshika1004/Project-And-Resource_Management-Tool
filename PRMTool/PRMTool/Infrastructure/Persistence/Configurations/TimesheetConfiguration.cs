using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class TimesheetConfiguration : IEntityTypeConfiguration<Timesheet>
{
    public void Configure(EntityTypeBuilder<Timesheet> builder)
    {
        builder.ToTable("Timesheets");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.TotalHours).HasPrecision(5, 2);

        builder.Property(t => t.Status).IsRequired();

        builder.HasOne(t => t.Employee)
            .WithMany(e => e.Timesheets)
            .HasForeignKey(t => t.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
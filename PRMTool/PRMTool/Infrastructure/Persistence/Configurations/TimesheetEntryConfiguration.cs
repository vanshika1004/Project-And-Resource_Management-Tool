using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class TimesheetEntryConfiguration : IEntityTypeConfiguration<TimesheetEntry>
{
    public void Configure(EntityTypeBuilder<TimesheetEntry> builder)
    {
        builder.ToTable("TimesheetEntries");

        builder.HasKey(te => te.Id);

        builder.Property(te => te.HoursWorked).HasPrecision(5, 2);

        builder.HasOne(te => te.Timesheet).WithMany(t => t.Entries)
            .HasForeignKey(te => te.TimesheetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(te => te.Project)
            .WithMany(p => p.TimesheetEntries)
            .HasForeignKey(te => te.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class TimesheetEntryActivityTagConfiguration : IEntityTypeConfiguration<TimesheetEntryActivityTag>
{
    public void Configure(
        EntityTypeBuilder<TimesheetEntryActivityTag> builder)
    {
        builder.ToTable("TimesheetEntryActivityTags");

        builder.HasKey(t => t.Id);

        builder.HasOne(t => t.TimesheetEntry)
            .WithMany(te => te.ActivityTags)
            .HasForeignKey(t => t.TimesheetEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.ActivityTag)
            .WithMany(a => a.TimesheetEntryTags)
            .HasForeignKey(t => t.ActivityTagId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class SystemConfigurationEntityConfiguration
    : IEntityTypeConfiguration<SystemConfiguration>
{
    public void Configure(
        EntityTypeBuilder<SystemConfiguration> builder)
    {
        builder.ToTable("SystemConfigurations");

        builder.HasKey(sc => sc.Id);

        builder.Property(sc => sc.LLMProvider).HasMaxLength(100);

        builder.Property(sc => sc.ApiKey).HasMaxLength(500);

        builder.Property(sc => sc.SchedulerInterval).IsRequired();

        builder.Property(sc => sc.MaxWeeklyHours).IsRequired();
    }
}
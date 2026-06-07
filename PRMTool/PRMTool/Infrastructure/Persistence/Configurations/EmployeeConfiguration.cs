using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.FullName).IsRequired().HasMaxLength(150);

        builder.Property(e => e.Department).IsRequired().HasMaxLength(100);

        builder.Property(e => e.Designation).IsRequired().HasMaxLength(100);

        builder.Property(e => e.Status).IsRequired();

        builder.Property(e => e.IsActive).HasDefaultValue(true);

        // Employee ↔ User (1:1)

        builder.HasOne(e => e.User)
            .WithOne(u => u.Employee)
            .HasForeignKey<Employee>(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Employee ↔ Manager (Self Reference)

        builder.HasOne(e => e.Manager)
            .WithMany(e => e.TeamMembers)
            .HasForeignKey(e => e.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
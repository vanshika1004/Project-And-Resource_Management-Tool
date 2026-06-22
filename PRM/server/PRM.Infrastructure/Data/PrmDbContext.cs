using Microsoft.EntityFrameworkCore;
using PRM.Core.Entities;

namespace PRM.Infrastructure.Data;

public class PrmDbContext : DbContext
{
    public PrmDbContext(DbContextOptions<PrmDbContext> options) : base(options)
    {
    }

    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<UserSkill> UserSkills => Set<UserSkill>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Milestone> Milestones => Set<Milestone>();
    public DbSet<Allocation> Allocations => Set<Allocation>();
    public DbSet<Timesheet> Timesheets => Set<Timesheet>();
    public DbSet<TimesheetEntry> TimesheetEntries => Set<TimesheetEntry>();
    public DbSet<SystemConfig> SystemConfigs => Set<SystemConfig>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // RolePermission composite key
        modelBuilder.Entity<RolePermission>()
            .HasKey(rp => new { rp.RoleId, rp.PermissionId });

        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId);

        // UserSkill composite key
        modelBuilder.Entity<UserSkill>()
            .HasKey(us => new { us.UserId, us.SkillId });

        // User self-referencing relationship for Manager
        modelBuilder.Entity<User>()
            .HasOne(u => u.Manager)
            .WithMany(m => m.DirectReports)
            .HasForeignKey(u => u.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ensure unique usernames and emails
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();
            
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Project and Manager relationship
        modelBuilder.Entity<Project>()
            .HasOne(p => p.Manager)
            .WithMany()
            .HasForeignKey(p => p.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        // SystemConfig primary key
        modelBuilder.Entity<SystemConfig>()
            .HasKey(sc => sc.Key);
            
        // Convert enums to strings in DB for readability
        modelBuilder.Entity<Skill>()
            .Property(s => s.Category)
            .HasConversion<string>();
            
        modelBuilder.Entity<UserSkill>()
            .Property(us => us.ProficiencyLevel)
            .HasConversion<string>();
            
        modelBuilder.Entity<Project>()
            .Property(p => p.Status)
            .HasConversion<string>();
            
        modelBuilder.Entity<Project>()
            .Property(p => p.HealthStatus)
            .HasConversion<string>();
            
        modelBuilder.Entity<Milestone>()
            .Property(m => m.Status)
            .HasConversion<string>();
            
        modelBuilder.Entity<Timesheet>()
            .Property(t => t.Status)
            .HasConversion<string>();
            
        modelBuilder.Entity<TimesheetEntry>()
            .Property(e => e.HoursWorked)
            .HasPrecision(5, 2);
    }
}

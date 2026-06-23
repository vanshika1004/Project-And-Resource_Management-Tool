using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PRM.Core.Constants;
using PRM.Core.Entities;
using PRM.Application.Security;

namespace PRM.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(PrmDbContext context, IConfiguration config)
    {
        // Run pending migrations
        await context.Database.MigrateAsync();

        // Seed Roles
        if (!await context.Roles.AnyAsync())
        {
            await context.Roles.AddRangeAsync(
                new Role { Name = PrmConstants.Roles.Admin, Description = "System Administrator" },
                new Role { Name = PrmConstants.Roles.Manager, Description = "Delivery Manager" },
                new Role { Name = PrmConstants.Roles.Resource, Description = "Individual Contributor" }
            );
            await context.SaveChangesAsync();
        }

        // Seed Permissions
        if (!await context.Permissions.AnyAsync())
        {
            var permissions = new[]
            {
                new Permission { Name = "Users.Manage", Description = "Create and manage users" },
                new Permission { Name = "Resources.Manage", Description = "Manage resource profiles and skills" },
                new Permission { Name = "Projects.Manage", Description = "Create and update projects and milestones" },
                new Permission { Name = "System.Configure", Description = "Configure system settings" },
                new Permission { Name = "Allocations.ViewAll", Description = "View company-wide allocations" },
                new Permission { Name = "Resources.Allocate", Description = "Allocate resources and perform AI matches" },
                new Permission { Name = "Projects.ViewTeam", Description = "View projects and project health" },
                new Permission { Name = "Timesheets.ViewTeam", Description = "View team timesheets" },
                new Permission { Name = "Timesheets.SubmitOwn", Description = "Submit own timesheet" },
                new Permission { Name = "Allocations.ViewOwn", Description = "View own allocations" }
            };

            await context.Permissions.AddRangeAsync(permissions);
            await context.SaveChangesAsync();
        }

        // Seed Role Permissions
        if (!await context.RolePermissions.AnyAsync())
        {
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == PrmConstants.Roles.Admin);
            var managerRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == PrmConstants.Roles.Manager);
            var resourceRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == PrmConstants.Roles.Resource);

            var perms = await context.Permissions.ToListAsync();

            if (adminRole != null && managerRole != null && resourceRole != null)
            {
                // Admin Permissions
                foreach (var p in perms.Where(p => p.Name.StartsWith("Users.") || p.Name.StartsWith("Resources.Manage") || p.Name.StartsWith("Projects.Manage") || p.Name.StartsWith("System.") || p.Name.StartsWith("Allocations.ViewAll")))
                {
                    context.RolePermissions.Add(new RolePermission { RoleId = adminRole.Id, PermissionId = p.Id });
                }

                // Manager Permissions
                foreach (var p in perms.Where(p => p.Name.StartsWith("Resources.Allocate") || p.Name.StartsWith("Projects.ViewTeam") || p.Name.StartsWith("Timesheets.ViewTeam")))
                {
                    context.RolePermissions.Add(new RolePermission { RoleId = managerRole.Id, PermissionId = p.Id });
                }

                // Resource Permissions
                foreach (var p in perms.Where(p => p.Name.StartsWith("Timesheets.SubmitOwn") || p.Name.StartsWith("Allocations.ViewOwn")))
                {
                    context.RolePermissions.Add(new RolePermission { RoleId = resourceRole.Id, PermissionId = p.Id });
                }

                await context.SaveChangesAsync();
            }
        }

        // Seed Admin User
        if (!await context.Users.AnyAsync(u => u.Username == "admin"))
        {
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == PrmConstants.Roles.Admin);
            if (adminRole != null)
            {
                var username = config["AdminSeed:Username"] ?? PrmConstants.DefaultAdminUsername;
                var email = config["AdminSeed:Email"] ?? "admin@techserve.com";
                var rawPassword = config["AdminSeed:Password"] ?? PrmConstants.DefaultAdminPassword;
                var passwordHash = PasswordHasher.Hash(rawPassword);

                var adminUser = new User
                {
                    Username = username,
                    Email = email,
                    PasswordHash = passwordHash,
                    RoleId = adminRole.Id,
                    FullName = config["AdminSeed:FullName"] ?? "System Administrator",
                    ForcePasswordChange = true, // Admin must change password on first login
                    IsActive = true
                };

                await context.Users.AddAsync(adminUser);
                await context.SaveChangesAsync();
            }
        }

        // Seed Default System Configs
        var defaultConfigs = new Dictionary<string, string>
        {
            { "SmtpHost", "smtp.example.com" },
            { "SmtpPort", "587" },
            { "SmtpUser", "user@example.com" },
            { "SmtpPassword", "password" },
            { "FromEmail", "no-reply@techserve.com" }
        };

        foreach (var kvp in defaultConfigs)
        {
            if (!await context.SystemConfigs.AnyAsync(c => c.Key == kvp.Key))
            {
                context.SystemConfigs.Add(new SystemConfig { Key = kvp.Key, Value = kvp.Value });
            }
        }
        await context.SaveChangesAsync();
    }
}

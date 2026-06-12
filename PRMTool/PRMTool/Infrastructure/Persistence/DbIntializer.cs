using BCrypt.Net;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public static class DbInitializer
{
    public static async Task SeedAdminAsync(
        ApplicationDbContext context)
    {
        if (await context.Users.AnyAsync(u => u.Username == "riyamehta"))
            return;

        User admin = await context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
        if (admin == null)
        {
            admin = new User
            {
                FullName = "System Administrator",
                Username = "admin",
                Email = "admin@prmtool.com",
                Role = UserRole.Admin,
                ForcePasswordChange = true,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@1234")
            };
            context.Users.Add(admin);
        }

        var manager = new User
        {
            FullName = "Sarah Manager",
            Username = "sarahmanager",
            Email = "sarah@prmtool.com",
            Role = UserRole.Manager,
            ForcePasswordChange = false,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager@1234")
        };
        context.Users.Add(manager);

        var employee = new User
        {
            FullName = "Riya Mehta",
            Username = "riyamehta",
            Email = "riya@prmtool.com",
            Role = UserRole.Employee,
            ForcePasswordChange = false,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Riya@1234")
        };
        context.Users.Add(employee);

        await context.SaveChangesAsync();

        var empRecord = new Employee
        {
            Id = employee.Id,
            UserId = employee.Id,
            FullName = "Riya Mehta",
            Department = "Engineering",
            ManagerId = manager.Id,
            Status = EmployeeStatus.Bench,
            IsActive = true
        };
        context.Employees.Add(empRecord);

        var project = new Project
        {
            ManagerId = manager.Id,
            ProjectName = "AI Integrations",
            Description = "Integrate Gemini and Groq",
            StartDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(30),
            Status = ProjectStatus.Active,
            HealthStatus = ProjectHealthStatus.OnTrack,
            TotalStoryPoints = 100
        };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        context.Milestones.Add(new Milestone
        {
            ProjectId = project.Id,
            Title = "Backend Setup",
            DueDate = DateTime.UtcNow.AddDays(5),
            Status = MilestoneStatus.InProgress
        });

        context.Allocations.Add(new Allocation
        {
            EmployeeId = empRecord.Id,
            ProjectId = project.Id,
            UtilizationPercent = 50,
            FromDate = DateTime.UtcNow.AddDays(-5),
            ToDate = DateTime.UtcNow.AddDays(20)
        });

        await context.SaveChangesAsync();
    }
}
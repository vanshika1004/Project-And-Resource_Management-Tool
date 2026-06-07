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
        if (await context.Users.AnyAsync())
            return;

        var admin = new User
        {
            FullName = "System Administrator",
            Username = "admin",
            Email = "admin@prmtool.com",
            Role = UserRole.Admin,
            ForcePasswordChange = true,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@1234")
        };

        context.Users.Add(admin);

        await context.SaveChangesAsync();
    }
}
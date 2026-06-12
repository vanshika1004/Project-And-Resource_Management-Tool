using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;
using Domain.Entities;

namespace DbCheck
{
    class Program
    {
        static void Main(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=PRMToolDB;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;");
            
            using var context = new ApplicationDbContext(optionsBuilder.Options);
            
            var users = context.Users.ToList();
            Console.WriteLine($"Total Users: {users.Count}");
            foreach(var u in users)
            {
                Console.WriteLine($"- ID: {u.Id}, Username: {u.Username}, Role: {u.Role}");
            }
        }
    }
}

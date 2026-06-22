using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PRM.Infrastructure.Data;

public class PrmDbContextFactory : IDesignTimeDbContextFactory<PrmDbContext>
{
    public PrmDbContext CreateDbContext(string[] args)
    {
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../PRM.API"))
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            connectionString = "Server=(localdb)\\mssqllocaldb;Database=TechServe_ResourceDB;Trusted_Connection=True;MultipleActiveResultSets=true";
        }

        var optionsBuilder = new DbContextOptionsBuilder<PrmDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new PrmDbContext(optionsBuilder.Options);
    }
}

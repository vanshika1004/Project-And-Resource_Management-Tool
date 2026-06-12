using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class SystemConfigurationRepository : ISystemConfigurationRepository
{
    private readonly ApplicationDbContext _dbContext;

    public SystemConfigurationRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SystemConfiguration?> GetAsync()
    {
        return await _dbContext.SystemConfigurations.FirstOrDefaultAsync();
    }

    public async Task AddAsync(SystemConfiguration config)
    {
        await _dbContext.SystemConfigurations.AddAsync(config);
    }

    public void Update(SystemConfiguration config)
    {
        _dbContext.SystemConfigurations.Update(config);
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}

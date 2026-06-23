using Microsoft.EntityFrameworkCore;
using PRM.Core.Interfaces;
using PRM.Infrastructure.Data;
using System.Threading.Tasks;

namespace PRM.Infrastructure.Repositories;

public class SystemConfigRepository : ISystemConfigRepository
{
    private readonly PrmDbContext _dbContext;

    public SystemConfigRepository(PrmDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string?> GetValueAsync(string key)
    {
        var config = await _dbContext.SystemConfigs.FirstOrDefaultAsync(c => c.Key == key);
        return config?.Value;
    }
}

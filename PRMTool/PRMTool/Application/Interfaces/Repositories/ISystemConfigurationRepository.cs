using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface ISystemConfigurationRepository
{
    Task<SystemConfiguration?> GetAsync();
    Task AddAsync(SystemConfiguration config);
    void Update(SystemConfiguration config);
    Task SaveChangesAsync();
}

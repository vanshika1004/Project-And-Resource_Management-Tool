using System.Collections.Generic;
using System.Threading.Tasks;
using PRM.Core.Entities;

namespace PRM.Core.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<IEnumerable<User>> GetAllAsync(bool includeInactive = false);
    Task<IEnumerable<User>> GetByManagerIdAsync(int managerId);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
}

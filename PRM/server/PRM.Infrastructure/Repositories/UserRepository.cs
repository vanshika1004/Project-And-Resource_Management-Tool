using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PRM.Core.Entities;
using PRM.Core.Interfaces;
using PRM.Infrastructure.Data;

namespace PRM.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly PrmDbContext _context;

    public UserRepository(PrmDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users
            .Include(u => u.Role)
            .Include(u => u.UserSkills)
                .ThenInclude(us => us.Skill)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .Include(u => u.Role)
                .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<IEnumerable<User>> GetAllAsync(bool includeInactive = false)
    {
        var query = _context.Users
            .Include(u => u.Role)
            .Include(u => u.UserSkills)
                .ThenInclude(us => us.Skill)
            .AsQueryable();
        
        if (!includeInactive)
            query = query.Where(u => u.IsActive);
            
        return await query.ToListAsync();
    }

    public async Task<IEnumerable<User>> GetByManagerIdAsync(int managerId)
    {
        return await _context.Users
            .Include(u => u.Role)
            .Include(u => u.UserSkills)
                .ThenInclude(us => us.Skill)
            .Where(u => u.ManagerId == managerId && u.IsActive)
            .ToListAsync();
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
}

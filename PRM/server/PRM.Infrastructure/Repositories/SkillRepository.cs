using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PRM.Core.Entities;
using PRM.Core.Interfaces;
using PRM.Infrastructure.Data;

namespace PRM.Infrastructure.Repositories;

public class SkillRepository : ISkillRepository
{
    private readonly PrmDbContext _context;

    public SkillRepository(PrmDbContext context)
    {
        _context = context;
    }

    public async Task<Skill?> GetByNameAsync(string name)
    {
        return await _context.Skills
            .FirstOrDefaultAsync(s => s.Name.ToLower() == name.ToLower());
    }

    public async Task<Skill> AddAsync(Skill skill)
    {
        await _context.Skills.AddAsync(skill);
        await _context.SaveChangesAsync();
        return skill;
    }
}

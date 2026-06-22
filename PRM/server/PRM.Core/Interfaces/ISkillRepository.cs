using System.Threading.Tasks;
using PRM.Core.Entities;

namespace PRM.Core.Interfaces;

public interface ISkillRepository
{
    Task<Skill?> GetByNameAsync(string name);
    Task<Skill> AddAsync(Skill skill);
}

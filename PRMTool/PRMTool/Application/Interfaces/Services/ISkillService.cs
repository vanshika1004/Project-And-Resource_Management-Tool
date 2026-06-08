using Application.DTOs.Skill;

namespace Application.Interfaces.Services;

public interface ISkillService
{
    Task<int> CreateSkillAsync(CreateSkillRequestDto request);

    Task<List<SkillDto>> GetAllAsync();
}
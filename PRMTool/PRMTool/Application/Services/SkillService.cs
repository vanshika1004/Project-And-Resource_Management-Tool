using Application.DTOs.Skill;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Application.Services;

public class SkillService : ISkillService
{
    private readonly ISkillRepository _skillRepository;

    public SkillService(ISkillRepository skillRepository)
    {
        _skillRepository = skillRepository;
    }

    public async Task<int> CreateSkillAsync(CreateSkillRequestDto request)
    {
        var existingSkill = await _skillRepository.GetByNameAsync(request.SkillName);

        if (existingSkill != null)
        {
            throw new Exception("Skill already exists.");
        }

        var skill = new Skill
        {
            SkillName = request.SkillName,
            Category = request.Category
        };

        await _skillRepository.AddAsync(skill);

        await _skillRepository.SaveChangesAsync();

        return skill.Id;
    }

    public async Task<List<SkillDto>> GetAllAsync()
    {
        var skills = await _skillRepository.GetAllAsync();

        return skills.Select(s => new SkillDto
        {
            Id = s.Id,
            SkillName = s.SkillName,
            Category = s.Category
        }).ToList();
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PRM.Application.DTOs;
using PRM.Application.Exceptions;
using PRM.Application.Interfaces;
using PRM.Application.Security;
using PRM.Core.Entities;
using PRM.Core.Interfaces;

namespace PRM.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepo;
    private readonly IAllocationRepository _allocationRepo;
    private readonly ISkillRepository _skillRepo;
    private readonly ITimesheetRepository _timesheetRepo;

    public UserService(IUserRepository userRepo, IAllocationRepository allocationRepo, ISkillRepository skillRepo, ITimesheetRepository timesheetRepo)
    {
        _userRepo = userRepo;
        _allocationRepo = allocationRepo;
        _skillRepo = skillRepo;
        _timesheetRepo = timesheetRepo;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepo.GetAllAsync(includeInactive: true);
        return users.Select(MapToDto);
    }

    public async Task<UserDto> GetUserByIdAsync(int id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null) throw new NotFoundException("User", id);
        return MapToDto(user);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
    {
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            FullName = request.FullName,
            RoleId = request.RoleId,
            Department = request.Department,
            ManagerId = request.ManagerId > 0 ? request.ManagerId : null,
            PasswordHash = PasswordHasher.Hash(request.TemporaryPassword),
            ForcePasswordChange = true,
            IsActive = true
        };

        await _userRepo.AddAsync(user);
        return await GetUserByIdAsync(user.Id);
    }

    public async Task UpdateUserAsync(UpdateUserRequest request)
    {
        var user = await _userRepo.GetByIdAsync(request.Id);
        if (user == null) throw new NotFoundException("User", request.Id);

        user.FullName = request.FullName;
        user.Email = request.Email;
        user.Department = request.Department;
        user.ManagerId = request.ManagerId > 0 ? request.ManagerId : null;
        user.IsActive = request.IsActive;

        await _userRepo.UpdateAsync(user);
    }

    public async Task DeactivateUserAsync(int id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null) throw new NotFoundException("User", id);

        user.IsActive = false;

        var allocations = await _allocationRepo.GetByResourceIdAsync(id);
        var activeAllocations = allocations.Where(a => a.ToDate >= DateTime.UtcNow.Date).ToList();

        foreach (var allocation in activeAllocations)
        {
            await _allocationRepo.EndAllocationAsync(allocation.Id, DateTime.UtcNow.Date);
        }

        await _userRepo.UpdateAsync(user);
    }

    public async Task ReactivateUserAsync(int id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null) throw new NotFoundException("User", id);

        user.IsActive = true;
        await _userRepo.UpdateAsync(user);
    }

    public async Task ResetPasswordAsync(int id, string newTemporaryPassword)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null) throw new NotFoundException("User", id);

        user.PasswordHash = PasswordHasher.Hash(newTemporaryPassword);
        user.ForcePasswordChange = true;
        await _userRepo.UpdateAsync(user);
    }

    public async Task<IEnumerable<UserSkillDto>> GetUserSkillsAsync(int userId)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null) throw new NotFoundException("User", userId);

        return user.UserSkills.Select(us => new UserSkillDto(us.SkillId, us.Skill.Name, us.Skill.Category, us.ProficiencyLevel));
    }

    public async Task AddSkillAsync(int userId, AddSkillRequest request)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null) throw new NotFoundException("User", userId);

        var skill = await _skillRepo.GetByNameAsync(request.SkillName);
        if (skill == null)
        {
            skill = await _skillRepo.AddAsync(new Skill
            {
                Name = request.SkillName,
                Category = request.Category
            });
        }

        if (user.UserSkills.Any(us => us.SkillId == skill.Id))
        {
            throw new BusinessRuleViolationException($"User already has the skill '{skill.Name}'.");
        }

        user.UserSkills.Add(new UserSkill
        {
            SkillId = skill.Id,
            ProficiencyLevel = request.ProficiencyLevel
        });

        await _userRepo.UpdateAsync(user);
    }

    public async Task UpdateSkillAsync(int userId, int skillId, UpdateSkillRequest request)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null) throw new NotFoundException("User", userId);

        var userSkill = user.UserSkills.FirstOrDefault(us => us.SkillId == skillId);
        if (userSkill == null) throw new NotFoundException("UserSkill", skillId);

        userSkill.ProficiencyLevel = request.ProficiencyLevel;
        await _userRepo.UpdateAsync(user);
    }

    public async Task RemoveSkillAsync(int userId, int skillId)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null) throw new NotFoundException("User", userId);

        var userSkill = user.UserSkills.FirstOrDefault(us => us.SkillId == skillId);
        if (userSkill == null) throw new NotFoundException("UserSkill", skillId);

        user.UserSkills.Remove(userSkill);
        await _userRepo.UpdateAsync(user);
    }

    public async Task<IEnumerable<UserDto>> GetTeamMembersAsync(int managerId)
    {
        var users = await _userRepo.GetByManagerIdAsync(managerId);
        return users.Select(MapToDto);
    }

    public async Task<IEnumerable<TeamDashboardMemberDto>> GetTeamDashboardAsync(int managerId)
    {
        var users = await _userRepo.GetByManagerIdAsync(managerId);
        var result = new List<TeamDashboardMemberDto>();
        var today = DateTime.UtcNow.Date;

        foreach (var user in users)
        {
            var allocations = await _allocationRepo.GetByResourceIdAsync(user.Id);
            int allocPercent = allocations
                .Where(a => a.FromDate <= today && a.ToDate >= today)
                .Sum(a => a.UtilisationPercent);

            string skills = user.UserSkills != null && user.UserSkills.Any() 
                ? string.Join(", ", user.UserSkills.Select(us => us.Skill.Name)) 
                : "";

            result.Add(new TeamDashboardMemberDto(
                user.Id,
                user.FullName,
                user.Department,
                allocPercent,
                skills
            ));
        }

        return result;
    }

    public async Task<TeamMemberDetailsDto> GetTeamMemberDetailsAsync(int id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null) throw new NotFoundException("User", id);

        var today = DateTime.UtcNow.Date;
        var allAllocations = await _allocationRepo.GetByResourceIdAsync(user.Id);
        var activeAllocations = allAllocations
            .Where(a => a.ToDate >= today)
            .ToList();

        int allocPercent = activeAllocations
            .Where(a => a.FromDate <= today)
            .Sum(a => a.UtilisationPercent);

        string statusStr = allocPercent > 0 ? $"ALLOCATED ({allocPercent}%)" : "BENCH";

        string profileSkills = user.UserSkills != null && user.UserSkills.Any() 
            ? string.Join(", ", user.UserSkills.Select(us => us.Skill.Name)) 
            : "";

        var activeAllocDtos = activeAllocations.Select(a => new ActiveAllocationDto(
            a.Project.Name,
            a.UtilisationPercent,
            a.FromDate,
            a.ToDate
        )).ToList();

        // Get recent activity tags
        var fourWeeksAgo = today.AddDays(-28);
        var timesheets = await _timesheetRepo.GetByResourceIdAsync(user.Id);
        
        var recentTags = timesheets
            .Where(t => t.WeekStartDate >= fourWeeksAgo)
            .SelectMany(t => t.Entries)
            .Select(e => e.ActivityTags)
            .Where(tags => !string.IsNullOrWhiteSpace(tags))
            .SelectMany(tags => tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Distinct()
            .ToList();

        return new TeamMemberDetailsDto(
            user.Id,
            user.FullName,
            user.Department,
            statusStr,
            profileSkills,
            activeAllocDtos,
            recentTags
        );
    }

    public async Task UnfreezeTimesheetAccessAsync(int userId, int managerId)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null) throw new NotFoundException("User", userId);

        if (user.ManagerId != managerId)
            throw new BusinessRuleViolationException("You can only unfreeze your own team members.");

        user.IsTimesheetFrozen = false;
        await _userRepo.UpdateAsync(user);
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto(
            user.Id, user.Username, user.Email, user.FullName, 
            user.Role?.Name ?? "Unknown", user.Department, user.ManagerId, user.IsActive);
    }
}

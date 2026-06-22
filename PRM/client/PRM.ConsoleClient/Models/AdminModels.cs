using System;
using System.Collections.Generic;

namespace PRM.ConsoleClient.Models;

public enum SkillCategory
{
    Backend,
    Frontend,
    DevOps,
    QA,
    Other
}

public enum ProficiencyLevel
{
    Beginner,
    Intermediate,
    Advanced
}

public enum ProjectStatus
{
    Planned,
    Active,
    OnHold,
    Completed
}

public enum MilestoneStatus
{
    NotStarted,
    InProgress,
    Done
}

// User Models
public record UserDto(int Id, string Username, string Email, string FullName, string Role, string? Department, int? ManagerId, bool IsActive);
public record CreateUserRequest(string Username, string Email, string FullName, int RoleId, string? Department, int? ManagerId, string TemporaryPassword);
public record UpdateUserRequest(int Id, string FullName, string Email, string? Department, int? ManagerId, bool IsActive);

// Skills Models
public record UserSkillDto(int SkillId, string SkillName, SkillCategory Category, ProficiencyLevel ProficiencyLevel);
public record ResetPasswordRequest(string NewTemporaryPassword);
public record AddSkillRequest(string SkillName, SkillCategory Category, ProficiencyLevel ProficiencyLevel);
public record UpdateSkillRequest(ProficiencyLevel ProficiencyLevel);

public enum HealthStatus
{
    OnTrack,
    Attention,
    AtRisk
}

// Project Models
public record ProjectDto(int Id, string Name, string Description, DateTime StartDate, DateTime EndDate, ProjectStatus Status, int? ManagerId, int TotalStoryPoints, HealthStatus HealthStatus, List<MilestoneDto> Milestones);
public record CreateProjectRequest(string Name, string Description, DateTime StartDate, DateTime EndDate, int? ManagerId, int TotalStoryPoints);
public record UpdateProjectRequest(int Id, string Name, string Description, DateTime StartDate, DateTime EndDate, ProjectStatus Status, int? ManagerId, int TotalStoryPoints, HealthStatus HealthStatus);

public record MilestoneDto(int Id, int ProjectId, string Title, DateTime DueDate, int StoryPoints, MilestoneStatus Status);
public record CreateMilestoneRequest(int ProjectId, string Title, DateTime DueDate, int StoryPoints);

// Allocation Models
public record AllocationDto(int Id, int UserId, string UserFullName, int ProjectId, string ProjectName, int UtilisationPercent, DateTime FromDate, DateTime ToDate);

// Config Models
public record SystemConfigDto(string Key, string Value);

// AI Models
public record TeamBuildRoleRequest(string Title, List<string> RequiredSkills, ProficiencyLevel MinimumProficiency);
public record TeamBuildRequest(int ProjectId, List<TeamBuildRoleRequest> Roles);

public record TeamBuildRoleResultDto(string Title, int? AssignedUserId, string? AssignedUserName, string Reason);
public record TeamBuildResultDto(List<TeamBuildRoleResultDto> Matches, string AiSummary);

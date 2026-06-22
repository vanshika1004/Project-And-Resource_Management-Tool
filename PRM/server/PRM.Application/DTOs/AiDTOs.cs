namespace PRM.Application.DTOs;

public record AiSkillMatchRequest(int? ProjectId, string Requirement);

public record TeamBuildRoleRequest(string Title, System.Collections.Generic.List<string> RequiredSkills, PRM.Core.Enums.ProficiencyLevel MinimumProficiency);
public record TeamBuildRequest(int ProjectId, System.Collections.Generic.List<TeamBuildRoleRequest> Roles);

public record TeamBuildRoleResultDto(string Title, int? AssignedUserId, string? AssignedUserName, string Reason);
public record TeamBuildResultDto(System.Collections.Generic.List<TeamBuildRoleResultDto> Matches, string AiSummary);

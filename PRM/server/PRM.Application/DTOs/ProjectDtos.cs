using System;
using System.Collections.Generic;
using PRM.Core.Enums;

namespace PRM.Application.DTOs;

public record ProjectDto(int Id, string Name, string? Description, DateTime StartDate, DateTime EndDate, ProjectStatus Status, int? ManagerId, int TotalStoryPoints, HealthStatus HealthStatus, IEnumerable<MilestoneDto> Milestones, IEnumerable<AllocationDto>? Allocations = null);
public record CreateProjectRequest(string Name, string? Description, DateTime StartDate, DateTime EndDate, int? ManagerId, int TotalStoryPoints);
public record UpdateProjectRequest(int Id, string Name, string? Description, DateTime StartDate, DateTime EndDate, ProjectStatus Status, int? ManagerId, int TotalStoryPoints, HealthStatus HealthStatus);

public record MilestoneDto(int Id, int ProjectId, string Title, DateTime DueDate, int StoryPoints, MilestoneStatus Status);
public record CreateMilestoneRequest(int ProjectId, string Title, DateTime DueDate, int StoryPoints);
public record UpdateMilestoneRequest(int Id, string Title, DateTime DueDate, int StoryPoints, MilestoneStatus Status);

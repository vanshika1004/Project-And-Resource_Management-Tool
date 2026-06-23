using System;

namespace PRM.Application.DTOs;

public record AllocationDto(int Id, int UserId, string UserFullName, int ProjectId, string ProjectName, int UtilisationPercent, DateTime FromDate, DateTime ToDate);
public record CreateAllocationRequest(int UserId, int ProjectId, int UtilisationPercent, DateTime FromDate, DateTime ToDate);
public record UpdateAllocationRequest(int Id, int UtilisationPercent, DateTime FromDate, DateTime ToDate);

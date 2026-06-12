using Application.DTOs.Dashboard;

namespace Application.Interfaces.Services;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetDashboardSummaryAsync();

    Task<ResourceDashboardDto> GetResourceDashboardAsync(int? managerId = null);

    Task<EmployeeDetailDto?> GetEmployeeDetailAsync(int employeeId);

    Task<UtilizationReportDto> GetUtilizationReportAsync();

    Task<TeamTimesheetReportDto> GetTeamTimesheetReportAsync(
        DateTime? weekStartDate);

    Task<SkillMatrixReportDto> GetSkillMatrixReportAsync();

    Task<ProjectDashboardDto> GetProjectDashboardAsync();
}
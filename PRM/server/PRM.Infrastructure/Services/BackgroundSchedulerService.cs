using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PRM.Core.Entities;
using PRM.Core.Enums;
using PRM.Infrastructure.Data;

namespace PRM.Infrastructure.Services;

public class BackgroundSchedulerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BackgroundSchedulerService> _logger;

    public BackgroundSchedulerService(IServiceProvider serviceProvider, ILogger<BackgroundSchedulerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background Scheduler started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            int intervalHours = 4; // Default
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<PrmDbContext>();
                    
                    // Fetch interval from DB if available
                    var config = await dbContext.SystemConfigs.FirstOrDefaultAsync(c => c.Key == "SchedulerInterval", stoppingToken);
                    if (config != null && int.TryParse(config.Value, out int configuredInterval) && configuredInterval > 0)
                    {
                        intervalHours = configuredInterval;
                    }

                    _logger.LogInformation("Running background tasks...");

                    await UpdateProjectHealthAsync(dbContext, stoppingToken);
                    await FlagMissedTimesheetsAsync(dbContext, stoppingToken);
                    
                    // The allocation status/bench updates dynamically when queries are made, 
                    // so we don't need to persist a status on the User entity here.
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while running background tasks.");
            }

            _logger.LogInformation($"Background tasks completed. Waiting {intervalHours} hours until next run.");
            
            try
            {
                // Wait for the configured interval
                await Task.Delay(TimeSpan.FromHours(intervalHours), stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }

    private async Task UpdateProjectHealthAsync(PrmDbContext dbContext, CancellationToken stoppingToken)
    {
        var activeProjects = await dbContext.Projects
            .Include(p => p.Milestones)
            .Include(p => p.Allocations)
            .Include(p => p.TimesheetEntries)
            .Include(p => p.Manager)
            .Where(p => p.Status == ProjectStatus.Active)
            .ToListAsync(stoppingToken);

        var emailService = _serviceProvider.CreateScope().ServiceProvider.GetService<PRM.Application.Interfaces.IEmailService>();
        var aiService = _serviceProvider.CreateScope().ServiceProvider.GetService<PRM.Application.Interfaces.IAiService>();

        foreach (var project in activeProjects)
        {
            var oldHealth = project.HealthStatus;
            var newHealth = HealthStatus.OnTrack;

            // Check for overdue milestones
            var overdueMilestones = project.Milestones.Count(m => m.DueDate < DateTime.Today && m.Status != MilestoneStatus.Done);
            
            if (overdueMilestones > 0)
            {
                newHealth = HealthStatus.AtRisk;
            }
            else
            {
                // Check if approaching deadline but not done
                var attentionMilestones = project.Milestones.Count(m => m.DueDate >= DateTime.Today && m.DueDate <= DateTime.Today.AddDays(7) && m.Status == MilestoneStatus.NotStarted);
                if (attentionMilestones > 0)
                {
                    newHealth = HealthStatus.Attention;
                }
            }

            if (oldHealth != newHealth)
            {
                project.HealthStatus = newHealth;
                
                // If it became AT_RISK, send email
                if (newHealth == HealthStatus.AtRisk && project.Manager != null && emailService != null && aiService != null)
                {
                    try
                    {
                        var aiSummary = await aiService.GetProjectRiskSummaryAsync(project.Id, project.Manager.Id);
                        
                        string suggestedHelp = "No specific resources were found on the bench to mitigate these risks.";
                        try
                        {
                            suggestedHelp = await aiService.GetSkillMatchRecommendationAsync($"Find employees on the bench whose skills could mitigate these risks: {aiSummary}", project.Manager.Id);
                        }
                        catch (Exception) { /* ignore if none found */ }

                        var milestonesHtml = "<ul style='margin-top:0;'>";
                        foreach(var m in project.Milestones.OrderBy(m => m.DueDate))
                        {
                            var mColor = m.Status == MilestoneStatus.Done ? "green" : (m.DueDate < DateTime.Today ? "red" : "black");
                            milestonesHtml += $"<li style='color:{mColor}'>{m.Title} - {m.Status} (Due: {m.DueDate:yyyy-MM-dd})</li>";
                        }
                        milestonesHtml += "</ul>";

                        var body = $@"
<div style='font-family: Arial, sans-serif; max-width: 600px;'>
    <h2 style='color: #333;'>Notification: Project At-Risk</h2>
    <p>When the Project Health Scheduler marks a project AT_RISK, this email is sent automatically.</p>
    
    <div style='background: #f8f9fa; padding: 15px; border-radius: 5px; margin-bottom: 15px; border-left: 4px solid #007bff;'>
        <h4 style='margin-top: 0; color: #555;'>1. Project details</h4>
        <strong>Name:</strong> {project.Name}<br/>
        <strong>Manager:</strong> {project.Manager.FullName}
    </div>

    <div style='background: #f8f9fa; padding: 15px; border-radius: 5px; margin-bottom: 15px; border-left: 4px solid #17a2b8;'>
        <h4 style='margin-top: 0; color: #555;'>2. Key Milestones</h4>
        {milestonesHtml}
    </div>

    <div style='background: #f8f9fa; padding: 15px; border-radius: 5px; margin-bottom: 15px; border-left: 4px solid #dc3545;'>
        <h4 style='margin-top: 0; color: #555;'>3. Health status</h4>
        Status: <span style='color: red; font-weight: bold;'>{project.HealthStatus}</span>
    </div>

    <div style='background: #f8f9fa; padding: 15px; border-radius: 5px; margin-bottom: 15px; border-left: 4px solid #ffc107;'>
        <h4 style='margin-top: 0; color: #555;'>4. AI risk summary</h4>
        <p style='margin-bottom: 0;'>{aiSummary.Replace("\n", "<br/>")}</p>
    </div>

    <div style='background: #f8f9fa; padding: 15px; border-radius: 5px; margin-bottom: 15px; border-left: 4px solid #28a745;'>
        <h4 style='margin-top: 0; color: #555;'>5. Suggested help</h4>
        <p style='margin-bottom: 0;'>{suggestedHelp.Replace("\n", "<br/>")}</p>
    </div>
</div>";
                        await emailService.SendEmailAsync(project.Manager.Email, $"[PRM Alert] Project '{project.Name}' At Risk", body);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to send AT_RISK email for project {project.Id}");
                    }
                }
            }
        }

        await dbContext.SaveChangesAsync(stoppingToken);
    }

    private async Task FlagMissedTimesheetsAsync(PrmDbContext dbContext, CancellationToken stoppingToken)
    {
        DateTime today = DateTime.Today;
        int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
        DateTime lastMonday = today.AddDays(-diff).AddDays(-7).Date;

        var emailService = _serviceProvider.CreateScope().ServiceProvider.GetService<PRM.Application.Interfaces.IEmailService>();

        var activeAllocationsForWeek = await dbContext.Allocations
            .Include(a => a.Project)
            .Where(a => a.FromDate <= lastMonday.AddDays(6) && a.ToDate >= lastMonday)
            .ToListAsync(stoppingToken);

        var activeEmployeesWithAllocations = activeAllocationsForWeek.Select(a => a.UserId).Distinct().ToList();

        foreach (var userId in activeEmployeesWithAllocations)
        {
            var user = await dbContext.Users.Include(u => u.Manager).FirstOrDefaultAsync(u => u.Id == userId, stoppingToken);
            if (user == null) continue;

            var userAllocs = activeAllocationsForWeek.Where(a => a.UserId == userId).ToList();
            var projectNames = string.Join(", ", userAllocs.Select(a => a.Project?.Name).Where(n => !string.IsNullOrEmpty(n)).Distinct());

            var timesheet = await dbContext.Timesheets
                .FirstOrDefaultAsync(t => t.UserId == userId && t.WeekStartDate.Date == lastMonday, stoppingToken);

            if (timesheet == null)
            {
                // Create missed timesheet record
                timesheet = new Timesheet
                {
                    UserId = userId,
                    WeekStartDate = lastMonday,
                    Status = TimesheetStatus.Missed,
                    ReminderCount = 0
                };
                dbContext.Timesheets.Add(timesheet);
            }
            
            // If it's missed, process reminders
            if (timesheet.Status == TimesheetStatus.Missed && !user.IsTimesheetFrozen)
            {
                if (emailService != null)
                {
                    if (timesheet.ReminderCount < 2)
                    {
                        await emailService.SendEmailAsync(user.Email, $"Timesheet Reminder [{projectNames}]: Week of {lastMonday:yyyy-MM-dd}", 
                            $"Please submit your timesheet for the week of {lastMonday:yyyy-MM-dd} covering your work on: {projectNames}. This is reminder {timesheet.ReminderCount + 1} of 2.");
                        timesheet.ReminderCount++;
                    }
                    else if (timesheet.ReminderCount == 2)
                    {
                        // Freeze user
                        user.IsTimesheetFrozen = true;
                        await emailService.SendEmailAsync(user.Email, $"Account Frozen - Missing Timesheet [{projectNames}]", 
                            $"Your account has been frozen for timesheet submission due to missing timesheet for the week of {lastMonday:yyyy-MM-dd} (Projects: {projectNames}). Please contact your manager.");
                        
                        if (user.Manager != null)
                        {
                            await emailService.SendEmailAsync(user.Manager.Email, $"Employee Account Frozen: {user.FullName}", 
                                $"{user.FullName}'s account has been frozen due to missing timesheets for projects: {projectNames}.");
                        }
                        timesheet.ReminderCount++;
                    }
                }
            }
        }

        await dbContext.SaveChangesAsync(stoppingToken);
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebAPI.Services;

public class DailyMaintenanceJob : BackgroundService
{
    private readonly ILogger<DailyMaintenanceJob> _logger;
    private readonly IServiceProvider _serviceProvider;

    public DailyMaintenanceJob(ILogger<DailyMaintenanceJob> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DailyMaintenanceJob is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("DailyMaintenanceJob is running at: {time}", DateTimeOffset.Now);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var maintenanceService = scope.ServiceProvider.GetRequiredService<IMaintenanceService>();
                    await maintenanceService.RunMaintenanceAsync();
                }

                _logger.LogInformation("DailyMaintenanceJob completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while running the daily maintenance job.");
            }

            // Wait for 3 hours before running again
            await Task.Delay(TimeSpan.FromHours(3), stoppingToken);
        }

        _logger.LogInformation("DailyMaintenanceJob is stopping.");
    }
}

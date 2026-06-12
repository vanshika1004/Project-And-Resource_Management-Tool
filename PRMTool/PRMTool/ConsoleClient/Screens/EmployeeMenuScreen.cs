using ConsoleClient.Storage;
using ConsoleClient.Screens.Employee;

namespace ConsoleClient.Screens;

public static class EmployeeMenuScreen
{
    public static async Task ShowAsync()
    {
        while (true)
        {
            ConsoleUIHelper.ClearScreen();

            var now = DateTime.Now;

            ConsoleUIHelper.DrawHeader(
                $"Welcome, {SessionManager.FullName}!",
                now.ToString("dd-MMM-yyyy  HH:mm"));

            try
            {
                var timesheets = await ApiClients.TimesheetApiClient.GetMyTimesheetsAsync();
                
                // Determine last week's end date (Friday)
                var lastFriday = now.Date;
                while (lastFriday.DayOfWeek != DayOfWeek.Friday)
                {
                    lastFriday = lastFriday.AddDays(-1);
                }
                var lastWeekStart = lastFriday.AddDays(-4);

                var missed = timesheets.Any(t => t.WeekStartDate.Date == lastWeekStart && string.Equals(t.Status, "Missed", StringComparison.OrdinalIgnoreCase));
                var notSubmitted = !timesheets.Any(t => t.WeekStartDate.Date == lastWeekStart);
                
                // If not submitted, it's missed if we're past some day, but let's just show RED WARNING if Status == Missed.
                // Or if there is a Timesheet with status Missed.
                if (missed)
                {
                    var originalColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("⚠ WARNING: You have a MISSED timesheet for last week!");
                    Console.WriteLine("Please submit your timesheet immediately.");
                    Console.ForegroundColor = originalColor;
                    Console.WriteLine();
                }
            }
            catch { /* Ignore API errors for the warning */ }

            var choice = ConsoleUIHelper.ShowMenu(new[]
            {
                "Submit Timesheet",
                "My Timesheets",
                "My Allocations",
                "Logout"
            });

            switch (choice)
            {
                case 1:
                    await SubmitTimesheetScreen.ShowAsync();
                    break;

                case 2:
                    await TimesheetHistoryScreen.ShowAsync();
                    break;

                case 3:
                    await MyAllocationsScreen.ShowAsync();
                    break;

                case 4:
                    SessionManager.ClearSession();
                    ConsoleUIHelper.ShowSuccess("Logged out.");
                    ConsoleUIHelper.PressAnyKey();
                    return;

                default:
                    ConsoleUIHelper.ShowError(
                        "Invalid option. Please try again.");
                    ConsoleUIHelper.PressAnyKey();
                    break;
            }
        }
    }
}

using ConsoleClient.ApiClients;
using ConsoleClient.Storage;

namespace ConsoleClient.Screens;

public static class ChangePasswordScreen
{
    public static async Task ShowAsync(
        int userId, string currentPassword)
    {
        while (true)
        {
            ConsoleUIHelper.ClearScreen();

            ConsoleUIHelper.DrawHeader(
                "CHANGE PASSWORD",
                "You must set a new password to continue.");

            Console.WriteLine();

            var newPassword =
                ConsoleUIHelper.PromptPassword("New Password        ");

            var confirmPassword =
                ConsoleUIHelper.PromptPassword("Confirm Password    ");

            if (newPassword != confirmPassword)
            {
                ConsoleUIHelper.ShowError(
                    "Passwords do not match. Try again.");
                ConsoleUIHelper.PressAnyKey();
                continue;
            }

            if (newPassword.Length < 8)
            {
                ConsoleUIHelper.ShowError(
                    "Password must be at least 8 characters.");
                ConsoleUIHelper.PressAnyKey();
                continue;
            }

            if (!newPassword.Any(char.IsUpper))
            {
                ConsoleUIHelper.ShowError(
                    "Password must contain at least "
                    + "one uppercase letter.");
                ConsoleUIHelper.PressAnyKey();
                continue;
            }

            if (!newPassword.Any(char.IsDigit))
            {
                ConsoleUIHelper.ShowError(
                    "Password must contain at least one number.");
                ConsoleUIHelper.PressAnyKey();
                continue;
            }

            ConsoleUIHelper.DrawSeparator();
            ConsoleUIHelper.ShowInfo("[S] Save and Continue");

            Console.Write("\nEnter option: ");
            var option = Console.ReadLine()?.Trim().ToUpper();

            if (option != "S") continue;

            try
            {
                await AuthApiClient.ChangePasswordAsync(
                    userId, currentPassword, newPassword);

                SessionManager.ClearForcePasswordChange();

                ConsoleUIHelper.ShowSuccess(
                    "Password updated. Welcome!");
                ConsoleUIHelper.PressAnyKey();
                return;
            }
            catch (Exception ex)
            {
                ConsoleUIHelper.ShowError(ex.Message);
                ConsoleUIHelper.PressAnyKey();
            }
        }
    }
}

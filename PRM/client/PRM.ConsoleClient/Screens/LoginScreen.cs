using System;
using System.Threading.Tasks;
using PRM.ConsoleClient.Models;
using PRM.ConsoleClient.Services;
using PRM.ConsoleClient.Utils;

namespace PRM.ConsoleClient.Screens;

public class LoginScreen
{
    private readonly ApiClient _api;

    public LoginScreen(ApiClient api)
    {
        _api = api;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            ConsoleUI.PrintHeader("PROJECT & RESOURCE MANAGEMENT TOOL", "Learn & Code — Final Project");
            Console.WriteLine("1. Login");
            Console.WriteLine("2. Exit");
            Console.WriteLine();
            
            string option = ConsoleUI.ReadInput("Enter option");

            if (option == "1")
            {
                await ProcessLoginAsync();
            }
            else if (option == "2")
            {
                Environment.Exit(0);
            }
            else
            {
                ConsoleUI.PrintError("Invalid option. Please try again.");
            }
        }
    }

    private async Task ProcessLoginAsync()
    {
        Console.WriteLine();
        string username = ConsoleUI.ReadInput("Username");
        string password = ConsoleUI.ReadPassword("Password");

        try
        {
            var req = new LoginRequest(username, password);
            var res = await _api.PostAsync<LoginRequest, LoginResponse>("auth/login", req);

            if (res != null)
            {
                AuthState.Token = res.Token;
                AuthState.UserId = res.UserId;
                AuthState.Username = res.Username;
                AuthState.FullName = res.FullName;
                AuthState.Role = res.Role;

                if (res.ForcePasswordChange)
                {
                    await ForcePasswordChangeAsync(password);
                }

                await RedirectToMenuAsync();
            }
        }
        catch (Exception ex)
        {
            ConsoleUI.PrintError(ex.Message);
        }
    }

    private async Task ForcePasswordChangeAsync(string oldPassword)
    {
        while (true)
        {
            ConsoleUI.PrintHeader("CHANGE PASSWORD", "You must set a new password to continue.");
            
            string newPassword = ConsoleUI.ReadPassword("New Password".PadRight(20));
            string confirmPassword = ConsoleUI.ReadPassword("Confirm Password".PadRight(20));

            Console.WriteLine("\n──────────────────────────────────────────────");
            string option = ConsoleUI.ReadInput("[S] Save and Continue");

            if (option.ToUpper() == "S")
            {
                if (newPassword != confirmPassword)
                {
                    ConsoleUI.PrintError("Passwords do not match. Try again.");
                    continue;
                }

                try
                {
                    var req = new ChangePasswordRequest(AuthState.UserId, oldPassword, newPassword);
                    await _api.PostAsync("auth/change-password", req);
                    ConsoleUI.PrintSuccess("Password updated. Welcome!");
                    break;
                }
                catch (Exception ex)
                {
                    ConsoleUI.PrintError(ex.Message);
                }
            }
        }
    }

    private async Task RedirectToMenuAsync()
    {
        if (AuthState.Role == "Admin")
        {
            await new AdminMenuScreen(_api).RunAsync();
        }
        else if (AuthState.Role == "Manager")
        {
            await new ManagerMenuScreen(_api).RunAsync();
        }
        else if (AuthState.Role == "Resource" || AuthState.Role == "Resource")
        {
            await new EmployeeMenuScreen(_api).RunAsync();
        }
        else
        {
            ConsoleUI.PrintError($"Unknown role: {AuthState.Role}");
            AuthState.Clear();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PRM.ConsoleClient.Models;
using PRM.ConsoleClient.Services;
using PRM.ConsoleClient.Utils;

namespace PRM.ConsoleClient.Screens;

public class ManageUsersScreen
{
    private readonly ApiClient _api;

    public ManageUsersScreen(ApiClient api)
    {
        _api = api;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            ConsoleUI.PrintHeader("MANAGE USERS");
            Console.WriteLine("1. Create User Account");
            Console.WriteLine("2. View All Users");
            Console.WriteLine("3. Reset User Password");
            Console.WriteLine("4. Deactivate User");
            Console.WriteLine("5. Back");
            Console.WriteLine();

            string option = ConsoleUI.ReadInput("Enter option");

            switch (option)
            {
                case "1":
                    await CreateUserAsync();
                    break;
                case "2":
                    await ViewAllUsersAsync();
                    break;
                case "3":
                    await ResetUserPasswordAsync();
                    break;
                case "4":
                    await DeactivateUserAsync();
                    break;
                case "5":
                    return;
                default:
                    ConsoleUI.PrintError("Invalid option. Please try again.");
                    break;
            }
        }
    }

    private async Task CreateUserAsync()
    {
        ConsoleUI.PrintHeader("CREATE USER ACCOUNT");
        
        string fullName = ConsoleUI.ReadInput("Full Name         ");
        string email = ConsoleUI.ReadInput("Email             ");
        string username = ConsoleUI.ReadInput("Username          ");
        string tempPassword = ConsoleUI.ReadPassword("Temporary Password");
        
        Console.WriteLine("Role              : (1) Admin  (2) Manager  (3) Resource");
        string roleChoice = ConsoleUI.ReadInput("Enter choice      ");

        int roleId = roleChoice switch
        {
            "1" => 1,
            "2" => 2,
            "3" => 3,
            _ => 3 // default to Resource
        };

        Console.WriteLine("\n──────────────────────────────────────────────");
        string confirm = ConsoleUI.ReadInput("[S] Save     [B] Back");

        if (confirm.ToUpper() == "S")
        {
            try
            {
                var req = new CreateUserRequest(username, email, fullName, roleId, null, null, tempPassword);
                await _api.PostAsync("admin/users", req);
                ConsoleUI.PrintSuccess("Account created. User must change password on first login.");
            }
            catch (Exception ex)
            {
                ConsoleUI.PrintError(ex.Message);
            }
        }
    }

    private async Task ViewAllUsersAsync()
    {
        ConsoleUI.PrintHeader("ALL USERS");

        try
        {
            var users = await _api.GetAsync<List<UserDto>>("admin/users");
            if (users != null)
            {
                Console.WriteLine($"{"ID".PadRight(5)} {"Username".PadRight(17)} {"Role".PadRight(11)} {"Status"}");
                Console.WriteLine("──────────────────────────────────────────────");
                foreach (var user in users)
                {
                    string status = user.IsActive ? "Active" : "Inactive";
                    Console.WriteLine($"{user.Id.ToString().PadRight(5)} {user.Username.PadRight(17)} {user.Role.PadRight(11)} {status}");
                }
                Console.WriteLine("──────────────────────────────────────────────");
                Console.WriteLine($"Total: {users.Count}   |   Active: {users.Count(u => u.IsActive)}   |   Inactive: {users.Count(u => !u.IsActive)}");
            }
        }
        catch (Exception ex)
        {
            ConsoleUI.PrintError(ex.Message);
            return;
        }

        Console.WriteLine("\n[R] Reactivate a user     [B] Back");
        string option = ConsoleUI.ReadInput("Enter option");

        if (option.ToUpper() == "R")
        {
            string idStr = ConsoleUI.ReadInput("Enter User ID to reactivate");
            if (int.TryParse(idStr, out int id))
            {
                try
                {
                    await _api.PutAsync($"admin/users/{id}/reactivate", new { });
                    ConsoleUI.PrintSuccess("Account reactivated.");
                }
                catch (Exception ex)
                {
                    ConsoleUI.PrintError(ex.Message);
                }
            }
        }
    }

    private async Task ResetUserPasswordAsync()
    {
        ConsoleUI.PrintHeader("RESET USER PASSWORD");
        
        string username = ConsoleUI.ReadInput("Enter Username or User ID");
        // To keep it simple, we expect ID for now, but BRD allows username. 
        // We'd need a lookup endpoint to fetch by username. I'll fetch all users and find it.
        
        try
        {
            var users = await _api.GetAsync<List<UserDto>>("admin/users");
            var user = users?.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) || u.Id.ToString() == username);

            if (user == null)
            {
                ConsoleUI.PrintError("User not found.");
                return;
            }

            Console.WriteLine($"\nUser found: {user.FullName} ({user.Role})");
            string newPassword = ConsoleUI.ReadPassword("\nNew Temporary Password");

            Console.WriteLine("\n──────────────────────────────────────────────");
            string confirm = ConsoleUI.ReadInput("[S] Save     [B] Back");

            if (confirm.ToUpper() == "S")
            {
                var req = new ResetPasswordRequest(newPassword);
                await _api.PostAsync($"admin/users/{user.Id}/reset-password", req);
                ConsoleUI.PrintSuccess("Password reset. User will be prompted to change it on next login.");
            }
        }
        catch (Exception ex)
        {
            ConsoleUI.PrintError(ex.Message);
        }
    }

    private async Task DeactivateUserAsync()
    {
        ConsoleUI.PrintHeader("DEACTIVATE USER");
        
        string username = ConsoleUI.ReadInput("Enter Username or User ID");
        
        try
        {
            var users = await _api.GetAsync<List<UserDto>>("admin/users");
            var user = users?.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) || u.Id.ToString() == username);

            if (user == null)
            {
                ConsoleUI.PrintError("User not found.");
                return;
            }

            Console.WriteLine($"\nUser found: {user.FullName} ({user.Role})");
            Console.WriteLine($"Status     : {(user.IsActive ? "Active" : "Inactive")}");
            Console.WriteLine("\nAre you sure you want to deactivate this account?");
            Console.WriteLine("Deactivated users cannot log in. Their data is preserved.");

            string confirm = ConsoleUI.ReadInput("\n[Y] Yes, Deactivate     [B] Back");

            if (confirm.ToUpper() == "Y")
            {
                // In API we have /api/admin/users/{id}/deactivate
                await _api.PutAsync($"admin/users/{user.Id}/deactivate", new { });
                ConsoleUI.PrintSuccess("User deactivated.");
            }
        }
        catch (Exception ex)
        {
            ConsoleUI.PrintError(ex.Message);
        }
    }
}

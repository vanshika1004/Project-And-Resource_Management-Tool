using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using ConsoleClient.ApiClients;

namespace ConsoleClient.Screens.Admin.User;

public static class ViewAllUsersScreen
{
    public static async Task ShowAsync()
    {
        ConsoleUIHelper.ClearScreen();
        ConsoleUIHelper.DrawHeader("ALL USERS");

        List<UserDto> users = null;
        try
        {
            users = await UserApiClient.GetAllUsersAsync();

            Console.WriteLine($"{"ID",-5} {"Username",-16} {"Role",-13} {"Status"}");
            Console.WriteLine(new string('─', 46));

            foreach (var u in users)
            {
                var status = u.IsActive ? "ACTIVE" : "INACTIVE";
                Console.WriteLine($"{u.Id,-5} {u.Username,-16} {u.Role,-13} {status}");
            }

            Console.WriteLine(new string('─', 46));
            
            var active = users.Count(u => u.IsActive);
            var inactive = users.Count(u => !u.IsActive);
            Console.WriteLine($"Total: {users.Count}   |   Active: {active}   |   Inactive: {inactive}");
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            ConsoleUIHelper.ShowError(ex.Message);
        }

        Console.WriteLine("[R] Reactivate a user     [B] Back");
        Console.WriteLine();
        while (true)
        {
            var opt = ConsoleUIHelper.Prompt("Enter option").ToUpper();
            if (opt == "B") return;
            if (opt == "R")
            {
                var idStr = ConsoleUIHelper.Prompt("Enter User ID to reactivate");
                if (int.TryParse(idStr, out var id))
                {
                    var userToReactivate = users?.FirstOrDefault(u => u.Id == id);
                    if (userToReactivate == null)
                    {
                        ConsoleUIHelper.ShowError("User not found.");
                        continue;
                    }
                    if (userToReactivate.IsActive)
                    {
                        ConsoleUIHelper.ShowInfo("User is already active.");
                        continue;
                    }

                    Console.WriteLine();
                    Console.WriteLine($"User: {userToReactivate.Username} ({userToReactivate.Role.ToUpper()}) — currently Inactive");
                    Console.WriteLine();
                    Console.WriteLine("Reactivate this account?");
                    Console.WriteLine("[Y] Yes     [B] Cancel");
                    var confirm = ConsoleUIHelper.Prompt("").ToUpper();
                    if (confirm == "Y")
                    {
                        try
                        {
                            await UserApiClient.ReactivateUserAsync(id);
                            ConsoleUIHelper.ShowSuccess($"Account reactivated. {userToReactivate.Username} can now log in. ✓");
                            Console.WriteLine("Note: Previous allocations are NOT restored. Admin must re-allocate manually if needed.");
                            ConsoleUIHelper.PressAnyKey();
                            return; // Return to reload the screen
                        }
                        catch (Exception ex)
                        {
                            ConsoleUIHelper.ShowError(ex.Message);
                        }
                    }
                }
            }
        }
    }
}

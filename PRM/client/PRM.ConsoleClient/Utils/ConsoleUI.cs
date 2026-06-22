using System;

namespace PRM.ConsoleClient.Utils;

public static class ConsoleUI
{
    public static void PrintHeader(string title, string subtitle = "")
    {
        Console.Clear();
        int contentWidth = Math.Max(42, Math.Max(title.Length, subtitle.Length));
        string border = new string('═', contentWidth + 4);
        
        Console.WriteLine($"╔{border}╗");
        Console.WriteLine($"║    {title.PadRight(contentWidth)}║");
        if (!string.IsNullOrEmpty(subtitle))
        {
            Console.WriteLine($"║    {subtitle.PadRight(contentWidth)}║");
        }
        Console.WriteLine($"╚{border}╝");
        Console.WriteLine();
    }

    public static void PrintError(string message)
    {
        var prev = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n[Error] {message}");
        Console.ForegroundColor = prev;
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(true);
    }

    public static void PrintSuccess(string message)
    {
        var prev = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n{message} ✓");
        Console.ForegroundColor = prev;
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(true);
    }

    public static string ReadInput(string prompt)
    {
        Console.Write($"{prompt}: ");
        return Console.ReadLine()?.Trim() ?? string.Empty;
    }

    public static string ReadPassword(string prompt)
    {
        Console.Write($"{prompt}: ");
        string pass = string.Empty;
        ConsoleKeyInfo key;

        do
        {
            key = Console.ReadKey(true);

            // Backspace Should Not Work
            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
            {
                pass += key.KeyChar;
                Console.Write("*");
            }
            else
            {
                if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    pass = pass.Substring(0, (pass.Length - 1));
                    Console.Write("\b \b");
                }
            }
        }
        // Stops Receving Keys Once Enter is Pressed
        while (key.Key != ConsoleKey.Enter);

        Console.WriteLine();
        return pass;
    }
}

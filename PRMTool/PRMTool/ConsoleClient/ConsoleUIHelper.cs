namespace ConsoleClient;

public static class ConsoleUIHelper
{
    private const int BoxWidth = 46;

    public static void DrawHeader(string title, string? subtitle = null)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔" + new string('═', BoxWidth) + "╗");
        Console.WriteLine("║    " + title.PadRight(BoxWidth - 4) + "║");
        if (subtitle != null)
        {
            Console.WriteLine("║    " + subtitle.PadRight(BoxWidth - 4) + "║");
        }
        Console.WriteLine("╚" + new string('═', BoxWidth) + "╝");
        Console.WriteLine();
        Console.ResetColor();
    }

    public static void DrawSeparator()
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(new string('─', BoxWidth + 2));
        Console.ResetColor();
    }

    public static void ShowSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n{message} ✓");
        Console.ResetColor();
    }

    public static void ShowError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n✗ {message}");
        Console.ResetColor();
    }

    public static void ShowWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n⚠  {message}");
        Console.ResetColor();
    }

    public static void ShowInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public static string Prompt(string label)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"{label}: ");
        Console.ResetColor();
        return Console.ReadLine()?.Trim() ?? string.Empty;
    }

    public static string PromptPassword(string label)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"{label}: ");
        Console.ResetColor();

        var password = string.Empty;
        ConsoleKeyInfo key;

        while (true)
        {
            key = Console.ReadKey(intercept: true);

            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                break;
            }
            else if (key.Key == ConsoleKey.Backspace
                     && password.Length > 0)
            {
                password = password[..^1];
                Console.Write("\b \b");
            }
            else if (!char.IsControl(key.KeyChar))
            {
                password += key.KeyChar;
                Console.Write("*");
            }
        }

        return password;
    }

    public static int ShowMenu(string[] options)
    {
        Console.WriteLine();
        for (int i = 0; i < options.Length; i++)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{i + 1}. ");
            Console.ResetColor();
            Console.WriteLine(options[i]);
        }

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("Enter option: ");
        Console.ResetColor();

        if (int.TryParse(Console.ReadLine()?.Trim(), out int choice))
        {
            return choice;
        }

        return -1;
    }

    public static void ClearScreen()
    {
        Console.Clear();
    }

    public static void PressAnyKey()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("Press any key to continue...");
        Console.ResetColor();
        Console.ReadKey(intercept: true);
        Console.WriteLine();
    }

    private static string CenterText(string text, int width)
    {
        if (text.Length >= width)
            return text[..width];

        int leftPad = (width - text.Length) / 2;
        int rightPad = width - text.Length - leftPad;

        return new string(' ', leftPad) + text
             + new string(' ', rightPad);
    }
}

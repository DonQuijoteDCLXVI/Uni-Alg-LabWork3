namespace PracticalWork3.Infrastructure;

public static class ConsoleIO
{
    /// <summary>Читает строку; при пустом вводе или конце потока возвращает значение по умолчанию.</summary>
    public static string Prompt(string message, string defaultValue)
    {
        Console.Write($"{message} [{defaultValue}]: ");
        var line = Console.ReadLine();
        return string.IsNullOrWhiteSpace(line) ? defaultValue : line.Trim();
    }

    public static void Header(string title)
    {
        Console.WriteLine();
        Console.WriteLine(new string('=', 70));
        Console.WriteLine(title);
        Console.WriteLine(new string('=', 70));
    }

    public static void Error(string message)
    {
        var old = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Ошибка: " + message);
        Console.ForegroundColor = old;
    }
}

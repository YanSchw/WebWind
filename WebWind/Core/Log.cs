namespace WebWind.Core;

public class Log
{
    public static void Info(string message)
    {
        WriteLog("INFO", message, ConsoleColor.Green);
    }

    public static void Warn(string message)
    {
        WriteLog("WARN", message, ConsoleColor.Yellow);
    }

    public static void Error(string message)
    {
        WriteLog("ERROR", message, ConsoleColor.Red);
    }

    public static void Fatal(string message)
    {
        WriteLog("FATAL", message, ConsoleColor.DarkRed);
    }

    public static void Debug(string message)
    {
        WriteLog("DEBUG", message, ConsoleColor.Cyan);
    }

    public static void Trace(string message)
    {
        WriteLog("TRACE", message, ConsoleColor.Gray);
    }

    // Generic log method with color support
    private static void WriteLog(string level, string message, ConsoleColor color)
    {
        var originalColor = Console.ForegroundColor;
        
        Console.ForegroundColor = color;

        Console.WriteLine($"{DateTime.Now} [{level}] {message}");

        Console.ForegroundColor = originalColor;
    }
}
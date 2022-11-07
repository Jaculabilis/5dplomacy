namespace MultiversalDiplomacy.Adjudicate.Logging;

public class ConsoleLogger : IAdjudicatorLogger
{
    public static ConsoleLogger Instance { get; } = new();

    public void Log(int contextLevel, string message, params object[] args)
    {
        string spacing = string.Format($"{{0,{2 * contextLevel}}}", string.Empty);
        string formattedMessage = string.Format(message, args);
        Console.WriteLine(spacing + formattedMessage);
    }
}
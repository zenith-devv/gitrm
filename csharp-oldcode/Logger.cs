public static class Logger
{
    public enum MessageType
    {
        Default,
        Done,
        Info,
        Warn,
        Err,
        Debug
    }

    public static void Log(MessageType type, string message)
    {
        switch (type)
        {
            case MessageType.Default:
                PrintLabel("*", ConsoleColor.Blue);
                break;
            case MessageType.Done:
                PrintLabel("+", ConsoleColor.Green);
                break;
            case MessageType.Info:
                PrintLabel("i", ConsoleColor.White);
                break;
            case MessageType.Warn:
                PrintLabel("!", ConsoleColor.Yellow);
                break;
            case MessageType.Err:
                PrintLabel("!", ConsoleColor.Red);
                break;
            case MessageType.Debug:
                PrintLabel("x", ConsoleColor.Gray);
                break;
        }
        Console.Write(message);
    }

    public static void PrintLabel(string mark, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write("["+mark+"]");
        Console.ResetColor();
        Console.Write(" ");
    }
}
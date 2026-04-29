using static Logger;
using static Logger.MessageType;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            DisplayHelp();
            return;
        }

        string command = args[0].ToLower();

        switch (command)
        {
            case "build":
                BuildAssistant.Build();
                break;
            case "fetch":
                if (args.Length < 2) 
                {
                    Log(Err, "repo url not specified. usage: bob fetch <url>\n");
                    break; 
                }
                string url = args[1];
                BuildAssistant.Fetch(url);
                break;
            case "config":
                JsonHandler.CreateTemplate();
                break;
            case "version":
                DisplayVersion();
                break;
            default:
                Log(Err, $"unknown command: {command}\n");
                break;
        }
    }

    private static void DisplayHelp()
    {
        Console.WriteLine("usage: bob [command]");
        Console.WriteLine("available commands:");
        Console.WriteLine("build   : read bob-config.json and compile project to executable");
        Console.WriteLine("fetch   : clone a repository and automatically build the project (if bobconfig.json exists)");
        Console.WriteLine("config  : create an empty bobconfig.json template");
        Console.WriteLine("version : display bob version");
    }

    private static void DisplayVersion()
    {
        Console.WriteLine("bob (build orchestrator binary) version 0.5.2-alpha");
        Console.WriteLine("copyright (c) 2026 Michael Zenith");
        Console.WriteLine("licensed under MIT license");
    }
}
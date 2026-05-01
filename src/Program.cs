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
            case "clone":
                if (args.Length < 2) 
                {
                    Log(Err, "Repo url not specified. usage: gitrm clone <url>\n");
                    break; 
                }
                string url = args[1];
                BuildAssistant.Fetch(url);
                break;
            case "config":
                ConfigParser.CreateTemplate();
                break;
            case "version":
                DisplayVersion();
                break;
            default:
                Log(Err, $"Unknown command: {command}\n");
                break;
        }
    }

    private static void DisplayHelp()
    {
        Console.WriteLine("Usage: gitrm [command]");
        Console.WriteLine("Available commands:");
        Console.WriteLine("build   : Read gitrm.yaml and compile project to executable");
        Console.WriteLine("clone   : Clone a repository and automatically build the project (if gitrm.yaml exists)");
        Console.WriteLine("config  : Create an empty gitrm.yaml template");
        Console.WriteLine("version : Display gitrm version");
    }

    private static void DisplayVersion()
    {
        Console.WriteLine("gitrm (git repo manager) version 0.6-beta");
        Console.WriteLine("Copyright (c) 2026 Michael Zenith");
        Console.WriteLine("Licensed under MIT license");
    }
}
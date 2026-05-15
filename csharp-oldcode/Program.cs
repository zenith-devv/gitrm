using static Logger;
using static Logger.MessageType;

public class Program
{
    public static async Task Main(string[] args)
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
                    Log(Err, "url not specified. Usage: gitrm clone [-k] <url>\n");
                    break;
                }
                bool keepSource = args.Contains("-k");
                string cloneUrl = args.First(a => a != "clone" && a != "-k");
                BuildAssistant.Fetch(cloneUrl, keepSource);
                break;
            case "check":
                await CheckCommand.Run();
                break;
            case "config":
                ConfigParser.CreateTemplate();
                break;
            case "list":
                var pkgs = PackageDatabase.All().ToList();
                if (pkgs.Count == 0) { Log(Info, "No packages installed.\n"); break; }
                foreach (var p in pkgs)
                    Log(Done, $"{p.Name,-20} {p.Version,-10} {p.Source}\n");
                break;
            case "remove":
                if (args.Length < 2) { Log(Err, "Package name not specified.\n"); break; }
                var pkg = PackageDatabase.Get(args[1]);
                if (pkg == null) { Log(Err, $"Package '{args[1]}' not found in database.\n"); break; }
                foreach (var bin in pkg.Binaries)
                {
                    if (File.Exists(bin)) { File.Delete(bin); Log(Done, $"Removed {bin}\n"); }
                }
                PackageDatabase.Remove(args[1]);
                Log(Done, $"Package '{args[1]}' removed.\n");
                break;
            case "update":
                string? updateTarget = args.Length >= 2 ? args[1] : null;
                await BuildAssistant.Update(updateTarget);
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
        Console.WriteLine("check   : Scan installed packages and check for updates");
        Console.WriteLine("clone   : Clone a repository and automatically build the project (if gitrm.yaml exists)");
        Console.WriteLine("clone -k: Same as clone but keeps the source after build");
        Console.WriteLine("config  : Create an empty gitrm.yaml template");
        Console.WriteLine("list    : List all installed packages");
        Console.WriteLine("remove  : Remove an installed package");
        Console.WriteLine("update  : Update all installed packages (or specify a name)");
        Console.WriteLine("version : Display gitrm version");
    }

    private static void DisplayVersion()
    {
        Console.WriteLine("gitrm (git repo manager) version 0.7-beta");
        Console.WriteLine("Copyright (c) 2026 Michael Zenith");
        Console.WriteLine("Licensed under MIT license");
    }
}
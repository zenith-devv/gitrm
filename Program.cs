using static JsonHandler;
using static ToolChecker;
using static Logger;
using static Logger.MessageType;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: bob [command]");
            Console.WriteLine();
            Console.WriteLine("Available commands:");
            Console.WriteLine("build - read bobconfig.json and compile project to executable");
            Console.WriteLine("check - check installed compilers and build systems");
            Console.WriteLine("git-clone - clone a repository and automatically build the project (if bobconfig.json exists)");
            Console.WriteLine("make-json - create an empty bobconfig.json template");
            return;
        }

        string command = args[0].ToLower();

        switch (command)
        {
            case "build":
                Log(Default, "to do\n");
                break;
            case "check":
                Check();
                break;
            case "git-clone":
                Log(Default, "to do\n");
                break;
            case "make-json":
                CreateTemplate();
                break;
            default:
                Log(Err, $"unknown command: {command}\n");
                break;
        }
    }
}
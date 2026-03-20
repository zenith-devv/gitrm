using static CommandRunner;
using static Logger;
using static Logger.MessageType;

public static class Checker
{
    public static void Check()
    {
        string[] tools = ["g++", "dotnet", "cargo", "go", "cmake", "meson", "make"];
        var installed = new List<string>();
        var missing = new List<string>();

        Log(Default, "checking compilers and build systems...\n");
        foreach (string tool in tools)
        {
            int result = RunQuiet("which", tool);
            if (result == 0) 
                installed.Add(tool);
            else
                missing.Add(tool); 
        }

        if (installed.Count>0)
        {
            Log(Default, "installed: ");
            foreach (string item in installed)
                Console.Write($"{item} ");

            Console.WriteLine();
        }

        if (missing.Count>0)
        {
            Log(Default, "missing: ");
            foreach (string item in missing)
                Console.Write($"{item} ");

            Console.WriteLine();
        }
        
    }
}
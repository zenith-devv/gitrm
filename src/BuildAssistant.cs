using static Logger;
using static Logger.MessageType;

public class ProjectConfig
{
    public string CompilerFlags { get; set; } = "";
    public string MainFile { get; set; } = "";
    public string OutputFile { get; set; } = "";
}

public static class BuildAssistant
{
    public static void Build()
    {
        var config = JsonHandler.LoadConfig();
        if (config == null || string.IsNullOrEmpty(config.MainFile)) return;
        string compiler = DetectCompiler(config.MainFile);

        if (compiler == "dotnet")
        {
            Log(Default, "csharp project detected\n");
            var projectFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.csproj");
            if (projectFiles.Length == 0)
            {
                Log(Err, ".csproj file missing\n");
                return;
            }
            string targetProject = projectFiles[0];
            string outPath = string.IsNullOrWhiteSpace(config.OutputFile) ? "bob-out" : config.OutputFile;
            string cmd = "dotnet";
            string args = $"publish {targetProject} {config.CompilerFlags} -o {outPath}";
            Log(Default, $"running '{cmd} {args}'\n");
            int result = CommandRunner.Run(cmd, args);

            if (result == 0)
                Log(Success, $"build finished successfully. output located in {outPath}\n");
            else
                Log(Err, "project build failed. make sure if bob-config.json is correct.\n");
                
            return;
        }

        // todo: cpp, c, rust and go
        else
            Log(Err, "invalid or unsupported file type\n");
    }

    private static string DetectCompiler(string mainFile)
    {
        string extension = Path.GetExtension(mainFile).ToLower();

        return extension switch
        {
            ".cpp" or ".cc" or ".cxx" => "g++",
            ".c" => "gcc",
            ".cs" => "dotnet",
            ".rs" => "cargo",
            ".go" => "go",
            _ => ""
        };
    }
}
using static Logger;
using static Logger.MessageType;

public class CsBuilder : IBuilder
{
    public string Name => "C# (.NET)";
    public bool CanHandle(string ext) => ext is ".cs" or ".csproj" or ".sln";

    public void Build(ProjectConfig config)
    {
        Log(Default, $"{Name} project detected\n");
        if (CommandRunner.RunQuiet("dotnet", "--version") != 0)
        {
            Log(Err, "dotnet is not installed or not in PATH, unable to build\n");
            return;
        }
        
        string targetProject = config.Build.MainFile;
        string extension = Path.GetExtension(targetProject).ToLower();

        if (extension == ".cs")
             Log(Warn, "Single file build. For better performance and dependencies use .csproj file\n");

        string outPath = string.IsNullOrWhiteSpace(config.Build.OutputPath) ? "publish" : config.Build.OutputPath;
        string cmd = "dotnet";
        string args = $"publish {targetProject} {config.Build.Flags} -o {outPath}".Trim();
        Log(Default, $"Running \"{cmd} {args}\"\n");
        int result = CommandRunner.Run(cmd, args);

        if (result == 0)
            Log(Done, $"Build finished successfully. Output located in {outPath}\n");
        else
            Log(Err, $"Project build failed. (exit code {result})\n");
    }
}
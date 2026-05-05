using static Logger;
using static Logger.MessageType;

public class GoBuilder : IBuilder
{
    public string Name => "Go";
    public bool CanHandle(string ext) => ext is ".go" or ".mod";

    public void Build(ProjectConfig config)
    {
        Log(Default, $"{Name} project detected\n");

        if (CommandRunner.RunQuiet("go", "version") != 0)
        {
            Log(Err, "go is not installed or not in PATH, unable to build\n");
            return;
        }

        string ext = Path.GetExtension(config.Build.MainFile).ToLower();
        string cmd = "go";
        string args;
        string outPath;

        if (ext == ".mod")
        {
            // go.mod — build the whole module
            string outputFlag = string.IsNullOrWhiteSpace(config.Build.OutputPath)
                ? ""
                : $"-o {config.Build.OutputPath}";
            args = $"build {outputFlag} {config.Build.Flags} ./...".Trim();
            outPath = string.IsNullOrWhiteSpace(config.Build.OutputPath) ? "." : config.Build.OutputPath;
        }
        else
        {
            // single .go file
            string outFile = string.IsNullOrWhiteSpace(config.Build.OutputPath) ? "app.out" : config.Build.OutputPath;
            args = $"build -o {outFile} {config.Build.Flags} {config.Build.MainFile}".Trim();
            outPath = outFile;
            Log(Warn, "Single file build. For better dependency management use go.mod\n");
        }

        Log(Default, $"Running \"{cmd} {args}\"\n");
        int result = CommandRunner.Run(cmd, args);

        if (result == 0)
            Log(Done, $"Build finished successfully. Output located in {outPath}\n");
        else
            Log(Err, $"Project build failed. (exit code {result})\n");
    }
}
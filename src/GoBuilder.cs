using static Logger;
using static Logger.MessageType;

public class GoBuilder : IBuilder
{
    public string Name => "go";
    public bool CanHandle(string ext) => ext is ".go" or ".mod";

    public void Build(ProjectConfig config)
    {
        Log(Default, $"{Name} project detected\n");

        if (CommandRunner.RunQuiet("go", "version") != 0)
        {
            Log(Err, "go is not installed or not in PATH, unable to build\n");
            return;
        }
        Log(Default, "go is present\n");

        string ext = Path.GetExtension(config.MainFile).ToLower();
        string cmd = "go";
        string args;
        string outPath;

        if (ext == ".mod")
        {
            // go.mod — build the whole module
            string outputFlag = string.IsNullOrWhiteSpace(config.OutputFile)
                ? ""
                : $"-o {config.OutputFile}";
            args = $"build {outputFlag} {config.CompilerFlags} ./...".Trim();
            outPath = string.IsNullOrWhiteSpace(config.OutputFile) ? "." : config.OutputFile;
        }
        else
        {
            // single .go file
            string outFile = string.IsNullOrWhiteSpace(config.OutputFile) ? "app.out" : config.OutputFile;
            args = $"build -o {outFile} {config.CompilerFlags} {config.MainFile}".Trim();
            outPath = outFile;
            Log(Warn, "single file build. for better dependency management use go.mod\n");
        }

        Log(Default, $"running \"{cmd} {args}\"\n");
        int result = CommandRunner.Run(cmd, args);

        if (result == 0)
            Log(Done, $"build finished successfully. output located in {outPath}\n");
        else
            Log(Err, $"project build failed. (exit code {result})\n");
    }
}
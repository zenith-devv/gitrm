using static Logger;
using static Logger.MessageType;

public class GccBuilder : IBuilder
{
    public string Name => "c/c++ (gcc/g++)";
    public bool CanHandle(string ext) => ext is ".c" or ".cpp" or ".cc";

    public void Build(ProjectConfig config)
    {
        Log(Default, $"{Name} project detected\n");

        string ext = Path.GetExtension(config.Build.MainFile).ToLower();
        bool isCpp = ext is ".cpp" or ".cc";
        string compiler = isCpp ? "g++" : "gcc";

        if (CommandRunner.RunQuiet(compiler, "--version") != 0)
        {
            Log(Err, $"{compiler} is not installed or not in PATH, unable to build\n");
            return;
        }

        string outFile = string.IsNullOrWhiteSpace(config.Build.OutputPath) ? "a.out" : config.Build.OutputPath;
        string args = $"{config.Build.MainFile} -o {outFile} {config.Build.Flags}".Trim();

        Log(Default, $"Running \"{compiler} {args}\"\n");
        int result = CommandRunner.Run(compiler, args);

        if (result == 0)
            Log(Done, $"Build finished successfully. Output located in {outFile}\n");
        else
            Log(Err, $"Project build failed. (exit code {result})\n");
    }
}
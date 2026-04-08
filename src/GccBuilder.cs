using static Logger;
using static Logger.MessageType;

public class GccBuilder : IBuilder
{
    public string Name => "c/c++ (gcc/g++)";
    public bool CanHandle(string ext) => ext is ".c" or ".cpp" or ".cc";

    public void Build(ProjectConfig config)
    {
        Log(Default, $"{Name} project detected\n");

        string ext = Path.GetExtension(config.MainFile).ToLower();
        bool isCpp = ext is ".cpp" or ".cc";
        string compiler = isCpp ? "g++" : "gcc";

        if (CommandRunner.RunQuiet(compiler, "--version") != 0)
        {
            Log(Err, $"{compiler} is not installed or not in PATH, unable to build\n");
            return;
        }
        Log(Default, $"{compiler} is present\n");

        string outFile = string.IsNullOrWhiteSpace(config.OutputFile) ? "a.out" : config.OutputFile;
        string args = $"{config.MainFile} -o {outFile} {config.CompilerFlags}".Trim();

        Log(Default, $"running \"{compiler} {args}\"\n");
        int result = CommandRunner.Run(compiler, args);

        if (result == 0)
            Log(Done, $"build finished successfully. output located in {outFile}\n");
        else
            Log(Err, $"project build failed. (exit code {result})\n");
    }
}
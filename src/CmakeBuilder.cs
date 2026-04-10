using static Logger;
using static Logger.MessageType;

public class CmakeBuilder : IBuilder
{
    public string Name => "cmake";
    public bool CanHandle(string ext) => false;
    public bool Detect(string directory) => File.Exists(Path.Combine(directory, "CMakeLists.txt"));

    public void Build(ProjectConfig config)
    {
        Log(Default, $"{Name} build system detected\n");

        if (CommandRunner.RunQuiet("cmake", "--version") != 0)
        {
            Log(Err, "cmake is not installed or not in PATH, unable to build\n");
            return;
        }
        Log(Default, "cmake is present\n");

        string buildDir = string.IsNullOrWhiteSpace(config.OutputFile) ? "build" : config.OutputFile;

        Log(Default, $"running \"cmake -S . -B {buildDir} {config.CompilerFlags}\"\n");
        int setupRes = CommandRunner.Run("cmake", $"-S . -B {buildDir} {config.CompilerFlags}");
        if (setupRes != 0)
        {
            Log(Err, $"project setup failed. (exit code {setupRes})\n");
            return;
        }

        Log(Default, $"running \"cmake --build {buildDir}\"\n");
        int compileRes = CommandRunner.Run("cmake", $"--build {buildDir}");
        if (compileRes == 0)
            Log(Done, $"build finished successfully. output located in {buildDir}\n");
        else
            Log(Err, $"project build failed. (exit code {compileRes})\n");
    }
}
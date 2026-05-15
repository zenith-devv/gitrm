using static Logger;
using static Logger.MessageType;

public class CmakeBuilder : IBuilder
{
    public string Name => "CMake";
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

        string buildDir = string.IsNullOrWhiteSpace(config.Build.OutputPath) ? "build" : config.Build.OutputPath;

        Log(Default, $"Running \"cmake -S . -B {buildDir} {config.Build.Flags}\"\n");
        int setupRes = CommandRunner.Run("cmake", $"-S . -B {buildDir} {config.Build.Flags}");
        if (setupRes != 0)
        {
            Log(Err, $"Project setup failed. (exit code {setupRes})\n");
            return;
        }

        Log(Default, $"Running \"cmake --build {buildDir}\"\n");
        int compileRes = CommandRunner.Run("cmake", $"--build {buildDir}");
        if (compileRes == 0)
            Log(Done, $"Build finished successfully. Output located in {buildDir}\n");
        else
            Log(Err, $"Project build failed. (exit code {compileRes})\n");
    }
}
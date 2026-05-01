using static Logger;
using static Logger.MessageType;

public class MesonBuilder : IBuilder
{
    public string Name => "meson";
    public bool CanHandle(string ext) => false;
    public bool Detect(string directory) => File.Exists(Path.Combine(directory, "meson.build"));

    public void Build(ProjectConfig config)
    {
        Log(Default, $"{Name} build system detected\n");

        if (CommandRunner.RunQuiet("meson", "--version") != 0)
        {
            Log(Err, "meson is not installed or not in PATH, unable to build\n");
            return;
        }

        string buildDir = string.IsNullOrWhiteSpace(config.Build.OutputPath) ? "build" : config.Build.OutputPath;

        Log(Default, $"Running \"meson setup {config.Build.Flags} {buildDir}\"\n");
        int setupRes = CommandRunner.Run("meson", $"setup {config.Build.Flags} {buildDir}");
        if (setupRes != 0)
        {
            Log(Err, $"Project setup failed. (exit code {setupRes})\n");
            return;
        }

        Log(Default, $"Running \"meson compile -C {buildDir}\"\n");
        int compileRes = CommandRunner.Run("meson", $"compile -C {buildDir}");
        if (compileRes == 0)
            Log(Done, $"Build finished successfully. Output located in {buildDir}\n");
        else
            Log(Err, $"Project build failed. (exit code {compileRes})\n");
    }
}
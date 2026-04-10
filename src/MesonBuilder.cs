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
        Log(Default, "meson is present\n");

        string buildDir = string.IsNullOrWhiteSpace(config.OutputFile) ? "build" : config.OutputFile;

        Log(Default, $"running \"meson setup {config.CompilerFlags} {buildDir}\"\n");
        int setupRes = CommandRunner.Run("meson", $"setup {config.CompilerFlags} {buildDir}");
        if (setupRes != 0)
        {
            Log(Err, $"project setup failed. (exit code {setupRes})\n");
            return;
        }

        Log(Default, $"running \"meson compile -C {buildDir}\"\n");
        int compileRes = CommandRunner.Run("meson", $"compile -C {buildDir}");
        if (compileRes == 0)
            Log(Done, $"build finished successfully. output located in {buildDir}\n");
        else
            Log(Err, $"project build failed. (exit code {compileRes})\n");
    }
}
using static Logger;
using static Logger.MessageType;

public class RustBuilder : IBuilder
{
    public string Name => "Rust (Cargo)";
    public bool CanHandle(string extension) => extension is ".rs" or ".toml";

    public void Build(ProjectConfig config)
    {
        Log(Default, $"{Name} project detected\n");
        if (CommandRunner.RunQuiet("cargo", "--version") != 0)
        {
            Log(Err, "cargo is not installed or not in PATH, unable to build\n");
            return;
        }
        
        string targetProject = config.Build.MainFile;
        string extension = Path.GetExtension(targetProject).ToLower();
        string cmd;
        string args;
        string outPath;

        if (extension == ".toml")
        {
            cmd = "cargo";
            string outputFlag = string.IsNullOrWhiteSpace(config.Build.OutputPath) 
                ? "" 
                : $"--target-dir {config.Build.OutputPath}";
            args = $"build {outputFlag} {config.Build.Flags}".Trim();
            
            if (!string.IsNullOrWhiteSpace(config.Build.OutputPath))
                outPath = config.Build.OutputPath;
            else
            {
                bool isRelease = config.Build.Flags.Contains("-r") || config.Build.Flags.Contains("--release");
                outPath = isRelease ? "target/release" : "target/debug";
            }
        }
        else
        {
            cmd = "rustc";
            string binaryName = string.IsNullOrWhiteSpace(config.Build.OutputPath) ? "app.out" : config.Build.OutputPath;
            args = $"{targetProject} -o {binaryName} {config.Build.Flags}";
            outPath = binaryName;
            Log(Warn, "Single file build. For better performance and dependencies use Cargo.toml\n");
        }

        Log(Default, $"Running \"{cmd} {args}\"\n");
        int result = CommandRunner.Run(cmd, args);

        if (result == 0)
            Log(Done, $"Build finished successfully. Output located in {outPath}\n");
        else
            Log(Err, $"Project build failed. (exit code {result})\n");
    }
}
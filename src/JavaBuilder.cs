using static Logger;
using static Logger.MessageType;

public class JavaBuilder : IBuilder
{
    public string Name => "java";
    public bool CanHandle(string ext) => ext is ".java" or ".xml"; // .xml covers pom.xml

    public void Build(ProjectConfig config)
    {
        // .xml could be anything — only handle pom.xml
        if (Path.GetExtension(config.Build.MainFile).ToLower() == ".xml" &&
            !config.Build.MainFile.Equals("pom.xml", StringComparison.OrdinalIgnoreCase))
        {
            Log(Err, "Only pom.xml is supported as an .xml project file\n");
            return;
        }

        Log(Default, $"{Name} project detected\n");

        string ext = Path.GetExtension(config.Build.MainFile).ToLower();
        bool isMaven = ext == ".xml";

        if (isMaven)
            BuildMaven(config);
        else
            BuildSingle(config);
    }

    private void BuildMaven(ProjectConfig config)
    {
        if (CommandRunner.RunQuiet("mvn", "--version") != 0)
        {
            Log(Err, "mvn is not installed or not in PATH, unable to build\n");
            return;
        }

        // mvn package puts the jar in target/ by default; OutputFile is ignored (Maven controls it)
        string args = $"package {config.Build.Flags} -f {config.Build.MainFile}".Trim();
        Log(Default, $"Running \"mvn {args}\"\n");

        if (!string.IsNullOrWhiteSpace(config.Build.OutputPath))
            Log(Warn, "OutputFile is ignored for Maven builds — output is placed in target/ by Maven\n");

        int result = CommandRunner.Run("mvn", args);

        if (result == 0)
            Log(Done, "Build finished successfully. Output located in target/\n");
        else
            Log(Err, $"Project build failed. (exit code {result})\n");
    }

    private void BuildSingle(ProjectConfig config)
    {
        if (CommandRunner.RunQuiet("javac", "--version") != 0)
        {
            Log(Err, "javac is not installed or not in PATH, unable to build\n");
            return;
        }
        Log(Warn, "Single file build. For better performance and dependencies use pom.xml (mvn)\n");

        string outDir = string.IsNullOrWhiteSpace(config.Build.OutputPath) ? "." : config.Build.OutputPath;
        string args = $"{config.Build.MainFile} -d {outDir} {config.Build.Flags}".Trim();

        Log(Default, $"Running \"javac {args}\"\n");
        int result = CommandRunner.Run("javac", args);

        if (result == 0)
            Log(Done, $"Build finished successfully. .class files located in {outDir}\n");
        else
            Log(Err, $"Project build failed. (exit code {result})\n");
    }
}
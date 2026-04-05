using static Logger;
using static Logger.MessageType;

public class JavaBuilder : IBuilder
{
    public string Name => "java";
    public bool CanHandle(string ext) => ext is ".java" or ".xml"; // .xml covers pom.xml

    public void Build(ProjectConfig config)
    {
        // .xml could be anything — only handle pom.xml
        if (Path.GetExtension(config.MainFile).ToLower() == ".xml" &&
            !config.MainFile.Equals("pom.xml", StringComparison.OrdinalIgnoreCase))
        {
            Log(Err, "only pom.xml is supported as an .xml project file\n");
            return;
        }

        Log(Default, $"{Name} project detected\n");

        string ext = Path.GetExtension(config.MainFile).ToLower();
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
        Log(Default, "mvn is present\n");

        // mvn package puts the jar in target/ by default; OutputFile is ignored (Maven controls it)
        string args = $"package {config.CompilerFlags} -f {config.MainFile}".Trim();
        Log(Default, $"running \"mvn {args}\"\n");

        if (!string.IsNullOrWhiteSpace(config.OutputFile))
            Log(Warn, "OutputFile is ignored for Maven builds — output is placed in target/ by Maven\n");

        int result = CommandRunner.Run("mvn", args);

        if (result == 0)
            Log(Done, "build finished successfully. output located in target/\n");
        else
            Log(Err, $"project build failed. (exit code {result})\n");
    }

    private void BuildSingle(ProjectConfig config)
    {
        if (CommandRunner.RunQuiet("javac", "--version") != 0)
        {
            Log(Err, "javac is not installed or not in PATH, unable to build\n");
            return;
        }
        Log(Default, "javac is present\n");
        Log(Warn, "single file build. for better performance and dependencies use pom.xml (Maven)\n");

        string outDir = string.IsNullOrWhiteSpace(config.OutputFile) ? "." : config.OutputFile;
        string args = $"{config.MainFile} -d {outDir} {config.CompilerFlags}".Trim();

        Log(Default, $"running \"javac {args}\"\n");
        int result = CommandRunner.Run("javac", args);

        if (result == 0)
            Log(Done, $"build finished successfully. .class files located in {outDir}\n");
        else
            Log(Err, $"project build failed. (exit code {result})\n");
    }
}
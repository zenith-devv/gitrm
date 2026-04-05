using System.Data;
using static Logger;
using static Logger.MessageType;

public class ProjectConfig
{
    public string CompilerFlags { get; set; } = "";
    public string MainFile { get; set; } = "";
    public string OutputFile { get; set; } = "";
}

public static class BuildAssistant
{
    private static readonly List<IBuilder> Builders =
    [
        new CsBuilder(),
        new GccBuilder(),
        new RustBuilder(),
        new GoBuilder(),
        new JavaBuilder(),
    ];

    public static void Build()
    {
        var config = JsonHandler.LoadConfig();

        if (config == null)
            return;
            
        if (string.IsNullOrEmpty(config.MainFile)) 
        {
            Log(Err, "MainFile is missing in bob-config.json\n");
            return;
        }

        string extension = Path.GetExtension(config.MainFile).ToLower();
        var builder = Builders.FirstOrDefault(b => b.CanHandle(extension));

        if (builder != null)
            builder.Build(config);
        else
            Log(Err, $"no builder found for {extension}\n");
    }

    public static void Fetch(string url)
    {
        string cleanUrl = url.TrimEnd('/');
        string repoName = cleanUrl.Substring(cleanUrl.LastIndexOf('/') + 1).Replace(".git", "");

        if (Directory.Exists(repoName))
        {
            Log(Err, $"folder '{repoName}' already exists\n");
            return;
        }

        Log(Default, $"cloning repo '{repoName}' using git...\n");
        int exitCode = CommandRunner.Run("git", $"clone {url}");

        if (exitCode == 0)
        {
            string configPath = Path.Combine(repoName, "bob-config.json");

            if (File.Exists(configPath))
            {
                Log(Default, "bob-config.json found. entering repo...\n");
                Directory.SetCurrentDirectory(repoName); 
                Build();
                Directory.SetCurrentDirectory("..");
            }
            else
            {
                Log(Info, "bob-config.json was not found in the repo. exiting\n");
                Directory.SetCurrentDirectory("..");
            }
        }
        else
        {
            Log(Err, "failed to clone repo\n");
        }
    }
}
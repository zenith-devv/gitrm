using static Logger;
using static Logger.MessageType;

public static class BuildAssistant
{
    private static readonly List<IBuilder> Builders =
    [
        new CsBuilder(),
        new GccBuilder(),
        new RustBuilder(),
        new GoBuilder(),
        new JavaBuilder(),
        new MesonBuilder(),
        new CmakeBuilder(),
    ];

    public static void Build()
    {
        var config = ConfigParser.LoadConfig();
        if (config == null)
            return;

        var builder = Builders.FirstOrDefault(b => b.Detect(Directory.GetCurrentDirectory()));

        if (builder == null)
        {
            if (string.IsNullOrEmpty(config.Build.MainFile)) 
            {
                Log(Err, "MainFile is missing in bob-config.json and no build system detected\n");
                return;
            }

            string extension = Path.GetExtension(config.Build.MainFile).ToLower();
            builder = Builders.FirstOrDefault(b => b.CanHandle(extension));
        }

        if (builder != null)
            builder.Build(config);
        else
        {
            string hint = string.IsNullOrEmpty(config.Build.MainFile) ? "unknown" : Path.GetExtension(config.Build.MainFile);
            Log(Err, $"No builder found for {hint}\n");
        }
    }

    public static void Fetch(string url)
    {
        string cleanUrl = url.TrimEnd('/');
        string repoName = cleanUrl.Substring(cleanUrl.LastIndexOf('/') + 1).Replace(".git", "");

        if (Directory.Exists(repoName))
        {
            Log(Err, $"Folder '{repoName}' already exists\n");
            return;
        }

        Log(Default, $"Cloning repo '{repoName}'...\n");
        int exitCode = CommandRunner.Run("git", $"clone {url}");

        if (exitCode == 0)
        {
            string configPath = Path.Combine(repoName, "gitrm.yaml");

            if (File.Exists(configPath))
            {
                Log(Default, "gitrm.yaml found. Entering repo...\n");
                Directory.SetCurrentDirectory(repoName); 
                Build();
                Directory.SetCurrentDirectory("..");
            }
            else
            {
                Log(Info, "gitrm.yaml was not found in the repo. Exiting\n");
                Directory.SetCurrentDirectory("..");
            }
        }
        else
        {
            Log(Err, "Failed to clone repo\n");
        }
    }
}
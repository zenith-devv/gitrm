using static Logger;
using static Logger.MessageType;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public static class ConfigParser
{
    private const string FileName = "gitrm.yaml";

    public static void CreateTemplate()
    {
        if (!File.Exists(FileName))
        {
            var template = new ProjectConfig
            {
                Build = new BuildSection
                {
                    MainFile = "",
                    Flags = "",
                    OutputPath = ""
                },
            };

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            string yamlContent = serializer.Serialize(template);
            File.WriteAllText(FileName, yamlContent);
            Log(Default, "Created empty gitrm.yaml template\n");
        }
        else
            Log(Err, "gitrm.yaml already exists\n");
    }

    public static ProjectConfig? LoadConfig()
    {
        if (!File.Exists(FileName))
        {
            Log(Err, "Could not find gitrm.yaml. Make sure to run 'gitrm config' and specify the data.\n");
            return null;
        }
        try
        {
            string yamlContent = File.ReadAllText(FileName);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            return deserializer.Deserialize<ProjectConfig>(yamlContent);
        }
        catch (Exception)
        {
            Log(Err, "gitrm.yaml is invalid\n");
            return null;
        }
    }

    public static ProjectConfig? LoadConfigFrom(string path)
    {
        if (!File.Exists(path)) return null;
        try
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            return deserializer.Deserialize<ProjectConfig>(File.ReadAllText(path));
        }
        catch { return null; }
    }
}
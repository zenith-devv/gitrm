using System.Text.Json;
using static Logger;
using static Logger.MessageType;

public static class JsonHandler
{
    private const string FileName = "bobconfig.json";

    public static void CreateTemplate()
    {
        if (!File.Exists(FileName))
        {
            var template = new
            {
                ProjectLang = "",
                MainFile = "",
                Compiler = "",
                Flags = "",
                OutputFile = ""
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(template, options);
            File.WriteAllText(FileName, jsonString);
            Log(Default, "created empty bobconfig.json template\n");
        }
        else
        {
            Log(Err, "bobconfig.json already exists\n");
        }
    }
}
public class ProjectConfig
{
    public BuildSection Build { get; set; } = new();
    // public List<string> Deps { get; set; } = new List<string>();
}

public class BuildSection
{
    public string MainFile { get; set; } = ""; 
    public string Flags { get; set; } = ""; 
    public string OutputPath { get; set; } = "";
}

public class PackageRecord
{
    public string Name { get; set; } = "";
    public string Version { get; set; } = "";
    public string Source { get; set; } = "";
    public DateTime InstalledAt { get; set; }
    public List<string> Binaries { get; set; } = [];
    public bool KeepSource { get; set; } = false;
}
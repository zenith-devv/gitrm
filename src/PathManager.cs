public static class PathManager
{
    private static string Home =>
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    public static string DataDir =>
        Path.Combine(
            Environment.GetEnvironmentVariable("XDG_DATA_HOME")
            ?? Path.Combine(Home, ".local/share"),
            "gitrm");

    public static string BinDir =>
        Path.Combine(Home, ".local/bin");

    public static string DbPath =>
        Path.Combine(DataDir, "db.json");

    public static string PackagesDir =>
        Path.Combine(DataDir, "packages");

    public static void EnsureDirectories()
    {
        Directory.CreateDirectory(DataDir);
        Directory.CreateDirectory(BinDir);
        Directory.CreateDirectory(PackagesDir);
    }

    public static bool IsBinInPath()
    {
        var path = Environment.GetEnvironmentVariable("PATH") ?? "";
        return path.Contains(BinDir) || path.Contains(".local/bin");
    }
}
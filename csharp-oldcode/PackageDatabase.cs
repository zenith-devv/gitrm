using System.Text.Json;
using static Logger;
using static Logger.MessageType;

public static class PackageDatabase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private static Dictionary<string, PackageRecord> _packages = [];
    private static bool _loaded = false;

    public static void EnsureLoaded()
    {
        if (_loaded) return;
        _loaded = true;

        if (!File.Exists(PathManager.DbPath))
            return;

        try
        {
            var json = File.ReadAllText(PathManager.DbPath);
            _packages = JsonSerializer.Deserialize<Dictionary<string, PackageRecord>>(json, JsonOptions) ?? [];
        }
        catch
        {
            Log(Warn, "db.json is corrupted, starting fresh\n");
            _packages = [];
        }
    }

    public static void Add(PackageRecord pkg)
    {
        EnsureLoaded();
        _packages[pkg.Name] = pkg;
        Save();
    }

    public static void Remove(string name)
    {
        EnsureLoaded();
        _packages.Remove(name);
        Save();
    }

    public static PackageRecord? Get(string name)
    {
        EnsureLoaded();
        return _packages.GetValueOrDefault(name);
    }

    public static IEnumerable<PackageRecord> All()
    {
        EnsureLoaded();
        return _packages.Values;
    }

    public static bool Contains(string name)
    {
        EnsureLoaded();
        return _packages.ContainsKey(name);
    }

    private static void Save()
    {
        PathManager.EnsureDirectories();
        File.WriteAllText(PathManager.DbPath,
            JsonSerializer.Serialize(_packages, JsonOptions));
    }
}
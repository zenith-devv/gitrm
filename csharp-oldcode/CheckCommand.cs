using static Logger;
using static Logger.MessageType;
using System.Diagnostics;

public static class CheckCommand
{
    public static async Task Run()
    {
        var packages = PackageDatabase.All().ToList();

        if (packages.Count == 0)
        {
            Log(Info, "No packages installed.\n");
            return;
        }

        Log(Default, "Checking packages...\n\n");

        int removed = 0;
        int updates = 0;

        foreach (var pkg in packages)
        {
            // sprawdź czy binaries istnieją na dysku
            var missing = pkg.Binaries.Where(b => !File.Exists(b)).ToList();

            if (missing.Count > 0)
            {
                Log(Err, $"{pkg.Name} {pkg.Version} — binaries missing, removing from database\n");
                PackageDatabase.Remove(pkg.Name);
                removed++;
                continue;
            }

            // sprawdź czy config istnieje
            var configPath = Path.Combine(PathManager.PackagesDir, pkg.Name, "gitrm.yaml");
            if (!File.Exists(configPath))
                Log(Warn, $"{pkg.Name} {pkg.Version} — config missing\n");

            // sprawdź nową wersję na GitHubie
            var latest = await GetLatestVersion(pkg.Source);

            if (latest == null)
                Log(Warn, $"{pkg.Name} {pkg.Version} — could not fetch remote version\n");
            else if (latest != pkg.Version)
            {
                Log(Info, $"{pkg.Name} {pkg.Version} → {latest} (update available)\n");
                updates++;
            }
            else
                Log(Done, $"{pkg.Name} {pkg.Version} — up to date\n");
        }

        Console.WriteLine();

        if (removed == 0 && updates == 0)
        {
            Log(Done, "All packages OK.\n");
            return;
        }

        if (removed > 0)
            Log(Warn, $"{removed} package(s) removed from database.\n");
        if (updates > 0)
            Log(Info, $"{updates} update(s) available. Run 'gitrm update' to upgrade.\n");
    }

    private static async Task<string?> GetLatestVersion(string repoUrl)
    {
        try
        {
            var psi = new ProcessStartInfo("git")
            {
                Arguments = $"ls-remote --tags {repoUrl}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using var process = Process.Start(psi)!;
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            // parsuj: "abc123  refs/tags/v0.1.0"
            var versions = output
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Where(l => l.Contains("refs/tags/v"))
                .Select(l => l.Split("refs/tags/v").Last().Trim())
                .Select(v => Version.TryParse(v, out var ver) ? ver : null)
                .Where(v => v != null)
                .OrderDescending()
                .FirstOrDefault();

            return versions?.ToString();
        }
        catch
        {
            return null;
        }
    }
}
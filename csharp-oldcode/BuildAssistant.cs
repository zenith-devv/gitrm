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
                Log(Err, "mainFile is missing in gitrm.yaml and no build system detected\n");
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

    public static void Fetch(string url, bool keepSource = false)
    {
        string cleanUrl = url.TrimEnd('/');
        string repoName = cleanUrl.Substring(cleanUrl.LastIndexOf('/') + 1).Replace(".git", "");

        string tempDir = Path.Combine(Path.GetTempPath(), $"gitrm-build-{Guid.NewGuid():N}");

        Log(Default, $"Cloning '{repoName}'...\n");
        int cloneResult = CommandRunner.Run("git", $"clone {url} {tempDir}");

        if (cloneResult != 0)
        {
            Log(Err, "Failed to clone repo\n");
            return;
        }

        string configPath = Path.Combine(tempDir, "gitrm.yaml");
        if (!File.Exists(configPath))
        {
            Log(Info, "gitrm.yaml not found in repo — cannot install\n");
            Directory.Delete(tempDir, true);
            return;
        }

        Log(Default, "gitrm.yaml found. Building...\n");

        string originalDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(tempDir);
        Build();
        Directory.SetCurrentDirectory(originalDir);

        var config = ConfigParser.LoadConfigFrom(configPath);
        if (config == null)
        {
            Log(Err, "Failed to read config after build\n");
            Directory.Delete(tempDir, true);
            return;
        }

        string outDir = Path.Combine(tempDir,
            string.IsNullOrWhiteSpace(config.Build.OutputPath) ? "dist" : config.Build.OutputPath);

        if (!Directory.Exists(outDir))
        {
            Log(Err, $"Output directory '{outDir}' not found after build\n");
            Directory.Delete(tempDir, true);
            return;
        }

        // debug — co widzi w outDir
        Log(Default, $"Scanning output dir: {outDir}\n");
        var allFiles = Directory.GetFiles(outDir);
        Log(Default, $"Total files found: {allFiles.Length}\n");

        foreach (var f in allFiles)
        {
            var ext = Path.GetExtension(f);
            var size = new FileInfo(f).Length;
            long mode = -1;
            try { mode = (long)new FileInfo(f).UnixFileMode; } catch { }
            Log(Debug, $"  {Path.GetFileName(f)} | ext='{ext}' | size={size} | mode={mode}\n");
        }

        PathManager.EnsureDirectories();
        var binaries = allFiles.Where(f => IsExecutable(f)).ToList();
        Log(Default, $"Executables found: {binaries.Count}\n");

        if (binaries.Count == 0)
        {
            Log(Warn, "No executables found in output directory\n");
            Directory.Delete(tempDir, true);
            return;
        }

        var installedBinaries = new List<string>();

        foreach (var binary in binaries)
        {
            string dest = Path.Combine(PathManager.BinDir, Path.GetFileName(binary));
            File.Copy(binary, dest, overwrite: true);

            if (!OperatingSystem.IsWindows())
                CommandRunner.RunQuiet("chmod", $"+x {dest}");

            installedBinaries.Add(dest);
            Log(Done, $"Installed → {dest}\n");
        }

        string pkgConfigDir = Path.Combine(PathManager.PackagesDir, repoName);
        Directory.CreateDirectory(pkgConfigDir);
        File.Copy(configPath, Path.Combine(pkgConfigDir, "gitrm.yaml"), overwrite: true);

        if (keepSource)
        {
            string srcDest = Path.Combine(pkgConfigDir, "src");
            if (Directory.Exists(srcDest)) Directory.Delete(srcDest, true);
            Directory.Move(tempDir, srcDest);
            Log(Info, $"Source kept at {srcDest}\n");
        }
        else
        {
            Directory.Delete(tempDir, true);
        }

        string version = GetInstalledVersion(url);

        PackageDatabase.Add(new PackageRecord
        {
            Name = repoName,
            Version = version,
            Source = url,
            InstalledAt = DateTime.UtcNow,
            Binaries = installedBinaries,
            KeepSource = keepSource
        });

        Log(Done, $"Package '{repoName}' installed successfully\n");

        if (!PathManager.IsBinInPath())
        {
            Log(Warn, "~/.local/bin is not in PATH. Add to ~/.bashrc or ~/.zshrc:\n");
            Console.WriteLine("  export PATH=\"$HOME/.local/bin:$PATH\"");
        }
    }

    private static bool IsExecutable(string path)
    {
        if (OperatingSystem.IsWindows())
            return path.EndsWith(".exe");

        if (!string.IsNullOrEmpty(Path.GetExtension(path)))
            return false;

        var name = Path.GetFileName(path);
        var excluded = new[] { "CACHEDIR.TAG" };
        if (excluded.Contains(name)) return false;

        try
        {
            var mode = new FileInfo(path).UnixFileMode;
            return (mode & (UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute)) != 0;
        }
        catch
        {
            return fileInfo.Length > 100_000;
        }
        catch
        {
            try
            {
                var fileInfo = new FileInfo(path);
                return fileInfo.Length > 100_000;
            }
            catch
            {
                return false;
            }
        }
    }

    private static string GetInstalledVersion(string repoUrl)
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo("git")
            {
                Arguments = $"ls-remote --tags {repoUrl}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            using var process = System.Diagnostics.Process.Start(psi)!;
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            var version = output
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Where(l => l.Contains("refs/tags/v"))
                .Select(l => l.Split("refs/tags/v").Last().Trim())
                .Select(v => Version.TryParse(v, out var ver) ? ver : null)
                .Where(v => v != null)
                .OrderDescending()
                .FirstOrDefault();

            return version?.ToString() ?? "unknown";
        }
        catch { return "unknown"; }
    }

    public static async Task Update(string? packageName = null)
    {
        var packages = packageName != null
            ? new[] { PackageDatabase.Get(packageName) }.Where(p => p != null).Cast<PackageRecord>()
            : PackageDatabase.All();

        var toUpdate = packages.ToList();

        if (toUpdate.Count == 0)
        {
            Log(Info, packageName != null
                ? $"Package '{packageName}' not found in database.\n"
                : "No packages installed.\n");
            return;
        }

        int updated = 0;
        int skipped = 0;

        foreach (var pkg in toUpdate)
        {
            Log(Default, $"Checking {pkg.Name}...\n");

            var latest = await GetLatestVersionAsync(pkg.Source);

            if (latest == null)
            {
                Log(Warn, $"{pkg.Name} — could not fetch remote version, skipping\n");
                skipped++;
                continue;
            }

            if (latest == pkg.Version)
            {
                Log(Done, $"{pkg.Name} {pkg.Version} — already up to date\n");
                skipped++;
                continue;
            }

            Log(Default, $"{pkg.Name} {pkg.Version} → {latest}, updating...\n");

            foreach (var bin in pkg.Binaries)
            {
                if (File.Exists(bin))
                    File.Delete(bin);
            }

            Fetch(pkg.Source, pkg.KeepSource);
            updated++;
        }

        Console.WriteLine();
        Log(Done, $"Update complete. {updated} updated, {skipped} skipped.\n");
    }

    private static async Task<string?> GetLatestVersionAsync(string repoUrl)
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo("git")
            {
                Arguments = $"ls-remote --tags {repoUrl}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            using var process = System.Diagnostics.Process.Start(psi)!;
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            var version = output
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Where(l => l.Contains("refs/tags/v"))
                .Select(l => l.Split("refs/tags/v").Last().Trim())
                .Select(v => Version.TryParse(v, out var ver) ? ver : null)
                .Where(v => v != null)
                .OrderDescending()
                .FirstOrDefault();

            return version?.ToString();
        }
        catch { return null; }
    }
}
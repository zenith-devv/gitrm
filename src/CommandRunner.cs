using System.Diagnostics;

public static class CommandRunner
{
    public static int Run(string fileName, string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        if (process == null) return -1;

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (!string.IsNullOrWhiteSpace(output)) 
            Console.WriteLine(output);

        if (!string.IsNullOrWhiteSpace(error)) 
            Console.WriteLine(error);

        return process.ExitCode;
    }

    public static int RunQuiet(string fileName, string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        if (process == null) return -1;
        
        process.WaitForExit();
        
        return process.ExitCode;
    }
}

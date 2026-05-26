using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Rebelit.OT.InstallationWizard;

public class ScriptRunner(string edgeIp, string username, string password)
{
    public async Task<bool> RunScriptAsync(string scriptPath)
    {
        MakeExecutable(scriptPath);

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        var resolvedScriptPath = isWindows ? ToWslPath(scriptPath) : scriptPath;

        var scriptProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = isWindows ? "wsl.exe" : "/bin/bash",
                Arguments = isWindows
                    ? $"-- bash \"{resolvedScriptPath}\" \"{edgeIp}\" \"{username}\" \"{password}\""
                    : $"\"{resolvedScriptPath}\" \"{edgeIp}\" \"{username}\" \"{password}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Environment =
                {
                    ["EDGE_IP"] = edgeIp,
                    ["EDGE_USER"] = username,
                    ["EDGE_PASSWORD"] = password
                }
            }
        };

        scriptProcess.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null)
                Console.WriteLine(e.Data);
        };

        scriptProcess.ErrorDataReceived += (_, e) =>
        {
            if (e.Data == null) return;
            var prev = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[stderr] {e.Data}");
            Console.ForegroundColor = prev;
        };

        scriptProcess.Start();
        scriptProcess.BeginOutputReadLine();
        scriptProcess.BeginErrorReadLine();
        await scriptProcess.WaitForExitAsync();

        return scriptProcess.ExitCode == 0;
    }

    private static void MakeExecutable(string scriptPath)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return;

        var chmod = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "chmod",
                Arguments = $"+x \"{scriptPath}\"",
                UseShellExecute = false
            }
        };
        chmod.Start();
        chmod.WaitForExit();
    }

    private static string ToWslPath(string path)
    {
        var fullPath = Path.GetFullPath(path);
        var root = Path.GetPathRoot(fullPath);

        if (string.IsNullOrEmpty(root) || root.Length < 2 || root[1] != ':')
            return fullPath.Replace('\\', '/');

        var driveLetter = char.ToLowerInvariant(root[0]);
        var rest = fullPath[root.Length..].Replace('\\', '/');
        return $"/mnt/{driveLetter}/{rest}";
    }
}

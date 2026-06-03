using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Rebelit.OT.InstallationWizard;

public class ScriptRunner(string edgeIp, string username, string password)
{
    public async Task<bool> RunScriptAsync(string scriptPath)
    {
        MakeExecutable(scriptPath);

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        var startInfo = new ProcessStartInfo
        {
            FileName = isWindows ? ResolvePowerShell() : "/bin/bash",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            Environment =
            {
                ["SECURE_EDGE_IP"] = edgeIp,
                ["USERNAME"] = username,
                ["PASSWORD"] = password
            }
        };

        if (isWindows)
        {
            startInfo.ArgumentList.Add("-NoProfile");
            startInfo.ArgumentList.Add("-ExecutionPolicy");
            startInfo.ArgumentList.Add("Bypass");
            startInfo.ArgumentList.Add("-File");
            startInfo.ArgumentList.Add(scriptPath);
            startInfo.ArgumentList.Add(edgeIp);
            startInfo.ArgumentList.Add(username);
            startInfo.ArgumentList.Add(password);
        }
        else
        {
            startInfo.ArgumentList.Add(scriptPath);
            startInfo.ArgumentList.Add(edgeIp);
            startInfo.ArgumentList.Add(username);
            startInfo.ArgumentList.Add(password);
        }

        var scriptProcess = new Process
        {
            StartInfo = startInfo
        };

        scriptProcess.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                Console.WriteLine(e.Data);
            }
        };

        scriptProcess.ErrorDataReceived += (_, e) =>
        {
            if (e.Data == null)
            {
                return;
            }
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

    /// <summary>
    /// Resolves the available PowerShell executable by attempting to execute pwsh.exe and powershell.exe in order.
    /// </summary>
    /// <returns>The name of the first PowerShell executable that successfully runs, or "powershell.exe" as a fallback.</returns>
    private static string ResolvePowerShell()
    {
        foreach (var candidate in new[] { "pwsh.exe", "powershell.exe" })
        {
            try
            {
                using var probe = new Process();
                probe.StartInfo = new ProcessStartInfo
                {
                    FileName = candidate,
                    Arguments = "-NoProfile -Command exit",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                probe.Start();
                probe.WaitForExit();
                return candidate;
            }
            catch (Exception)
            {
                // ignored
            }
        }
        return "powershell.exe";
    }

    private static void MakeExecutable(string scriptPath)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

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
}

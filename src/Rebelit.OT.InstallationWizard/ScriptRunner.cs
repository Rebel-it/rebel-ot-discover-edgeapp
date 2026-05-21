using System.Diagnostics;

namespace Rebelit.OT.InstallationWizard;

public class ScriptRunner(string edgeIp, string username, string password)
{
    public async Task<bool> RunScriptAsync(string scriptPath)
    {
        MakeExecutable(scriptPath);

        var scriptProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"\"{scriptPath}\" \"{edgeIp}\" \"{username}\" \"{password}\"",
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

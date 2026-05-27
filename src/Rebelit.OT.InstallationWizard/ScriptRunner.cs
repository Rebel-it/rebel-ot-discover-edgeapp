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
            FileName = isWindows ? "powershell.exe" : "/bin/bash",
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
}

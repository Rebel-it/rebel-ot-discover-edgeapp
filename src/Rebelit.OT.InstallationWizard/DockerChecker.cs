using System.Diagnostics;

namespace Rebelit.OT.InstallationWizard;

public static class DockerChecker
{
    public static bool IsDockerInstalled()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                Console.WriteLine($"✓ Docker is installed: {output.Trim()}");
                return true;
            }

            Console.WriteLine("✗ Docker does not appear to be running or installed correctly.");
            return false;
        }
        catch (Exception)
        {
            Console.WriteLine("✗ Docker is not installed or not found in PATH. Please install Docker and try again.");
            return false;
        }
    }
}

// See https://aka.ms/new-console-template for more information
using Rebelit.OT.InstallationWizard;
using System.Runtime.InteropServices;

const string githubRepo = "Rebel-it/rebel-ot-discover-edgeapp";
const string extractDir = "/tmp/ot-wizard-release";

var scriptNames = new Dictionary<WizardAction, string>
{
    [WizardAction.Install] = "deploy_discover_edgeapp_args.sh",
    [WizardAction.Uninstall] = "remove_discover_edgeapp_args.sh"
};

var scriptNamesWindows = new Dictionary<WizardAction, string>
{
    [WizardAction.Install] = "deploy_discover_edgeapp_args.ps1",
    [WizardAction.Uninstall] = "remove_discover_edgeapp_args.ps1"
};

Console.WriteLine("Welcome to the OT Discover Edge App Installation Wizard!");

// 1. Check prerequisites
Console.WriteLine("\nChecking prerequisites...");
if (!DockerChecker.IsDockerInstalled())
    Environment.Exit(1);

// 2. Ask what to do
var action = WizardConsole.PromptAction();

// 3. Gather credentials
var edgeIp        = WizardConsole.PromptLine("Please enter your SecureEdge Pro IP address (e.g. 192.168.140.1)");
var adminUser     = WizardConsole.PromptLine("Please enter your SecureEdge Pro admin username");
var adminPassword = WizardConsole.PromptLine("Please enter your SecureEdge Pro admin password");

// TODO remove this once the repository is made public.
var accessToken   = WizardConsole.PromptLine("Please enter your GitHub access token");

// 4. Download & extract latest GitHub release
Console.WriteLine("\nFetching latest release from GitHub...");
try
{
    var downloader = new GitHubReleaseDownloader(githubRepo, accessToken);
    await downloader.DownloadAndExtractLatestReleaseAsync(extractDir);

    string targetScript;
    if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        targetScript = scriptNamesWindows[action];
    }
    else
    {
        targetScript = scriptNames[action];
    }

    var scriptPath = Directory.GetFiles(extractDir, targetScript, SearchOption.AllDirectories).FirstOrDefault();
    if (scriptPath is null)
    {
        WizardConsole.PrintError($"Script '{targetScript}' was not found in the release.");
        Environment.Exit(1);
    }

    // 5. Run the script
    Console.WriteLine($"\nRunning script: {Path.GetFileName(scriptPath)}");
    var runner = new ScriptRunner(edgeIp, adminUser, adminPassword);
    var success = await runner.RunScriptAsync(scriptPath);

    if (!success)
    {
        WizardConsole.PrintError($"Script '{targetScript}' failed.");
        Environment.Exit(1);
    }

    var completionMessage = action == WizardAction.Install ? "Installation complete!" : "Uninstall complete!";
    WizardConsole.PrintSuccess(completionMessage);
}
catch (HttpRequestException ex)
{
    WizardConsole.PrintError($"Failed to reach GitHub: {ex.Message}");
    Environment.Exit(1);
}
catch (Exception ex)
{
    WizardConsole.PrintError($"An error occurred: {ex.Message}");
    Environment.Exit(1);
}


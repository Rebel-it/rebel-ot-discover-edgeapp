namespace Rebelit.OT.InstallationWizard;

public enum WizardAction { Install, Uninstall }

public static class WizardConsole
{
    public static string PromptLine(string message)
    {
        Console.WriteLine(message);
        return Console.ReadLine() ?? string.Empty;
    }

    public static WizardAction PromptAction()
    {
        while (true)
        {
            Console.WriteLine("\nWhat would you like to do?");
            Console.WriteLine("  [1] Install the OT Discover Edge App");
            Console.WriteLine("  [2] Uninstall the OT Discover Edge App");
            Console.Write("Enter your choice (1 or 2): ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1": return WizardAction.Install;
                case "2": return WizardAction.Uninstall;
                default:
                    Console.WriteLine("✗ Invalid choice. Please enter 1 or 2.");
                    break;
            }
        }
    }

    public static void PrintSuccess(string message) => Console.WriteLine($"✓ {message}");
    public static void PrintError(string message) => Console.WriteLine($"✗ {message}");
}

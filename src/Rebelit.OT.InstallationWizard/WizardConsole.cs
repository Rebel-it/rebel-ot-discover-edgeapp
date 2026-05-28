namespace Rebelit.OT.InstallationWizard;

public enum WizardAction { Install, Uninstall, Exit }

public static class WizardConsole
{
    public static string PromptLine(string message)
    {
        Console.WriteLine(message);
        return (Console.ReadLine() ?? string.Empty).Trim();
    }

    public static WizardAction PromptAction()
    {
        while (true)
        {
            Console.WriteLine("\nWhat would you like to do?");
            Console.WriteLine("  [1] Install the OT Discover Edge App");
            Console.WriteLine("  [2] Uninstall the OT Discover Edge App");
            Console.WriteLine("  [8] To exit the installer");
            Console.Write("Enter your choice: ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1": return WizardAction.Install;
                case "2": return WizardAction.Uninstall;
                case "8": Environment.Exit(0); return WizardAction.Exit;
                default:
                    Console.WriteLine("✗ Invalid choice.");
                    break;
            }
        }
    }

    public static void PrintSuccess(string message) => Console.WriteLine($"✓ {message}");
    public static void PrintError(string message) => Console.WriteLine($"✗ {message}");
}

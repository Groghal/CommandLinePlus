using CommandLine;
using CommandLinePlus.GUI;
using CommandLinePlus.GUI.Example.Models.Verbs;
using CommandLinePlus.GUI.Models;

namespace CommandLinePlus.GUI.Example;

internal class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Configure CommandLinePlus.GUI for a Docker-like CLI tool
        var config = new GuiConfiguration
        {
            ExecutableName = "docker",
            WindowTitle = "Docker Command Builder",
            ShowCommandPreview = true,
            WindowWidth = 900,
            WindowHeight = 600,
            SortOrder = ControlSortOrder.RequiredFirstThenName, // Required fields first, then alphabetical
            Theme = new GuiTheme
            {
                BackgroundColor = Color.WhiteSmoke,
                RequiredLabelColor = Color.DarkRed,
                ShowRequiredAsterisk = true,
                UseRequiredFieldBoldFont = true,
                RequiredFieldBackgroundColor = Color.FromArgb(255, 255, 240, 240) // Light red tint
            }
        };

        // Show GUI with multiple verbs from current assembly
        var result = CommandLinePlus.Show(new[] { typeof(Program).Assembly, typeof(DockerBuildOptions).Assembly }, args,
            config);

        if (result != null)
        {
            Console.WriteLine($"\nExecuted Command Type: {result.GetType().Name}");
            Console.WriteLine($"Generated Command: docker {result}");

            // Handle specific command types
            switch (result)
            {
                case DockerRunOptions run:
                    Console.WriteLine($"\nContainer will run with:");
                    Console.WriteLine($"  Image: {run.Image}");
                    Console.WriteLine($"  Memory: {run.Memory}");
                    Console.WriteLine($"  Restart Policy: {run.RestartPolicy}");
                    if (run.Ports?.Any() == true)
                        Console.WriteLine($"  Ports: {string.Join(", ", run.Ports)}");
                    break;

                case DockerBuildOptions build:
                    Console.WriteLine($"\nBuilding image:");
                    Console.WriteLine($"  Context: {build.Context}");
                    if (build.Tags?.Any() == true)
                        Console.WriteLine($"  Tags: {string.Join(", ", build.Tags)}");
                    Console.WriteLine($"  Platform: {build.Platform}");
                    break;

                case DockerComposeOptions compose:
                    Console.WriteLine($"\nCompose operation:");
                    if (compose.Files?.Any() == true)
                        Console.WriteLine($"  Files: {string.Join(", ", compose.Files)}");
                    if (compose.Profiles?.Any() == true)
                        Console.WriteLine($"  Profiles: {string.Join(", ", compose.Profiles)}");
                    break;
            }
        }
        else
        {
            Console.WriteLine("Operation cancelled by user.");
        }
    }
}
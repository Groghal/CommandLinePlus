using CommandLine;
using CommandLinePlus.GUI;
using CommandLinePlus.GUI.Example.Models.Models;
using CommandLinePlus.GUI.Example.Models.Verbs;
using CommandLinePlus.GUI.Models;
using CommandLinePlus.Shared;

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
            },
            // Enable command execution to trigger IPostAction
            ExecuteCommand = false, // Set to true to actually execute commands
            // Custom command executor (optional) - this example just simulates execution
            CommandExecutor = (command) =>
            {
                Console.WriteLine($"[Simulated Execution] {command}");
                // Simulate success for demonstration
                return (0, "Build complete", "");
            }
        };

        // Define custom default setter action
        Action<IDefaultSetter> customDefaultSetter = (defaultSetter) =>
        {
            // Check if it's a DockerBuildOptions instance
            if (defaultSetter is DockerBuildOptions buildOptions)
            {
                // Set custom defaults based on environment or context
                buildOptions.Platform = Platform.LinuxAmd64;
                buildOptions.NoCache = true;
                buildOptions.Progress = "plain";
                buildOptions.Tags = new[] { "myapp:latest", "myapp:v1.0" };
            }
            // Check if it's a DockerRunOptions instance
            else if (defaultSetter is DockerRunOptions runOptions)
            {
                // Set custom defaults for run command
                runOptions.RestartPolicy = RestartPolicy.UnlessStopped;
                runOptions.Memory = "512m";
                runOptions.Detach = true;
            }
        };
        
        // Show GUI with multiple verbs from current assembly
        var result = CommandLinePlus.Show(new[] { typeof(Program).Assembly, typeof(DockerBuildOptions).Assembly }, args,
            config, setter => customDefaultSetter(setter));

        if (result != null)
        {
            Console.WriteLine($"\nExecuted Command Type: {result.GetType().Name}");
            var commandString = CommandArgumentBuilder.BuildCommandString(result);
            Console.WriteLine($"Generated Command: docker {commandString}");

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
            
            // Note: If the result implements IPostAction and ExecuteCommand is true in configuration,
            // the post-action will be executed automatically after the command runs.
            // Example: DockerBuildWithPostAction implements IPostAction to log build results.
        }
        else
        {
            Console.WriteLine("Operation cancelled by user.");
        }
    }
}
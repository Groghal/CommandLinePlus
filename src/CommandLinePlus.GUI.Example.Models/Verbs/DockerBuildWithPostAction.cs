using CommandLine;
using CommandLinePlus.Shared;

namespace CommandLinePlus.GUI.Example.Models.Verbs;

/// <summary>
/// Example of a verb that implements IPostAction
/// </summary>
[Verb("build-with-log", HelpText = "Build Docker image and log the result")]
public class DockerBuildWithPostAction : DockerBuildOptions, IPostAction
{
    [Option("log-file", Required = false, HelpText = "Path to log file for build output")]
    public string LogFile { get; set; }

    public void ExecutePostAction(object verb, int exitCode, string output, string error)
    {
        // Cast to get typed access to properties
        var buildOptions = verb as DockerBuildWithPostAction;
        
        // Example post-action: log the build result
        var logMessage = $"""
            Docker Build Completed at {DateTime.Now:yyyy-MM-dd HH:mm:ss}
            Exit Code: {exitCode}
            Context: {buildOptions?.Context}
            Tags: {string.Join(", ", buildOptions?.Tags ?? new[] { "<none>" })}
            Platform: {buildOptions?.Platform}
            No-Cache: {buildOptions?.NoCache}
            Dockerfile: {buildOptions?.Dockerfile ?? "Dockerfile"}
            
            Output:
            {output}
            
            Errors:
            {error}
            
            ========================================
            
            """;

        if (!string.IsNullOrEmpty(buildOptions?.LogFile))
        {
            try
            {
                File.AppendAllText(buildOptions.LogFile, logMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write log: {ex.Message}");
            }
        }
        
        // Also write to console
        Console.WriteLine($"\n=== Post-Build Action ===");
        Console.WriteLine($"Build completed with exit code: {exitCode}");
        Console.WriteLine($"Context: {buildOptions?.Context}");
        
        if (exitCode == 0)
        {
            Console.WriteLine("✓ Build successful!");
            if (buildOptions?.Tags?.Any() == true)
            {
                Console.WriteLine($"✓ Tagged as: {string.Join(", ", buildOptions.Tags)}");
            }
            
            // Example: Could trigger deployment if build succeeds
            if (buildOptions?.Platform == Models.Platform.LinuxAmd64)
            {
                Console.WriteLine("→ Ready for Linux AMD64 deployment");
            }
        }
        else
        {
            Console.WriteLine("✗ Build failed!");
            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine($"Error: {error}");
            }
        }
    }
}
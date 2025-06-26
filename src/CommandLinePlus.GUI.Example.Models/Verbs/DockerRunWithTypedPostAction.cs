using CommandLine;
using CommandLinePlus.Shared;

namespace CommandLinePlus.GUI.Example.Models.Verbs;

/// <summary>
/// Example of a verb that implements the generic IPostAction<T> for type safety
/// </summary>
[Verb("run-typed", HelpText = "Run container with typed post-action")]
public class DockerRunWithTypedPostAction : DockerRunOptions, IPostAction<DockerRunWithTypedPostAction>
{
    [Option("health-check-url", Required = false, HelpText = "URL to check after container starts")]
    public string HealthCheckUrl { get; set; }
    
    [Option("startup-delay", Required = false, Default = 5, HelpText = "Seconds to wait before health check")]
    public int StartupDelay { get; set; }

    public void ExecutePostAction(DockerRunWithTypedPostAction verb, int exitCode, string output, string error)
    {
        Console.WriteLine($"\n=== Post-Run Action (Typed) ===");
        Console.WriteLine($"Container: {verb.Image}");
        Console.WriteLine($"Name: {verb.Name ?? "<auto>"}");
        
        if (exitCode == 0)
        {
            Console.WriteLine("✓ Container started successfully!");
            
            // Log port mappings with direct typed access
            if (verb.Ports?.Any() == true)
            {
                Console.WriteLine($"✓ Exposed ports: {string.Join(", ", verb.Ports)}");
            }
            
            // Log environment variables
            if (verb.Environment?.Any() == true)
            {
                Console.WriteLine($"✓ Environment variables set: {verb.Environment.Count()}");
            }
            
            // Perform health check if configured
            if (!string.IsNullOrEmpty(verb.HealthCheckUrl))
            {
                Console.WriteLine($"\n→ Waiting {verb.StartupDelay} seconds before health check...");
                System.Threading.Thread.Sleep(verb.StartupDelay * 1000);
                
                try
                {
                    using var client = new System.Net.Http.HttpClient();
                    var response = client.GetAsync(verb.HealthCheckUrl).Result;
                    
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"✓ Health check passed: {verb.HealthCheckUrl}");
                    }
                    else
                    {
                        Console.WriteLine($"✗ Health check failed: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ Health check error: {ex.Message}");
                }
            }
            
            // Could send notifications, update service registry, etc.
            if (verb.RestartPolicy == Models.RestartPolicy.Always)
            {
                Console.WriteLine("→ Container configured for automatic restart");
            }
        }
        else
        {
            Console.WriteLine($"✗ Failed to start container (exit code: {exitCode})");
            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine($"Error: {error}");
            }
            
            // Could perform cleanup, send alerts, etc.
        }
    }
}
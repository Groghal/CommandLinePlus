using CommandLine;

namespace CommandLinePlus.GUI.Example.Models.Verbs;

[Verb("compose", HelpText = "Docker Compose operations")]
public class DockerComposeOptions
{
    [Option('f', "file", Required = false, HelpText = "Compose configuration files")]
    public IEnumerable<string> Files { get; set; }

    [Option('p', "project-name", Required = false, HelpText = "Project name")]
    public string ProjectName { get; set; }

    [Option("profile", Required = false, HelpText = "Profiles to enable")]
    public IEnumerable<string> Profiles { get; set; }

    [Option("env-file", Required = false, HelpText = "Environment file")]
    public IEnumerable<string> EnvFiles { get; set; }

    [Option("parallel", Required = false, Default = 1, HelpText = "Max concurrency for parallel operations")]
    public int Parallel { get; set; }

    [Option("compatibility", Required = false, HelpText = "Run in compatibility mode")]
    public bool Compatibility { get; set; }

    [Option("dry-run", Required = false, HelpText = "Execute command in dry run mode")]
    public bool DryRun { get; set; }

    [Option("command", Required = true, HelpText = "Compose command (up, down, start, stop, etc.)")]
    public string Command { get; set; }

    [Option("services", Required = false, HelpText = "Services to operate on")]
    public IEnumerable<string> Services { get; set; }
}
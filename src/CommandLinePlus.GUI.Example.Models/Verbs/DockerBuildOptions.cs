using CommandLine;
using CommandLinePlus.GUI.Example.Models.Models;
using CommandLinePlus.Shared;

namespace CommandLinePlus.GUI.Example.Models.Verbs;

[Verb("build", HelpText = "Build an image from a Dockerfile")]
public class DockerBuildOptions : IDefaultSetter
{
    [Option('t', "tag", Required = false, HelpText = "Name and optionally tag in name:tag format")]
    public IEnumerable<string> Tags { get; set; }

    [Option('f', "file", Required = false, HelpText = "Name of the Dockerfile")]
    [PathType(PathType.File, Filter = "Dockerfiles|Dockerfile*|All Files|*.*")]
    public string Dockerfile { get; set; }

    [Option("build-arg", Required = false, HelpText = "Set build-time variables")]
    public IEnumerable<string> BuildArgs { get; set; }

    [Option("target", Required = false, HelpText = "Set the target build stage")]
    public string Target { get; set; }

    [Option("platform", Required = false, HelpText = "Set platform if server is multi-platform capable")]
    public Platform? Platform { get; set; }

    [Option("no-cache", Required = false, HelpText = "Do not use cache when building")]
    public bool NoCache { get; set; }

    [Option("rm", Required = false, HelpText = "Remove intermediate containers after build")]
    public bool? RemoveIntermediateContainers { get; set; }

    [Option("force-rm", Required = false, HelpText = "Always remove intermediate containers")]
    public bool? ForceRemoveIntermediateContainers { get; set; }

    [Option("pull", Required = false, Default = PullPolicy.Missing, HelpText = "Always attempt to pull newer version")]
    public PullPolicy? Pull { get; set; }

    [Option("progress", Required = false, Default = "auto", HelpText = "Set type of progress output")]
    public string Progress { get; set; }

    [Option("secret", Required = false, HelpText = "Secret to expose to the build")]
    public IEnumerable<string> Secrets { get; set; }

    [Option("cache-from", Required = false, HelpText = "Images to consider as cache sources")]
    public IEnumerable<string> CacheFrom { get; set; }

    [Option("label", Required = false, HelpText = "Set metadata for an image")]
    public IEnumerable<string> Labels { get; set; }

    [Option("context", Required = false, Default = ".", HelpText = "Build context path or URL")]
    [PathType(PathType.Directory)]
    public string Context { get; set; }

    public void UpdateDefaults()
    {
        // Set default context if not already set
        if (string.IsNullOrEmpty(Context)) Context = ".";
    }
}
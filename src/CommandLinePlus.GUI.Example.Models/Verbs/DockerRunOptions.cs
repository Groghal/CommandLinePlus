using CommandLine;
using CommandLinePlus.GUI.Example.Models.Models;

namespace CommandLinePlus.GUI.Example.Models.Verbs;

// Docker-like verb classes with complex types including Enums and IEnumerable

// Enums for various Docker options

[Verb("run", HelpText = "Run a command in a new container")]
public class DockerRunOptions
{
    [Option('d', "detach", Required = false, HelpText = "Run container in background")]
    public bool Detach { get; set; }

    [Option('i', "interactive", Required = false, HelpText = "Keep STDIN open")]
    public bool Interactive { get; set; }

    [Option('t', "tty", Required = false, HelpText = "Allocate a pseudo-TTY")]
    public bool Tty { get; set; }

    [Option("name", Required = false, HelpText = "Assign a name to the container")]
    public string Name { get; set; }

    [Option('p', "publish", Required = false, HelpText = "Publish container port(s) to the host")]
    public IEnumerable<string> Ports { get; set; }

    [Option('e', "env", Required = false, HelpText = "Set environment variables")]
    public IEnumerable<string> Environment { get; set; }

    [Option('v', "volume", Required = false, HelpText = "Bind mount a volume")]
    public IEnumerable<string> Volumes { get; set; }

    [Option("restart", Required = false, Default = RestartPolicy.No, HelpText = "Restart policy")]
    public RestartPolicy RestartPolicy { get; set; }

    [Option("log-driver", Required = false, Default = LogDriver.Json, HelpText = "Logging driver")]
    public LogDriver LogDriver { get; set; }

    [Option("network", Required = false, Default = NetworkMode.Bridge, HelpText = "Connect to a network")]
    public NetworkMode Network { get; set; }

    [Option('m', "memory", Required = false, HelpText = "Memory limit (e.g., 512m, 2g)")]
    public string Memory { get; set; }

    [Option("cpus", Required = false, HelpText = "Number of CPUs (decimal)")]
    public double? Cpus { get; set; }

    [Option("memory-swap", Required = false, HelpText = "Swap limit equal to memory plus swap")]
    public string MemorySwap { get; set; }

    [Option("pids-limit", Required = false, HelpText = "Tune container pids limit")]
    public int? PidsLimit { get; set; }

    [Option("shm-size", Required = false, HelpText = "Size of /dev/shm")]
    public string ShmSize { get; set; }

    [Option("cpu-shares", Required = false, HelpText = "CPU shares (relative weight)")]
    public int? CpuShares { get; set; }

    [Option("label", Required = false, HelpText = "Set metadata on container")]
    public IEnumerable<string> Labels { get; set; }

    [Option("cap-add", Required = false, HelpText = "Add Linux capabilities")]
    public IEnumerable<string> CapabilitiesToAdd { get; set; }

    [Option("security-opt", Required = false, HelpText = "Security options (nullable example)")]
    public IEnumerable<SecurityOption>? SecurityOptions { get; set; }

    [Option("image", Required = true, HelpText = "Docker image to run")]
    public string Image { get; set; }

    [Option("command", Required = false, HelpText = "Command to run in container")]
    public IEnumerable<string> Command { get; set; }
}

// Example with IEnumerable of enums
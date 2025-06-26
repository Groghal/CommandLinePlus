using CommandLine;
using CommandLinePlus.GUI.Example.Models.Models;

namespace CommandLinePlus.GUI.Example.Models.Verbs;

[Verb("security", HelpText = "Advanced security configuration example")]
public class SecurityOptions
{
    [Option("user", Required = false, HelpText = "User and group to run as")]
    public string User { get; set; }

    [Option("cap-add", Required = false, HelpText = "Add Linux capabilities")]
    public IEnumerable<Capability> CapabilitiesToAdd { get; set; }

    [Option("cap-drop", Required = false, HelpText = "Drop Linux capabilities")]
    public IEnumerable<Capability> CapabilitiesToDrop { get; set; }

    [Option("security-opt", Required = false, HelpText = "Security options")]
    public IEnumerable<string> SecurityOptionsList { get; set; }

    [Option("privileged", Required = false, HelpText = "Give extended privileges")]
    public bool Privileged { get; set; }

    [Option("read-only", Required = false, HelpText = "Mount container's root filesystem as read only")]
    public bool ReadOnly { get; set; }

    [Option("pid", Required = false, HelpText = "PID namespace to use")]
    public string PidNamespace { get; set; }

    [Option("ipc", Required = false, HelpText = "IPC namespace to use")]
    public string IpcNamespace { get; set; }

    [Option("network", Required = false, HelpText = "Network namespaces")]
    public IEnumerable<NetworkMode> Networks { get; set; }
}
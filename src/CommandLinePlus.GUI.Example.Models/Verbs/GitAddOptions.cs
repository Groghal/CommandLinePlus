using CommandLine;

namespace CommandLinePlus.GUI.Example.Models.Verbs;

[Verb("add", HelpText = "Add file contents to the index")]
public class GitAddOptions
{
    [Option('p', "patch", Required = false, HelpText = "Interactively choose hunks of patch")]
    public bool Patch { get; set; }

    [Option('f', "force", Required = false, HelpText = "Allow adding otherwise ignored files")]
    public bool Force { get; set; }

    [Option('u', "update", Required = false, HelpText = "Update tracked files")]
    public bool Update { get; set; }

    [Option("files", HelpText = "Files to add", Required = true)]
    public IEnumerable<string> Files { get; set; }
}
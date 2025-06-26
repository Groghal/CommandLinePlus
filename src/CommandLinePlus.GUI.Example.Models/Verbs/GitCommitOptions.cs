using CommandLine;

namespace CommandLinePlus.GUI.Example.Models.Verbs;

[Verb("commit", HelpText = "Record changes to the repository")]
public class GitCommitOptions
{
    [Option('m', "message", Required = true, HelpText = "Commit message")]
    public string Message { get; set; }

    [Option('a', "all", Required = false, HelpText = "Commit all changed files")]
    public bool All { get; set; }

    [Option("amend", Required = false, HelpText = "Amend previous commit")]
    public bool Amend { get; set; }

    [Option("author", Required = false, HelpText = "Override author for commit")]
    public string Author { get; set; }
}
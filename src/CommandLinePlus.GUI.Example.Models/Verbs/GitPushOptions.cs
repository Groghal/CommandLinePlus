using CommandLine;

namespace CommandLinePlus.GUI.Example.Models.Verbs;

[Verb("push", HelpText = "Update remote refs along with associated objects")]
public class GitPushOptions
{
    [Option('f', "force", Required = false, HelpText = "Force push")]
    public bool Force { get; set; }

    [Option('u', "set-upstream", Required = false, HelpText = "Set upstream for git pull/status")]
    public bool SetUpstream { get; set; }

    [Option("repository", HelpText = "Repository name")]
    public string Repository { get; set; }

    [Option("refspec", HelpText = "Branch to push")]
    public string RefSpec { get; set; }
}
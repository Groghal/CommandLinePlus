namespace CommandLinePlus.Shared;

/// <summary>
/// Interface for defining post-execution actions for command-line verbs
/// </summary>
public interface IPostAction
{
    /// <summary>
    /// Executes after the command has been run
    /// </summary>
    /// <param name="verb">The verb instance with all property values</param>
    /// <param name="exitCode">The exit code from the command execution</param>
    /// <param name="output">The output from the command execution</param>
    /// <param name="error">The error output from the command execution</param>
    void ExecutePostAction(object verb, int exitCode, string output, string error);
}
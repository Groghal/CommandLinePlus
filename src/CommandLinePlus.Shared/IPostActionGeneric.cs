namespace CommandLinePlus.Shared;

/// <summary>
/// Generic interface for defining post-execution actions with typed verb access
/// </summary>
/// <typeparam name="T">The verb type</typeparam>
public interface IPostAction<in T> : IPostAction where T : class
{
    /// <summary>
    /// Executes after the command has been run with typed verb access
    /// </summary>
    /// <param name="verb">The verb instance with all property values</param>
    /// <param name="exitCode">The exit code from the command execution</param>
    /// <param name="output">The output from the command execution</param>
    /// <param name="error">The error output from the command execution</param>
    void ExecutePostAction(T verb, int exitCode, string output, string error);
    
    // Implement the non-generic interface
    void IPostAction.ExecutePostAction(object verb, int exitCode, string output, string error)
    {
        if (verb is T typedVerb)
        {
            ExecutePostAction(typedVerb, exitCode, output, error);
        }
    }
}
namespace CommandLinePlus.Shared;

/// <summary>
/// Specifies the type of file system path
/// </summary>
public enum PathType
{
    /// <summary>
    /// Path can be either a file or directory
    /// </summary>
    Any,

    /// <summary>
    /// Path must be a file
    /// </summary>
    File,

    /// <summary>
    /// Path must be a directory
    /// </summary>
    Directory
}
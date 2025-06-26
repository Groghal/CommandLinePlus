namespace CommandLinePlus.Shared;

/// <summary>
/// Specifies the type of path expected for a property (file, directory, or any)
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class PathTypeAttribute : Attribute
{
    /// <summary>
    /// Gets the type of path expected
    /// </summary>
    public PathType PathType { get; }

    /// <summary>
    /// Gets or sets the file filter for file selection (e.g., "Text files|*.txt|All files|*.*")
    /// </summary>
    public string Filter { get; set; } = "All Files|*.*";

    /// <summary>
    /// Initializes a new instance of the PathTypeAttribute class
    /// </summary>
    /// <param name="pathType">The type of path expected</param>
    public PathTypeAttribute(PathType pathType)
    {
        PathType = pathType;
    }
}
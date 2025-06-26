namespace CommandLinePlus.GUI.Models;

/// <summary>
/// Defines how controls should be sorted in the GUI
/// </summary>
public enum ControlSortOrder
{
    /// <summary>
    /// Keep the order as defined in the source code
    /// </summary>
    SourceCode,

    /// <summary>
    /// Sort alphabetically by property name
    /// </summary>
    ByName,

    /// <summary>
    /// Sort by property type, then by name within each type
    /// </summary>
    ByTypeThenName,

    /// <summary>
    /// Show required options first, then optional
    /// </summary>
    RequiredFirst,

    /// <summary>
    /// Show required options first, then sort by name
    /// </summary>
    RequiredFirstThenName,

    /// <summary>
    /// Show required options first, then sort by type and name
    /// </summary>
    RequiredFirstThenType
}
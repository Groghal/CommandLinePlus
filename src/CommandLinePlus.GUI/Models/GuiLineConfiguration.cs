using CommandLinePlus.GUI.Models;

namespace CommandLinePlus.GUI;

/// <summary>
/// Configuration options for CommandLinePlus.GUI
/// </summary>
public class GuiConfiguration
{
    /// <summary>
    /// The executable name to display in the generated command
    /// </summary>
    public string ExecutableName { get; set; } = "dotnet";

    /// <summary>
    /// Whether to show borders around controls for debugging
    /// </summary>
    public bool ShowControlBorders { get; set; } = false;

    /// <summary>
    /// Window title
    /// </summary>
    public string WindowTitle { get; set; } = "Command Line GUI";

    /// <summary>
    /// Initial window width
    /// </summary>
    public int WindowWidth { get; set; } = 800;

    /// <summary>
    /// Initial window height
    /// </summary>
    public int WindowHeight { get; set; } = 500;

    /// <summary>
    /// Whether to allow drag and drop for file/folder inputs
    /// </summary>
    public bool EnableDragDrop { get; set; } = true;

    /// <summary>
    /// Whether to show command preview before execution
    /// </summary>
    public bool ShowCommandPreview { get; set; } = true;

    /// <summary>
    /// How to sort the generated controls
    /// </summary>
    public ControlSortOrder SortOrder { get; set; } = ControlSortOrder.SourceCode;

    /// <summary>
    /// Whether to show Check All/Uncheck All buttons for enum checklist boxes
    /// </summary>
    public bool ShowCheckAllButtons { get; set; } = true;

    /// <summary>
    /// Whether to show the search/filter panel
    /// </summary>
    public bool ShowSearchFilter { get; set; } = true;

    /// <summary>
    /// Whether the window should start as always on top
    /// </summary>
    public bool AlwaysOnTop { get; set; } = false;

    /// <summary>
    /// Custom theme settings
    /// </summary>
    public GuiTheme Theme { get; set; } = new();
}
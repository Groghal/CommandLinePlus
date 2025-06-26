namespace CommandLinePlus.GUI.Models;

/// <summary>
/// Theme configuration for CommandLinePlus.GUI
/// </summary>
public class GuiTheme
{
    /// <summary>
    /// Background color for the main window
    /// </summary>
    public Color BackgroundColor { get; set; } = SystemColors.Control;

    /// <summary>
    /// Text color
    /// </summary>
    public Color TextColor { get; set; } = SystemColors.ControlText;

    /// <summary>
    /// Font for labels
    /// </summary>
    public Font LabelFont { get; set; } = SystemFonts.DefaultFont;

    /// <summary>
    /// Font for input controls
    /// </summary>
    public Font InputFont { get; set; } = SystemFonts.DefaultFont;

    /// <summary>
    /// Color for required field labels
    /// </summary>
    public Color RequiredLabelColor { get; set; } = Color.DarkRed;

    /// <summary>
    /// Whether to show an asterisk (*) after required field labels
    /// </summary>
    public bool ShowRequiredAsterisk { get; set; } = true;

    /// <summary>
    /// Background color for required field inputs
    /// </summary>
    public Color RequiredFieldBackgroundColor { get; set; } = Color.FromArgb(255, 255, 240, 240); // Light red tint

    /// <summary>
    /// Border color for required field inputs (if supported by control)
    /// </summary>
    public Color RequiredFieldBorderColor { get; set; } = Color.FromArgb(255, 200, 0, 0);

    /// <summary>
    /// Whether to use bold font for required field labels
    /// </summary>
    public bool UseRequiredFieldBoldFont { get; set; } = true;
}
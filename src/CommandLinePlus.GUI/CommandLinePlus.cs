using System.Reflection;

namespace CommandLinePlus.GUI;

/// <summary>
/// Main entry point for CommandLinePlus.GUI - a GUI generator for CommandLineParser
/// </summary>
public static class CommandLinePlus
{
    /// <summary>
    /// Shows a GUI for the specified command-line options type
    /// </summary>
    /// <typeparam name="T">The options class with CommandLineParser attributes</typeparam>
    /// <param name="args">Command line arguments to pre-populate the form</param>
    /// <param name="configuration">Optional configuration for the GUI</param>
    /// <returns>The parsed options object if user clicked Run, null if cancelled</returns>
    public static T Show<T>(string[] args = null, GuiConfiguration configuration = null) where T : class
    {
        var assemblies = new[] { typeof(T).Assembly };
        return Show<T>(assemblies, args, configuration);
    }

    /// <summary>
    /// Shows a GUI that can handle multiple verb types from specified assemblies
    /// </summary>
    /// <param name="assemblies">Assemblies to scan for verb classes</param>
    /// <param name="args">Command line arguments to pre-populate the form</param>
    /// <param name="configuration">Optional configuration for the GUI</param>
    /// <returns>The parsed options object if user clicked Run, null if cancelled</returns>
    public static object Show(IEnumerable<Assembly> assemblies, string[] args = null,
        GuiConfiguration configuration = null)
    {
        configuration ??= new GuiConfiguration();
        var manager = new CliGuiManager(configuration.ExecutableName, assemblies, configuration);
        return manager.ShowGui(args);
    }

    /// <summary>
    /// Shows a GUI for a specific options type from specified assemblies
    /// </summary>
    /// <typeparam name="T">The expected return type</typeparam>
    /// <param name="assemblies">Assemblies to scan for verb classes</param>
    /// <param name="args">Command line arguments to pre-populate the form</param>
    /// <param name="configuration">Optional configuration for the GUI</param>
    /// <returns>The parsed options object if user clicked Run, null if cancelled</returns>
    public static T Show<T>(IEnumerable<Assembly> assemblies, string[] args = null,
        GuiConfiguration configuration = null) where T : class
    {
        return Show(assemblies, args, configuration) as T;
    }
}
using System.Reflection;
using CommandLine;
using CommandLinePlus.GUI.Models;
using CommandLinePlus.Shared;

namespace CommandLinePlus.GUI;

public class CliGuiManager
{
    private readonly string _exeName;
    private readonly IEnumerable<Assembly> _assemblies;
    private readonly GuiConfiguration _configuration;
    private readonly Action<IDefaultSetter> _defaultSetterAction;

    public CliGuiManager(string exeName, IEnumerable<Assembly> assemblies, GuiConfiguration configuration = null,
        Action<IDefaultSetter> defaultSetterAction = null)
    {
        _exeName = exeName;
        _assemblies = assemblies;
        _configuration = configuration ?? new GuiConfiguration();
        _defaultSetterAction = defaultSetterAction;
    }


    private bool IsEnumerableType(Type type)
    {
        // Check if the type itself is IEnumerable<T> (excluding string, which implements IEnumerable<char>)
        if (type != typeof(string) &&
            type.IsGenericType &&
            type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            return true;

        // Check if the type implements IEnumerable<T>
        return type != typeof(string) &&
               type.GetInterfaces()
                   .Any(i => i.IsGenericType &&
                             i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
    }

    public object ShowGui(string[] args = null)
    {
        object result = null;
        if (args != null && args.Length > 0)
        {
            // Parse command line arguments
            var parser = new Parser(settings =>
            {
                settings.CaseInsensitiveEnumValues = true;
                settings.HelpWriter = null; // Suppress default help screen on error
            });

            var verbTypesArray = _assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => t.GetCustomAttribute<VerbAttribute>() != null)
                .ToArray();

            var parseResult = parser.ParseArguments(args, verbTypesArray);

            parseResult.WithParsed<object>(parsed =>
                {
                    // Get the verb type and its properties
                    var verbType = parsed.GetType();
                    var verbAttr = verbType.GetCustomAttribute<VerbAttribute>();
                    var properties = verbType.GetProperties()
                        .Concat(verbType.GetInterfaces()
                            .SelectMany(i => i.GetProperties()))
                        .Where(p => p.GetCustomAttribute<OptionAttribute>() != null)
                        .Distinct()
                        .ToDictionary(
                            p => p.GetCustomAttribute<OptionAttribute>().LongName ?? p.Name,
                            p => p.GetValue(parsed)?.ToString()
                        );

                    // Create and show the form with pre-filled values
                    var form = new MainForm(_exeName, _assemblies, _configuration, _defaultSetterAction);
                    form.PreFillOptions(verbAttr.Name, properties);
                    form.ShowDialog();
                    result = form.GetResult();
                })
                .WithNotParsed(errors =>
                {
                    // Show the GUI without pre-filled options
                    var form = new MainForm(_exeName, _assemblies, _configuration, _defaultSetterAction);
                    form.ShowDialog();
                    result = form.GetResult();
                });
            return result;
        }

        // If no arguments, show the form normally
        var mainForm = new MainForm(_exeName, _assemblies, _configuration, _defaultSetterAction);
        mainForm.ShowDialog();
        return mainForm.GetResult();
    }
}
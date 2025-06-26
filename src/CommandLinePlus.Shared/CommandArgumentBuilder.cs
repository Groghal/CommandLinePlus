using System.Collections;
using System.Reflection;
using CommandLine;

namespace CommandLinePlus.Shared;

/// <summary>
/// Universal command argument builder that converts verb objects to command-line arguments
/// </summary>
public static class CommandArgumentBuilder
{
    /// <summary>
    /// Builds command-line arguments from a verb object
    /// </summary>
    /// <param name="verbObject">The verb object containing property values</param>
    /// <returns>A list of command-line arguments</returns>
    public static List<string> BuildArguments(object verbObject)
    {
        if (verbObject == null)
            throw new ArgumentNullException(nameof(verbObject));

        var args = new List<string>();
        var type = verbObject.GetType();

        // Get verb name
        var verbAttr = type.GetCustomAttribute<VerbAttribute>();
        if (verbAttr != null) args.Add(verbAttr.Name);

        // Process all properties with OptionAttribute
        var properties = type.GetProperties()
            .Concat(type.GetInterfaces().SelectMany(i => i.GetProperties()))
            .Where(p => p.GetCustomAttribute<OptionAttribute>() != null)
            .Distinct();

        foreach (var prop in properties)
        {
            var optAttr = prop.GetCustomAttribute<OptionAttribute>();
            var value = prop.GetValue(verbObject);

            // Skip null values
            if (value == null)
                continue;

            // Skip empty strings
            if (value is string stringValue && string.IsNullOrWhiteSpace(stringValue))
                continue;

            // Skip empty collections
            if (IsEnumerableType(prop.PropertyType) && value is IEnumerable enumerable)
            {
                var hasItems = false;
                foreach (var item in enumerable)
                {
                    hasItems = true;
                    break;
                }

                if (!hasItems)
                    continue;
            }

            // Skip default values if not explicitly set
            if (optAttr.Default != null && value.Equals(optAttr.Default))
                continue;

            // Get the long name - if not specified, convert property name to kebab-case
            var longName = string.IsNullOrEmpty(optAttr.LongName) ? ConvertToKebabCase(prop.Name) : optAttr.LongName;
            var shortName = optAttr.ShortName;

            // Determine which name to use (prefer long name)
            var optionName = !string.IsNullOrEmpty(longName) ? $"--{longName}" :
                !string.IsNullOrEmpty(shortName) ? $"-{shortName}" :
                $"--{ConvertToKebabCase(prop.Name)}";

            // Handle different property types
            if (prop.PropertyType == typeof(bool) || prop.PropertyType == typeof(bool?))
            {
                // Boolean handling
                if (value is bool boolValue && boolValue)
                    // Only add the flag if it's true
                    args.Add(optionName);
                // For false, don't add anything (standard command-line behavior)
            }
            else if (prop.PropertyType.IsEnum || (Nullable.GetUnderlyingType(prop.PropertyType)?.IsEnum ?? false))
            {
                // Enum handling
                args.Add(optionName);
                args.Add(value.ToString().ToLowerInvariant());
            }
            else if (IsEnumerableType(prop.PropertyType) || IsEnumerableType(value.GetType()))
            {
                // Handle IEnumerable types
                var collection = value as IEnumerable;
                var items = new List<string>();

                foreach (var item in collection)
                    if (item != null)
                        items.Add(item.ToString());

                if (items.Any())
                {
                    // Use separator (default to comma if not specified)
                    var separator = optAttr.Separator != default(char) ? optAttr.Separator : ',';
                    args.Add(optionName);
                    args.Add(string.Join(separator.ToString(), items));
                }
            }
            else
            {
                // All other types (string, int, double, etc.)
                args.Add(optionName);
                args.Add(QuoteIfNeeded(value.ToString()));
            }
        }

        // Process Value attributes (positional arguments)
        var valueProperties = type.GetProperties()
            .Where(p => p.GetCustomAttribute<ValueAttribute>() != null)
            .OrderBy(p => p.GetCustomAttribute<ValueAttribute>().Index);

        foreach (var prop in valueProperties)
        {
            var value = prop.GetValue(verbObject);
            if (value != null)
            {
                var stringValue = value.ToString();
                if (!string.IsNullOrWhiteSpace(stringValue)) args.Add(QuoteIfNeeded(stringValue));
            }
        }

        return args;
    }

    /// <summary>
    /// Builds a command-line string from a verb object
    /// </summary>
    /// <param name="verbObject">The verb object containing property values</param>
    /// <returns>A command-line string</returns>
    public static string BuildCommandString(object verbObject)
    {
        var args = BuildArguments(verbObject);
        return string.Join(" ", args);
    }

    /// <summary>
    /// Builds command-line arguments from a verb object, including default values
    /// </summary>
    /// <param name="verbObject">The verb object containing property values</param>
    /// <param name="includeDefaults">Whether to include properties with default values</param>
    /// <returns>A list of command-line arguments</returns>
    public static List<string> BuildArguments(object verbObject, bool includeDefaults)
    {
        if (!includeDefaults)
            return BuildArguments(verbObject);

        if (verbObject == null)
            throw new ArgumentNullException(nameof(verbObject));

        var args = new List<string>();
        var type = verbObject.GetType();

        // Get verb name
        var verbAttr = type.GetCustomAttribute<VerbAttribute>();
        if (verbAttr != null) args.Add(verbAttr.Name);

        // Process all properties with OptionAttribute
        var properties = type.GetProperties()
            .Concat(type.GetInterfaces().SelectMany(i => i.GetProperties()))
            .Where(p => p.GetCustomAttribute<OptionAttribute>() != null)
            .Distinct();

        foreach (var prop in properties)
        {
            var optAttr = prop.GetCustomAttribute<OptionAttribute>();
            var value = prop.GetValue(verbObject);

            // Skip null values
            if (value == null)
                continue;

            // Skip empty strings
            if (value is string stringValue && string.IsNullOrWhiteSpace(stringValue))
                continue;

            // Skip empty collections
            if (IsEnumerableType(prop.PropertyType) && value is IEnumerable enumerable)
            {
                var hasItems = false;
                foreach (var item in enumerable)
                {
                    hasItems = true;
                    break;
                }

                if (!hasItems)
                    continue;
            }

            // Don't skip default values when includeDefaults is true

            // Get the long name - if not specified, convert property name to kebab-case
            var longName = string.IsNullOrEmpty(optAttr.LongName) ? ConvertToKebabCase(prop.Name) : optAttr.LongName;
            var shortName = optAttr.ShortName;

            // Determine which name to use (prefer long name)
            var optionName = !string.IsNullOrEmpty(longName) ? $"--{longName}" :
                !string.IsNullOrEmpty(shortName) ? $"-{shortName}" :
                $"--{ConvertToKebabCase(prop.Name)}";

            // Handle different property types
            if (prop.PropertyType == typeof(bool) || prop.PropertyType == typeof(bool?))
            {
                // Boolean handling
                if (value is bool boolValue && boolValue)
                    // Only add the flag if it's true
                    args.Add(optionName);
                // For false, don't add anything (standard command-line behavior)
            }
            else if (prop.PropertyType.IsEnum || (Nullable.GetUnderlyingType(prop.PropertyType)?.IsEnum ?? false))
            {
                // Enum handling
                args.Add(optionName);
                args.Add(value.ToString().ToLowerInvariant());
            }
            else if (IsEnumerableType(prop.PropertyType) || IsEnumerableType(value.GetType()))
            {
                // Handle IEnumerable types
                var collection = value as IEnumerable;
                var items = new List<string>();

                foreach (var item in collection)
                    if (item != null)
                        items.Add(item.ToString());

                if (items.Any())
                {
                    // Use separator (default to comma if not specified)
                    var separator = optAttr.Separator != default(char) ? optAttr.Separator : ',';
                    args.Add(optionName);
                    args.Add(string.Join(separator.ToString(), items));
                }
            }
            else
            {
                // All other types (string, int, double, etc.)
                args.Add(optionName);
                args.Add(QuoteIfNeeded(value.ToString()));
            }
        }

        // Process Value attributes (positional arguments)
        var valueProperties = type.GetProperties()
            .Where(p => p.GetCustomAttribute<ValueAttribute>() != null)
            .OrderBy(p => p.GetCustomAttribute<ValueAttribute>().Index);

        foreach (var prop in valueProperties)
        {
            var value = prop.GetValue(verbObject);
            if (value != null)
            {
                var stringValue = value.ToString();
                if (!string.IsNullOrWhiteSpace(stringValue)) args.Add(QuoteIfNeeded(stringValue));
            }
        }

        return args;
    }

    /// <summary>
    /// Builds a command-line string from a verb object, including default values
    /// </summary>
    /// <param name="verbObject">The verb object containing property values</param>
    /// <param name="includeDefaults">Whether to include properties with default values</param>
    /// <returns>A command-line string</returns>
    public static string BuildCommandString(object verbObject, bool includeDefaults)
    {
        var args = BuildArguments(verbObject, includeDefaults);
        return string.Join(" ", args);
    }

    private static bool IsEnumerableType(Type type)
    {
        if (type == typeof(string))
            return false;

        return type.GetInterface(typeof(IEnumerable<>).Name) != null;
    }

    private static string QuoteIfNeeded(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        if (value.Contains(" ") || value.Contains("\"") || value.Contains("'"))
            // Escape any existing quotes and wrap in quotes
            return $"\"{value.Replace("\"", "\\\"")}\"";

        return value;
    }

    private static string ConvertToKebabCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // Handle common cases: PascalCase, camelCase
        var result = System.Text.RegularExpressions.Regex.Replace(
            input,
            "(?<!^)(?=[A-Z])",
            "-"
        );

        return result.ToLowerInvariant();
    }
}
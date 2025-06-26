using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CommandLine;

namespace CommandLinePlus.Tests.Shared;

/// <summary>
/// Base class for testing CommandLine verb classes
/// </summary>
public abstract class VerbTestBase
{
    /// <summary>
    /// Override this method to provide the assemblies containing verb classes to test
    /// </summary>
    protected abstract IEnumerable<Assembly> GetAssembliesToTest();

    /// <summary>
    /// Gets all verb classes from the specified assemblies
    /// </summary>
    protected IEnumerable<Type> GetAllVerbClasses()
    {
        var assemblies = GetAssembliesToTest();
        return assemblies.SelectMany(assembly => assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.GetCustomAttribute<VerbAttribute>() != null));
    }

    /// <summary>
    /// Converts a PascalCase or camelCase string to kebab-case
    /// </summary>
    protected static string ConvertToKebabCase(string input)
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
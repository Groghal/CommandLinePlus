using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CommandLine;

namespace CommandLinePlus.Tests.Shared;

/// <summary>
/// Static validator for CommandLine verb classes
/// </summary>
public static class VerbValidator
{
    /// <summary>
    /// Validates that all properties in verb classes are null/default on creation
    /// </summary>
    public static ValidationResult ValidateNullOnCreation(params Assembly[] assemblies)
    {
        var issues = new List<string>();
        var verbClasses = GetVerbClasses(assemblies);

        foreach (var verbClass in verbClasses)
        {
            var instance = Activator.CreateInstance(verbClass);
            var properties = verbClass.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite);

            foreach (var property in properties)
            {
                var value = property.GetValue(instance);

                if (property.PropertyType.IsValueType && Nullable.GetUnderlyingType(property.PropertyType) == null)
                {
                    var defaultValue = Activator.CreateInstance(property.PropertyType);
                    if (!Equals(defaultValue, value))
                        issues.Add($"{verbClass.Name}.{property.Name} is not default value on creation (was: {value})");
                }
                else if (value != null)
                {
                    issues.Add($"{verbClass.Name}.{property.Name} is not null on creation (was: {value})");
                }
            }
        }

        return new ValidationResult(issues);
    }

    /// <summary>
    /// Validates that all public settable properties have Option or Value attributes
    /// </summary>
    public static ValidationResult ValidateAttributesRequired(params Assembly[] assemblies)
    {
        var issues = new List<string>();
        var verbClasses = GetVerbClasses(assemblies);

        foreach (var verbClass in verbClasses)
        {
            var properties = verbClass.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && p.GetSetMethod() != null && p.GetSetMethod().IsPublic);

            foreach (var property in properties)
            {
                var hasOptionAttribute = property.GetCustomAttribute<OptionAttribute>() != null;
                var hasValueAttribute = property.GetCustomAttribute<ValueAttribute>() != null;

                if (!hasOptionAttribute && !hasValueAttribute)
                    issues.Add($"{verbClass.Name}.{property.Name} is missing Option or Value attribute");
            }
        }

        return new ValidationResult(issues);
    }

    /// <summary>
    /// Validates that there are no duplicate option names or value indices
    /// </summary>
    public static ValidationResult ValidateNoDuplicates(params Assembly[] assemblies)
    {
        var issues = new List<string>();
        var verbClasses = GetVerbClasses(assemblies);

        foreach (var verbClass in verbClasses)
        {
            var properties = verbClass.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Check for duplicate short names
            var duplicateShortNames = properties
                .Select(p => new { Property = p, Attr = p.GetCustomAttribute<OptionAttribute>() })
                .Where(x => x.Attr != null && !string.IsNullOrEmpty(x.Attr.ShortName))
                .GroupBy(x => x.Attr.ShortName)
                .Where(g => g.Count() > 1);

            foreach (var group in duplicateShortNames)
            {
                var propNames = string.Join(", ", group.Select(x => x.Property.Name));
                issues.Add($"{verbClass.Name}: Duplicate short name '-{group.Key}' on properties: {propNames}");
            }

            // Check for duplicate long names
            var duplicateLongNames = properties
                .Select(p => new { Property = p, Attr = p.GetCustomAttribute<OptionAttribute>() })
                .Where(x => x.Attr != null)
                .GroupBy(x =>
                    string.IsNullOrEmpty(x.Attr.LongName) ? ConvertToKebabCase(x.Property.Name) : x.Attr.LongName)
                .Where(g => g.Count() > 1);

            foreach (var group in duplicateLongNames)
            {
                var propNames = string.Join(", ", group.Select(x => x.Property.Name));
                issues.Add($"{verbClass.Name}: Duplicate long name '--{group.Key}' on properties: {propNames}");
            }

            // Check for duplicate Value indices
            var duplicateIndices = properties
                .Select(p => new { Property = p, Attr = p.GetCustomAttribute<ValueAttribute>() })
                .Where(x => x.Attr != null)
                .GroupBy(x => x.Attr.Index)
                .Where(g => g.Count() > 1);

            foreach (var group in duplicateIndices)
            {
                var propNames = string.Join(", ", group.Select(x => x.Property.Name));
                issues.Add($"{verbClass.Name}: Duplicate Value index '{group.Key}' on properties: {propNames}");
            }
        }

        return new ValidationResult(issues);
    }

    /// <summary>
    /// Runs all validations and returns a combined result
    /// </summary>
    public static ValidationResult ValidateAll(params Assembly[] assemblies)
    {
        var allIssues = new List<string>();

        allIssues.AddRange(ValidateAttributesRequired(assemblies).Issues);
        allIssues.AddRange(ValidateNoDuplicates(assemblies).Issues);
        allIssues.AddRange(ValidateNullOnCreation(assemblies).Issues);

        return new ValidationResult(allIssues);
    }

    private static IEnumerable<Type> GetVerbClasses(IEnumerable<Assembly> assemblies)
    {
        return assemblies.SelectMany(assembly => assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.GetCustomAttribute<VerbAttribute>() != null));
    }

    private static string ConvertToKebabCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var result = System.Text.RegularExpressions.Regex.Replace(
            input,
            "(?<!^)(?=[A-Z])",
            "-"
        );

        return result.ToLowerInvariant();
    }
}

/// <summary>
/// Result of verb validation
/// </summary>
public class ValidationResult
{
    public IReadOnlyList<string> Issues { get; }
    public bool IsValid => !Issues.Any();

    public ValidationResult(IEnumerable<string> issues)
    {
        Issues = issues.ToList().AsReadOnly();
    }

    public override string ToString()
    {
        if (IsValid)
            return "All validations passed";

        return $"Validation failed with {Issues.Count} issue(s):\n" + string.Join("\n", Issues);
    }
}
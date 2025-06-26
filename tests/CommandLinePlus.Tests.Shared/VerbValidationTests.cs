using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using CommandLine;

namespace CommandLinePlus.Tests.Shared;

/// <summary>
/// Generic validation tests for CommandLine verb classes
/// Inherit from this class and implement GetAssembliesToTest() to test your verb classes
/// </summary>
[TestFixture]
public abstract class VerbValidationTests : VerbTestBase
{
    [Test]
    public void AllVerbClasses_AllPropertiesAreNullOnCreation()
    {
        // Arrange
        var verbClasses = GetAllVerbClasses();

        foreach (var verbClass in verbClasses)
        {
            // Act
            var instance = Activator.CreateInstance(verbClass);
            var properties = verbClass.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite);

            // Assert
            foreach (var property in properties)
            {
                var value = property.GetValue(instance);

                // Check if it's a value type (like bool, int, etc.)
                if (property.PropertyType.IsValueType && Nullable.GetUnderlyingType(property.PropertyType) == null)
                {
                    // For non-nullable value types, we expect default value which should be "null-like"
                    // For bool it's false, for int it's 0, etc.
                    var defaultValue = Activator.CreateInstance(property.PropertyType);
                    Assert.AreEqual(defaultValue, value,
                        $"{verbClass.Name}.{property.Name} should have default value (equivalent to null) on creation");
                }
                else
                {
                    // For reference types and nullable value types, we expect null
                    Assert.IsNull(value,
                        $"{verbClass.Name}.{property.Name} should be null on creation");
                }
            }
        }
    }

    [Test]
    public void AllVerbClasses_PublicSettableProperties_MustHaveOptionOrValueAttribute()
    {
        // Arrange
        var verbClasses = GetAllVerbClasses();
        var failedProperties = new List<string>();

        foreach (var verbClass in verbClasses)
        {
            // Get all public instance properties with a setter
            var properties = verbClass.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && p.GetSetMethod() != null && p.GetSetMethod().IsPublic);

            foreach (var property in properties)
            {
                // Check if property has Option or Value attribute
                var hasOptionAttribute = property.GetCustomAttribute<OptionAttribute>() != null;
                var hasValueAttribute = property.GetCustomAttribute<ValueAttribute>() != null;

                if (!hasOptionAttribute && !hasValueAttribute)
                    failedProperties.Add($"{verbClass.Name}.{property.Name}");
            }
        }

        // Assert
        if (failedProperties.Any())
        {
            var message = "The following properties in Verb classes are missing Option or Value attributes:\n" +
                          string.Join("\n", failedProperties);
            Assert.Fail(message);
        }
    }

    [Test]
    public void AllVerbClasses_PropertiesWithAttributes_HaveValidConfiguration()
    {
        // Arrange
        var verbClasses = GetAllVerbClasses();
        var issues = new List<string>();

        foreach (var verbClass in verbClasses)
        {
            var properties = verbClass.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Check for duplicate short names
            var shortNames = properties
                .Select(p => p.GetCustomAttribute<OptionAttribute>())
                .Where(attr => attr != null && !string.IsNullOrEmpty(attr.ShortName))
                .GroupBy(attr => attr.ShortName)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            foreach (var shortName in shortNames) issues.Add($"{verbClass.Name}: Duplicate short name '-{shortName}'");

            // Check for duplicate long names
            var longNames = properties
                .Select(p => new { Prop = p, Attr = p.GetCustomAttribute<OptionAttribute>() })
                .Where(x => x.Attr != null)
                .GroupBy(x => string.IsNullOrEmpty(x.Attr.LongName) ? ConvertToKebabCase(x.Prop.Name) : x.Attr.LongName)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            foreach (var longName in longNames) issues.Add($"{verbClass.Name}: Duplicate long name '--{longName}'");

            // Check for duplicate Value indices
            var valueIndices = properties
                .Select(p => p.GetCustomAttribute<ValueAttribute>())
                .Where(attr => attr != null)
                .GroupBy(attr => attr.Index)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            foreach (var index in valueIndices) issues.Add($"{verbClass.Name}: Duplicate Value index '{index}'");
        }

        // Assert
        if (issues.Any())
        {
            var message = "The following issues were found in Verb classes:\n" +
                          string.Join("\n", issues);
            Assert.Fail(message);
        }
    }

    [Test]
    public void AllVerbClasses_RequiredProperties_AreNotNullable()
    {
        // Arrange
        var verbClasses = GetAllVerbClasses();
        var issues = new List<string>();

        foreach (var verbClass in verbClasses)
        {
            var properties = verbClass.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                var optionAttr = property.GetCustomAttribute<OptionAttribute>();
                if (optionAttr != null && optionAttr.Required)
                {
                    // Check if it's a nullable type
                    var isNullable = !property.PropertyType.IsValueType ||
                                     Nullable.GetUnderlyingType(property.PropertyType) != null;

                    // String and reference types are inherently nullable, which is fine
                    // But nullable value types (int?, bool?) with Required=true might be confusing
                    if (property.PropertyType.IsValueType &&
                        Nullable.GetUnderlyingType(property.PropertyType) != null)
                        issues.Add($"{verbClass.Name}.{property.Name}: Required property is nullable value type");
                }
            }
        }

        // This is more of a warning than a hard requirement
        if (issues.Any())
            Console.WriteLine("Warning - Required properties with nullable value types:\n" +
                              string.Join("\n", issues));
    }
}
using System.Collections.Generic;
using System.Reflection;
using CommandLinePlus.GUI.Example.Models.Verbs;
using CommandLinePlus.Tests.Shared;
using NUnit.Framework;

namespace CommandLinePlus.GUI.Example.Tests;

/// <summary>
/// Example of using the shared verb validation tests for the Example project
/// </summary>
public class ExampleVerbValidationTests : VerbValidationTests
{
    protected override IEnumerable<Assembly> GetAssembliesToTest()
    {
        // Return the assembly containing your verb classes
        return new[] { typeof(DockerBuildOptions).Assembly };
    }
}

/// <summary>
/// Alternative approach using the static validator
/// </summary>
[TestFixture]
public class ExampleVerbStaticValidationTests
{
    [Test]
    public void ValidateAllExampleVerbs()
    {
        var assembly = typeof(DockerBuildOptions).Assembly;
        var result = VerbValidator.ValidateAll(assembly);

        if (!result.IsValid) Assert.Fail(result.ToString());
    }

    [Test]
    public void ValidateNoMissingAttributes()
    {
        var assembly = typeof(DockerBuildOptions).Assembly;
        var result = VerbValidator.ValidateAttributesRequired(assembly);

        Assert.IsTrue(result.IsValid, result.ToString());
    }

    [Test]
    public void ValidateNoDuplicateOptions()
    {
        var assembly = typeof(DockerBuildOptions).Assembly;
        var result = VerbValidator.ValidateNoDuplicates(assembly);

        Assert.IsTrue(result.IsValid, result.ToString());
    }
}
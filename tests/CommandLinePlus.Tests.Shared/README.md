# CommandLinePlus.Tests.Shared

Shared test utilities for validating CommandLineParser verb classes.

## Features

This library provides comprehensive validation for CommandLineParser verb classes:

1. **Attribute Validation** - Ensures all public properties have `[Option]` or `[Value]` attributes
2. **Duplicate Detection** - Detects duplicate option names and value indices
3. **Initialization Testing** - Verifies all properties are null/default on creation
4. **Configuration Validation** - Checks for common configuration issues

## Usage

### Option 1: Inherit from VerbValidationTests

Create a test class that inherits from `VerbValidationTests`:

```csharp
using CommandLinePlus.Tests.Shared;

[TestFixture]
public class MyVerbTests : VerbValidationTests
{
    protected override IEnumerable<Assembly> GetAssembliesToTest()
    {
        return new[] { typeof(MyVerbClass).Assembly };
    }
}
```

This will automatically run all validation tests on your verb classes.

### Option 2: Use VerbValidator Static Methods

For more control, use the `VerbValidator` static class:

```csharp
[Test]
public void ValidateMyVerbs()
{
    var result = VerbValidator.ValidateAll(typeof(MyVerbClass).Assembly);
    
    if (!result.IsValid)
    {
        Assert.Fail(result.ToString());
    }
}
```

### Option 3: Individual Validations

Run specific validations:

```csharp
// Check for missing attributes
var attrResult = VerbValidator.ValidateAttributesRequired(assembly);

// Check for duplicates
var dupResult = VerbValidator.ValidateNoDuplicates(assembly);

// Check initialization
var initResult = VerbValidator.ValidateNullOnCreation(assembly);
```

## Validation Rules

### 1. Attribute Requirements

- All public settable properties must have either `[Option]` or `[Value]` attribute
- Properties without these attributes will fail validation

### 2. No Duplicates

- No duplicate short names (e.g., two properties with `-f`)
- No duplicate long names (e.g., two properties with `--file`)
- Properties without explicit long names use kebab-case property names
- No duplicate value indices

### 3. Null on Creation

- All properties should be null/default when a verb instance is created
- Non-nullable value types should have their default value (0, false, etc.)
- Reference types and nullable value types should be null

## Example Verb Class

```csharp
[Verb("add", HelpText = "Add a file")]
public class AddOptions
{
    [Option('f', "file", Required = true, HelpText = "File to add")]
    public string FileName { get; set; }
    
    [Option("force", HelpText = "Force add")]
    public bool Force { get; set; }
    
    [Value(0, HelpText = "Additional files")]
    public IEnumerable<string> Files { get; set; }
}
```

## Integration with CI/CD

Add these tests to your test suite to catch configuration issues early:

```yaml
- task: DotNetCoreCLI@2
  inputs:
    command: 'test'
    projects: '**/*Tests.csproj'
```

## NUnit Integration

The library uses NUnit for test assertions. Make sure your test project references:

- NUnit
- NUnit.TestAdapter
- Microsoft.NET.Test.Sdk
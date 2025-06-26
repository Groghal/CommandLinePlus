# GuiLine

GuiLine is a Windows Forms GUI generator for [CommandLineParser](https://github.com/commandlineparser/commandline). It automatically creates user-friendly graphical interfaces for your command-line applications.

## Features

- 🎨 Automatic GUI generation from CommandLineParser verb classes
- 📝 Support for all CommandLineParser option types
- 🎯 Pre-population from command-line arguments
- 🎨 Customizable themes and appearance
- 📦 Easy integration via NuGet
- 🔄 Drag-and-drop support for file/folder inputs
- ✅ Built-in validation
- 🔴 Visual marking for required fields with customizable colors

## Installation

```bash
dotnet add package GuiLine
```

## Quick Start

### Simple Usage

```csharp
using CommandLine;
using GuiLine;

[Verb("deploy", HelpText = "Deploy application")]
public class DeployOptions
{
    [Option('e', "environment", Required = true, HelpText = "Target environment")]
    public string Environment { get; set; }

    [Option('v', "version", Required = true, HelpText = "Version to deploy")]
    public string Version { get; set; }

    [Option("force", Default = false, HelpText = "Force deployment")]
    public bool Force { get; set; }
}

class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        // Show GUI and get parsed options
        var options = GuiLine.Show<DeployOptions>(args);
        
        if (options != null)
        {
            // User clicked Run - use the parsed options
            Console.WriteLine($"Deploying {options.Version} to {options.Environment}");
            if (options.Force) Console.WriteLine("Force mode enabled");
        }
    }
}
```

### Multiple Verbs

```csharp
// Define multiple verb classes
[Verb("add", HelpText = "Add a new item")]
public class AddOptions { /* ... */ }

[Verb("remove", HelpText = "Remove an item")]
public class RemoveOptions { /* ... */ }

[Verb("list", HelpText = "List all items")]
public class ListOptions { /* ... */ }

// Show GUI with verb selection
var result = GuiLine.Show(
    new[] { typeof(Program).Assembly }, 
    args
);

// Handle the result based on its type
switch (result)
{
    case AddOptions add:
        // Handle add command
        break;
    case RemoveOptions remove:
        // Handle remove command
        break;
    case ListOptions list:
        // Handle list command
        break;
}
```

### Custom Configuration

```csharp
var config = new GuiLineConfiguration
{
    ExecutableName = "myapp",
    WindowTitle = "My Application Command Builder",
    WindowWidth = 900,
    WindowHeight = 600,
    ShowCommandPreview = true,
    EnableDragDrop = true,
    SortOrder = ControlSortOrder.ByTypeThenName, // Control sorting
    ShowCheckAllButtons = true, // Show check/uncheck all buttons for enum lists
    AlwaysOnTop = false, // Window stays on top of other windows
    Theme = new GuiLineTheme
    {
        BackgroundColor = Color.WhiteSmoke,
        TextColor = Color.DarkBlue,
        LabelFont = new Font("Segoe UI", 10),
        InputFont = new Font("Consolas", 10),
        // Required field styling
        RequiredLabelColor = Color.DarkRed,
        ShowRequiredAsterisk = true,
        UseRequiredFieldBoldFont = true,
        RequiredFieldBackgroundColor = Color.FromArgb(255, 255, 240, 240)
    }
};

var options = GuiLine.Show<MyOptions>(args, config);
```

### Control Sorting Options

GuiLine supports multiple sorting modes for generated controls:

1. **SourceCode** (default) - Maintains the order properties are defined in your class
2. **ByName** - Sorts alphabetically by property name
3. **ByTypeThenName** - Groups by type, then sorts by name within each group
4. **RequiredFirst** - Shows required options first, maintaining source order within each group
5. **RequiredFirstThenName** - Shows required options first, then sorts alphabetically
6. **RequiredFirstThenType** - Shows required options first, then groups by type and sorts by name

```csharp
// Keep source code order
config.SortOrder = ControlSortOrder.SourceCode;

// Sort alphabetically
config.SortOrder = ControlSortOrder.ByName;

// Group by type, then alphabetically
config.SortOrder = ControlSortOrder.ByTypeThenName;

// Required fields first (recommended for better UX)
config.SortOrder = ControlSortOrder.RequiredFirst;

// Required first, then alphabetical
config.SortOrder = ControlSortOrder.RequiredFirstThenName;

// Required first, then by type and name
config.SortOrder = ControlSortOrder.RequiredFirstThenType;
```

When using type-based sorting, controls are grouped in this order:
1. Booleans (checkboxes)
2. Enums (dropdowns)
3. Integer types (int, long, short, byte)
4. Floating-point types (double, float, decimal)
5. Strings (text inputs)
6. IEnumerable<Enum> (checked list boxes)
7. Other IEnumerable types
8. Everything else

### Search and Filter Options

GuiLine includes a powerful search and filter panel to help users quickly find options:

```csharp
// Enable search/filter panel (default: true)
config.ShowSearchFilter = true;
```

**Features:**
- **Comprehensive Search**: Type to search across:
  - Option names and long names
  - Help text and descriptions
  - Current values in input fields
  - All available options in dropdowns and checklists
  - Enum values (even if not selected)
- **Filter Options**:
  - All - Show all options
  - Required Only - Show only required options
  - Optional Only - Show only optional options
  - With Values - Show options that have values entered
  - Empty - Show options without values
- **Keyboard Shortcuts**:
  - `Ctrl+F` - Focus search box
  - `Escape` - Clear search (when search box is focused)
- **Clear Button**: One-click reset of search and filters
- **Always on Top**: Checkbox to keep the window above all other windows

### Window Management

GuiLine includes convenient window management features:

```csharp
// Set initial always-on-top state
config.AlwaysOnTop = true; // Window starts on top
```

The "Always on Top" checkbox in the search panel allows users to toggle this behavior at runtime, keeping the GuiLine window visible while working with other applications.

### Visual Styling for Required Fields

GuiLine provides comprehensive visual styling options to make required fields stand out:

```csharp
var config = new GuiLineConfiguration
{
    Theme = new GuiLineTheme
    {
        // Label styling for required fields
        RequiredLabelColor = Color.DarkRed,           // Color for required field labels
        ShowRequiredAsterisk = true,                  // Show asterisk (*) before required labels
        UseRequiredFieldBoldFont = true,              // Use bold font for required labels
        
        // Input field styling for required fields
        RequiredFieldBackgroundColor = Color.FromArgb(255, 255, 240, 240), // Light red tint
        RequiredFieldBorderColor = Color.FromArgb(255, 200, 0, 0)          // Dark red border
    }
};
```

**Features:**
- **Required Field Labels**: Displayed in a different color (default: dark red) with optional bold font
- **Asterisk Indicator**: Optional asterisk (*) prefix for required field labels
- **Input Field Highlighting**: Required input fields have a distinct background color
- **Visual Consistency**: All required field types (text boxes, dropdowns, checkboxes) receive the same visual treatment
- **Easy Identification**: Users can quickly identify which fields must be filled before running the command

### Check All/Uncheck All Buttons

For `IEnumerable<Enum>` properties, GuiLine provides convenient "Check All" and "Uncheck All" buttons:

```csharp
// Enable check all buttons (default: true)
config.ShowCheckAllButtons = true;

// Example enum collection property
[Option("capabilities", HelpText = "Linux capabilities to add")]
public IEnumerable<Capability> Capabilities { get; set; }
```

When enabled, users can quickly:
- Select all enum values with "Check All"
- Clear all selections with "Uncheck All"
- Still individually check/uncheck specific items

## Path Type Support

GuiLine intelligently handles file and directory selection with a multi-level fallback system:

### 1. Custom PathType Attribute (Highest Priority)

```csharp
[Option('f', "file", HelpText = "Configuration file")]
[PathType(PathType.File, Filter = "Config Files|*.json;*.xml|All Files|*.*")]
public string ConfigFile { get; set; }

[Option('d', "output-dir", HelpText = "Output directory")]
[PathType(PathType.Directory)]
public string OutputDirectory { get; set; }

[Option('p', "path", HelpText = "File or directory path")]
[PathType(PathType.Any)]  // Shows context menu with both options
public string Path { get; set; }
```

### 2. Automatic Detection from Property Names

If no `PathType` attribute is specified, GuiLine detects the type from property/option names:

**Directory Detection** (shows folder browser):
- Names containing: `folder`, `directory`, `dir`, `path`, `location`, `root`, `workspace`

**File Detection** (shows file browser):
- Names containing: `file`, `filename`, `document`, `script`, `executable`, `dll`, `config`

### 3. Context Menu Fallback

For ambiguous properties, a context menu appears with both "Select File..." and "Select Folder..." options.

## Supported Option Types

GuiLine supports all CommandLineParser option types:

- ✅ `string` - Text input
- ✅ `int`, `double`, `float` - Numeric inputs
- ✅ `bool` - Checkboxes
- ✅ `enum` - Dropdown lists
- ✅ `IEnumerable<T>` - Comma-separated values
- ✅ File/folder paths - With browse buttons and drag-drop
- ✅ Custom types with `TypeConverter`
- ✅ Nullable types (`int?`, `bool?`, `MyEnum?`, etc.)
- ✅ Nullable collections (`IEnumerable<MyEnum>?`)

### Nullable Type Support

GuiLine provides comprehensive support for nullable types:

**Simple Nullable Types:**
- `bool?` - Three-state checkbox (checked/unchecked/indeterminate)
- `int?`, `double?`, etc. - Empty text box represents null
- `MyEnum?` - Dropdown with `<none>` option for null

**Nullable Enum Collections:**
```csharp
[Option("features", HelpText = "Optional features to enable")]
public IEnumerable<Feature>? Features { get; set; }
```

For nullable `IEnumerable<Enum>`, GuiLine displays:
- A checkbox labeled "Set values (uncheck for null)"
- The enum checklist (enabled only when checkbox is checked)
- When unchecked, the property is set to null (not empty collection)

This allows distinguishing between:
- `null` - Feature not specified at all
- Empty collection - Feature explicitly set to none
- Collection with values - Specific features selected

## Setting Default Values

```csharp
// Set defaults for specific options
GuiLine.SetDefault("deploy", "environment", "production");
GuiLine.SetDefault("deploy", "force", true);

// Clear defaults
GuiLine.ClearDefaults("deploy"); // Clear for specific verb
GuiLine.ClearDefaults(); // Clear all defaults
```

## Integration with Existing CLI Apps

GuiLine is designed to complement your existing CLI applications:

```csharp
static void Main(string[] args)
{
    // If --gui flag is present, show GUI
    if (args.Contains("--gui"))
    {
        args = args.Where(a => a != "--gui").ToArray();
        var options = GuiLine.Show<Options>(args);
        if (options == null) return; // User cancelled
        
        // Convert back to args for existing parser
        args = ConvertToArgs(options);
    }
    
    // Your existing CommandLineParser code
    Parser.Default.ParseArguments<Options>(args)
        .WithParsed(RunApplication)
        .WithNotParsed(HandleErrors);
}
```

## Advanced Features

### Pre-population from Arguments

```csharp
// Pass existing arguments to pre-fill the form
string[] existingArgs = { "deploy", "--environment", "staging", "--version", "1.2.3" };
var options = GuiLine.Show<DeployOptions>(existingArgs);
```

### Validation

GuiLine automatically validates based on CommandLineParser attributes:
- Required options must be filled
- Numeric fields only accept valid numbers
- Enum fields show valid values in dropdowns

### File and Folder Selection

Options with common file/folder names automatically get browse buttons:
- Names containing: file, path, folder, directory
- Drag-and-drop support for easy file selection

## License

MIT License - see LICENSE file for details

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Support

For issues and feature requests, please use the [GitHub issue tracker](https://github.com/yourusername/GuiLine/issues).
using System.Diagnostics;
using System.Reflection;
using CommandLine;
using CommandLinePlus.GUI.Models;
using CommandLinePlus.Shared;

namespace CommandLinePlus.GUI;

public partial class MainForm : Form
{
    private readonly string _exeName;
    private readonly IEnumerable<Assembly> _assemblies;
    private ComboBox _verbSelector;
    private Panel _optionsPanel;
    private Button _runButton;
    private Dictionary<string, Type> _verbTypes;
    private Dictionary<string, (PropertyInfo Prop, Control Input)> _optionInputs;
    private TextBox _workingDirInput;
    private Panel _workingDirDropZone;
    private bool _showControlEdges = false; // Default to true
    private Label _verbDescriptionLabel;
    private object _result = null;
    private string _commandString = null;
    private GuiConfiguration _configuration;
    private TextBox _searchBox;
    private ComboBox _filterCombo;
    private Panel _searchPanel;
    private Dictionary<string, (Control Label, Control Input, Control Container)> _allControls;
    private Action<IDefaultSetter> _defaultSetterAction;

    public MainForm(string exeName, IEnumerable<Assembly> assemblies, GuiConfiguration configuration = null,
        Action<IDefaultSetter> defaultSetterAction = null)
    {
        _exeName = exeName;
        _assemblies = assemblies;
        _configuration = configuration ?? new GuiConfiguration();
        _showControlEdges = _configuration.ShowControlBorders;
        _defaultSetterAction = defaultSetterAction;
        InitializeDynamicUi();
    }

    private BorderStyle GetBorderStyle()
    {
        return _showControlEdges ? BorderStyle.FixedSingle : BorderStyle.None;
    }

    private int _reservedHeight = 0;

    private void InitializeDynamicUi()
    {
        Text = _configuration.WindowTitle;
        Width = _configuration.WindowWidth;
        Height = _configuration.WindowHeight;
        MinimumSize = new Size(500, 400); // Set minimum form size
        TopMost = _configuration.AlwaysOnTop;

        // Create a panel for verb selector and working directory (Group 2)
        var verbPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 120, // Increased to accommodate description
            Padding = new Padding(10),
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(0, 0, 0, 5) // Add bottom margin for spacing
        };
        _reservedHeight = verbPanel.Height + 50; // Include search panel and spacing

        const int dropZoneWidth = 160;
        const int browseButtonWidth = 30;
        const int rightPadding = 10;
        const int controlSpacing = 5;

        // Verb selector label and combo
        var verbLabel = new Label
        {
            Text = "Verb:",
            AutoSize = true,
            Location = new Point(0, 8)
        };

        _verbSelector = new ComboBox
        {
            Left = 50,
            Top = 5,
            Width = 300,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Height = 25
        };

        // Working Directory Controls
        var workingDirLabel = new Label
        {
            Text = "Working Directory:",
            AutoSize = true,
            Location = new Point(0, 38)
        };

        var workingDirInputPanel = new Panel
        {
            Left = 120,
            Top = 35,
            Width = verbPanel.Width - (120 + rightPadding + 20), // Account for panel padding
            Height = 24,
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
            BorderStyle = BorderStyle.None // No border for inner panel
        };

        _workingDirInput = new TextBox
        {
            Left = 0,
            Top = 0,
            Width = workingDirInputPanel.Width - (dropZoneWidth + browseButtonWidth + controlSpacing * 2),
            MinimumSize = new Size(0, 28),
            MaximumSize = new Size(0, 28),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
            Margin = new Padding(0, 0, controlSpacing, 0),
            BorderStyle = GetBorderStyle()
        };
        _workingDirInput.Text = Directory.GetCurrentDirectory();

        var workingDirBrowseButton = new Button
        {
            Text = "...",
            Width = browseButtonWidth,
            Dock = DockStyle.Right,
            Height = 24,
            FlatStyle = _showControlEdges ? FlatStyle.Standard : FlatStyle.System
        };

        workingDirBrowseButton.Click += (s, e) =>
        {
            using var dialog = new FolderBrowserDialog
            {
                SelectedPath = _workingDirInput.Text
            };

            if (dialog.ShowDialog() == DialogResult.OK) _workingDirInput.Text = dialog.SelectedPath;
        };

        _workingDirDropZone = new Panel
        {
            Width = dropZoneWidth,
            Height = 24,
            Dock = DockStyle.Right,
            BorderStyle = BorderStyle.FixedSingle,
            AllowDrop = true,
            Tag = _workingDirInput
        };

        _workingDirDropZone.DragEnter += (s, e) =>
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        };

        _workingDirDropZone.DragDrop += (s, e) =>
        {
            // Preserve scroll position
            var scrollPosition = _optionsPanel?.AutoScrollPosition ?? Point.Empty;

            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                var path = files[0];
                if (Directory.Exists(path))
                    _workingDirInput.Text = path;
                else if (File.Exists(path)) _workingDirInput.Text = Path.GetDirectoryName(path);
            }

            // Restore scroll position
            if (_optionsPanel != null && scrollPosition != Point.Empty)
                _optionsPanel.AutoScrollPosition = new Point(-scrollPosition.X, -scrollPosition.Y);
        };

        var dropLabel = new Label
        {
            Text = "Drop folder here",
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill
        };
        _workingDirDropZone.Controls.Add(dropLabel);

        // Add controls in the correct order (right to left)
        workingDirInputPanel.Controls.Add(workingDirBrowseButton);
        workingDirInputPanel.Controls.Add(_workingDirDropZone);
        workingDirInputPanel.Controls.Add(_workingDirInput);

        // Verb description label
        _verbDescriptionLabel = new Label
        {
            Left = 0,
            Top = 65,
            Width = verbPanel.Width - 20, // Account for panel padding
            Height = 40,
            AutoSize = false,
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
            BorderStyle = BorderStyle.None
        };

        // Add controls to verb panel
        verbPanel.Controls.Add(verbLabel);
        verbPanel.Controls.Add(_verbSelector);
        verbPanel.Controls.Add(_verbDescriptionLabel);
        verbPanel.Controls.Add(workingDirLabel);
        verbPanel.Controls.Add(workingDirInputPanel);


        // Create search/filter panel (Group 1)
        _searchPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 35,
            Padding = new Padding(10, 5, 10, 5),
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(0, 0, 0, 5) // Add bottom margin for spacing
        };

        _searchBox = new TextBox
        {
            Left = 10,
            Top = 7,
            Width = 200,
            Height = 21,
            PlaceholderText = "Search options..."
        };

        _filterCombo = new ComboBox
        {
            Left = 220,
            Top = 7,
            Width = 150,
            Height = 21,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _filterCombo.Items.AddRange(new[] { "All", "Required Only", "Optional Only", "With Values", "Empty" });
        _filterCombo.SelectedIndex = 0;

        var clearButton = new Button
        {
            Text = "Clear",
            Left = 380,
            Top = 7,
            Width = 60,
            Height = 21
        };
        clearButton.Click += (s, e) =>
        {
            _searchBox.Text = "";
            _filterCombo.SelectedIndex = 0;
        };

        var alwaysOnTopCheckBox = new CheckBox
        {
            Text = "Always on Top",
            Left = 450,
            Top = 9,
            Width = 110,
            AutoSize = true,
            Checked = _configuration.AlwaysOnTop
        };
        alwaysOnTopCheckBox.CheckedChanged += (s, e) => { TopMost = alwaysOnTopCheckBox.Checked; };

        _searchPanel.Controls.Add(_searchBox);
        _searchPanel.Controls.Add(_filterCombo);
        _searchPanel.Controls.Add(clearButton);
        _searchPanel.Controls.Add(alwaysOnTopCheckBox);
        _searchPanel.Visible = false; // Hidden until a verb is selected

        _optionsPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(10),
            BorderStyle = BorderStyle.FixedSingle // Group 3 border
        };

        // Create a panel for the run button
        var buttonPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 40,
            Padding = new Padding(5)
        };

        _runButton = new Button
        {
            Text = "Run",
            Dock = DockStyle.Fill,
            FlatStyle = _showControlEdges ? FlatStyle.Standard : FlatStyle.System
        };

        buttonPanel.Controls.Add(_runButton);

        // Create a main container panel to hold all panels with proper spacing
        var mainContainer = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(5) // This creates spacing around all panels
        };

        // Create an inner container for the three main panels
        var panelsContainer = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0) // Remove padding, we'll handle spacing differently
        };

        // Create spacer panels for vertical spacing
        var spacer1 = new Panel { Dock = DockStyle.Top, Height = 5 };
        var spacer2 = new Panel { Dock = DockStyle.Top, Height = 5 };

        // Add panels to the panels container with spacers
        panelsContainer.Controls.Add(_optionsPanel); // Fill remaining space
        panelsContainer.Controls.Add(spacer2); // Spacer between verb and options
        panelsContainer.Controls.Add(verbPanel); // Group 2
        panelsContainer.Controls.Add(spacer1); // Spacer between search and verb
        panelsContainer.Controls.Add(_searchPanel); // Group 1 (top)

        // Add the panels container to main container
        mainContainer.Controls.Add(panelsContainer);

        Controls.Add(mainContainer);
        Controls.Add(buttonPanel);

        _runButton.Click += OnRunClicked;
        _verbSelector.SelectedIndexChanged += OnVerbChanged;
        _searchBox.TextChanged += OnSearchFilterChanged;
        _filterCombo.SelectedIndexChanged += OnSearchFilterChanged;

        // Add keyboard shortcuts
        KeyPreview = true;
        KeyDown += (s, e) =>
        {
            if (e.Control && e.KeyCode == Keys.F)
            {
                if (_searchPanel.Visible)
                {
                    _searchBox.Focus();
                    _searchBox.SelectAll();
                    e.Handled = true;
                }
            }
            else if (e.KeyCode == Keys.Escape && _searchBox.Focused)
            {
                _searchBox.Text = "";
                _filterCombo.SelectedIndex = 0;
                e.Handled = true;
            }
        };

        LoadVerbs();
    }

    private void LoadVerbs()
    {
        _verbTypes = new Dictionary<string, Type>();

        var types = _assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.GetCustomAttribute<VerbAttribute>() != null);

        foreach (var type in types)
        {
            var verbAttr = type.GetCustomAttribute<VerbAttribute>();
            _verbTypes[verbAttr.Name] = type;
            _verbSelector.Items.Add(verbAttr.Name);
        }

        if (_verbSelector.Items.Count > 0)
            _verbSelector.SelectedIndex = 0;
    }

    private void OnVerbChanged(object sender, EventArgs e)
    {
        if (_verbSelector.SelectedItem == null) return;
        var selectedVerb = _verbSelector.SelectedItem.ToString();
        if (!_verbTypes.ContainsKey(selectedVerb)) return;

        var verbType = _verbTypes[selectedVerb];
        var verbAttr = verbType.GetCustomAttribute<VerbAttribute>();
        _verbDescriptionLabel.Text = verbAttr?.HelpText ?? "";
        BuildOptionsUi(verbType);
    }

    private void BuildOptionsUi(Type verbType)
    {
        _optionsPanel.Controls.Clear();
        _optionInputs = new Dictionary<string, (PropertyInfo, Control)>();
        _allControls = new Dictionary<string, (Control Label, Control Input, Control Container)>();
        _searchPanel.Visible = _configuration.ShowSearchFilter;
        _searchBox.Text = "";
        _filterCombo.SelectedIndex = 0;

        var verbAttr = verbType.GetCustomAttribute<VerbAttribute>();
        var verbName = verbAttr.Name;

        // Create an instance of the verb class to get default property values
        object verbInstance = null;
        try
        {
            verbInstance = Activator.CreateInstance(verbType);

            // If the verb implements IDefaultSetter, call the action if provided
            if (verbInstance is IDefaultSetter defaultSetter)
            {
                // First call the instance's UpdateDefaults method
                defaultSetter.UpdateDefaults();

                // Then call the external action if provided
                _defaultSetterAction?.Invoke(defaultSetter);
            }
        }
        catch
        {
        }

        // Get all properties including those from interfaces
        var options = verbType.GetProperties()
            .Concat(verbType.GetInterfaces()
                .SelectMany(i => i.GetProperties()))
            .Where(p => p.GetCustomAttribute<OptionAttribute>() != null)
            .Distinct();

        // Apply sorting based on configuration
        var sortedOptions = _configuration.SortOrder switch
        {
            ControlSortOrder.ByName => options
                .OrderBy(p => p.GetCustomAttribute<OptionAttribute>()?.LongName ?? p.Name),

            ControlSortOrder.ByTypeThenName => options
                .OrderBy(p => GetPropertyTypeSortOrder(p.PropertyType))
                .ThenBy(p => p.GetCustomAttribute<OptionAttribute>()?.LongName ?? p.Name),

            ControlSortOrder.RequiredFirst => options
                .OrderByDescending(p => p.GetCustomAttribute<OptionAttribute>()?.Required ?? false),

            ControlSortOrder.RequiredFirstThenName => options
                .OrderByDescending(p => p.GetCustomAttribute<OptionAttribute>()?.Required ?? false)
                .ThenBy(p => p.GetCustomAttribute<OptionAttribute>()?.LongName ?? p.Name),

            ControlSortOrder.RequiredFirstThenType => options
                .OrderByDescending(p => p.GetCustomAttribute<OptionAttribute>()?.Required ?? false)
                .ThenBy(p => GetPropertyTypeSortOrder(p.PropertyType))
                .ThenBy(p => p.GetCustomAttribute<OptionAttribute>()?.LongName ?? p.Name),

            _ => options // SourceCode - keep original order
        };

        //var y = _reservedHeight + 10;
        var y = 5;
        foreach (var prop in sortedOptions)
        {
            var optAttr = prop.GetCustomAttribute<OptionAttribute>();
            // Get the enum type (either directly or from IEnumerable<T>)
            Type enumType = null;
            var isEnumerableEnum = false;
            var underlyingType = GetUnderlyingType(prop.PropertyType);

            if (underlyingType.IsEnum)
            {
                enumType = underlyingType;
            }
            else if (prop.PropertyType.IsGenericType &&
                     prop.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
                     prop.PropertyType.GetGenericArguments()[0].IsEnum)
            {
                enumType = prop.PropertyType.GetGenericArguments()[0];
                isEnumerableEnum = true;
            }

            // Build help text with enum values if applicable
            var enumValues = "";
            if (enumType != null)
            {
                var values = Enum.GetNames(enumType);
                var isMultiple = isEnumerableEnum;
                enumValues = $" Available values" +
                             (isMultiple ? $" (separate multiple with '{optAttr.Separator}')" : "") +
                             $": " +
                             string.Join(", ", values);
            }

            var optionDisplayName = string.IsNullOrEmpty(optAttr.LongName)
                ? ConvertToKebabCase(prop.Name)
                : optAttr.LongName;
            var labelText = (_configuration.Theme.ShowRequiredAsterisk && optAttr.Required ? "* " : "") +
                            optionDisplayName +
                            (!string.IsNullOrEmpty(optAttr.HelpText) ? $" ({optAttr.HelpText})" : "") +
                            enumValues;

            var label = new Label
            {
                Text = labelText,
                Left = 10,
                Top = y,
                Width = 500,
                AutoSize = true,
                BorderStyle = GetBorderStyle()
            };

            // Apply required field styling to label
            if (optAttr.Required)
            {
                label.ForeColor = _configuration.Theme.RequiredLabelColor;
                if (_configuration.Theme.UseRequiredFieldBoldFont)
                    label.Font = new Font(label.Font ?? _configuration.Theme.LabelFont, FontStyle.Bold);
            }
            else
            {
                label.ForeColor = _configuration.Theme.TextColor;
                label.Font = _configuration.Theme.LabelFont;
            }

            y += 20;

            Control input;
            Control dropZone = null;

            // Get the default value from the instance if possible
            object defaultValue = null;
            if (verbInstance != null)
                try
                {
                    defaultValue = prop.GetValue(verbInstance);
                }
                catch
                {
                }

            // Boolean types (including nullable)
            if (underlyingType == typeof(bool))
            {
                var checkBox = new CheckBox { Left = 10, Top = y, Width = 300 };
                if (IsNullableType(prop.PropertyType))
                {
                    checkBox.ThreeState = true;
                    if (defaultValue is bool b)
                        checkBox.CheckState = b ? CheckState.Checked : CheckState.Unchecked;
                    else
                        checkBox.CheckState = CheckState.Indeterminate;
                }
                else
                {
                    if (defaultValue is bool b) checkBox.Checked = b;
                }

                // Apply required field styling
                if (optAttr.Required) checkBox.BackColor = _configuration.Theme.RequiredFieldBackgroundColor;

                input = checkBox;
                _optionsPanel.Controls.Add(input);
            }
            // Enum
            else if (enumType != null && !isEnumerableEnum)
            {
                var comboBox = new ComboBox
                {
                    Left = 10,
                    Top = y,
                    Width = 300,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };

                // Add empty option for nullable enums
                if (IsNullableType(prop.PropertyType)) comboBox.Items.Add("<none>");

                comboBox.Items.AddRange(Enum.GetNames(enumType));

                if (defaultValue != null)
                    comboBox.SelectedItem = defaultValue.ToString();
                else if (IsNullableType(prop.PropertyType)) comboBox.SelectedIndex = 0; // Select <none>

                // Apply required field styling
                if (optAttr.Required) comboBox.BackColor = _configuration.Theme.RequiredFieldBackgroundColor;

                input = comboBox;
                _optionsPanel.Controls.Add(input);
            }
            // IEnumerable<Enum>
            else if (enumType != null && isEnumerableEnum)
            {
                var enumNames = Enum.GetNames(enumType);
                var itemHeight = 18; // Approximate height per item
                var minHeight = 60;
                var maxHeight = 150;
                var calculatedHeight = Math.Max(minHeight, Math.Min(maxHeight, enumNames.Length * itemHeight + 4));

                // Check if the IEnumerable<Enum> is nullable
                var isNullableEnumerable = IsNullableType(prop.PropertyType) ||
                                           (prop.PropertyType.IsGenericType &&
                                            prop.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
                                            prop.PropertyType.GetGenericArguments()[0].IsGenericType &&
                                            prop.PropertyType.GetGenericArguments()[0].GetGenericTypeDefinition() ==
                                            typeof(Nullable<>));

                // Adjust panel size if nullable
                var topOffset = isNullableEnumerable ? 25 : 0;
                var panelHeight = _configuration.ShowCheckAllButtons
                    ? Math.Max(calculatedHeight + topOffset, 60 + topOffset)
                    : calculatedHeight + topOffset;

                var panel = new Panel
                {
                    Left = 10,
                    Top = y,
                    Width = _configuration.ShowCheckAllButtons ? 400 : 300,
                    Height = panelHeight,
                    BorderStyle = BorderStyle.None // No border for individual panels
                };

                CheckBox enableCheckBox = null;
                if (isNullableEnumerable)
                {
                    enableCheckBox = new CheckBox
                    {
                        Text = "Set values (uncheck for null)",
                        Left = 0,
                        Top = 0,
                        Width = 250,
                        Height = 20,
                        Checked = defaultValue != null
                    };
                    panel.Controls.Add(enableCheckBox);
                }

                var checkedListBox = new CheckedListBox
                {
                    Left = 0,
                    Top = topOffset,
                    Width = 300,
                    Height = calculatedHeight,
                    CheckOnClick = true,
                    ScrollAlwaysVisible = enumNames.Length > 8,
                    Enabled = !isNullableEnumerable || defaultValue != null
                };
                checkedListBox.Items.AddRange(enumNames);

                // Apply required field styling
                if (optAttr.Required)
                {
                    checkedListBox.BackColor = _configuration.Theme.RequiredFieldBackgroundColor;
                    if (enableCheckBox != null)
                        enableCheckBox.BackColor = _configuration.Theme.RequiredFieldBackgroundColor;
                }

                // Wire up enable/disable logic for nullable
                if (enableCheckBox != null)
                    enableCheckBox.CheckedChanged += (s, e) =>
                    {
                        checkedListBox.Enabled = enableCheckBox.Checked;
                        if (!enableCheckBox.Checked)
                            // Clear all selections when disabled
                            for (var i = 0; i < checkedListBox.Items.Count; i++)
                                checkedListBox.SetItemChecked(i, false);
                    };

                if (_configuration.ShowCheckAllButtons)
                {
                    // Create Check All / Uncheck All buttons
                    var checkAllButton = new Button
                    {
                        Text = "Check All",
                        Left = 305,
                        Top = topOffset,
                        Width = 90,
                        Height = 25
                    };

                    var uncheckAllButton = new Button
                    {
                        Text = "Uncheck All",
                        Left = 305,
                        Top = topOffset + 30,
                        Width = 90,
                        Height = 25
                    };

                    checkAllButton.Click += (s, e) =>
                    {
                        for (var i = 0; i < checkedListBox.Items.Count; i++)
                            checkedListBox.SetItemChecked(i, true);
                    };

                    uncheckAllButton.Click += (s, e) =>
                    {
                        for (var i = 0; i < checkedListBox.Items.Count; i++)
                            checkedListBox.SetItemChecked(i, false);
                    };

                    panel.Controls.Add(checkAllButton);
                    panel.Controls.Add(uncheckAllButton);
                }
                else
                {
                    // Adjust panel width if no buttons
                    panel.Width = 300;
                }

                if (defaultValue is IEnumerable<object> defEnumList)
                {
                    var defNames = defEnumList.Select(x => x.ToString()).ToHashSet();
                    for (var i = 0; i < checkedListBox.Items.Count; i++)
                        if (defNames.Contains(checkedListBox.Items[i].ToString()))
                            checkedListBox.SetItemChecked(i, true);
                }

                panel.Controls.Add(checkedListBox);

                // For nullable IEnumerable<Enum>, we need to store both the checkbox and the list
                if (enableCheckBox != null)
                    // Store the enable checkbox in the panel's Tag property for later retrieval
                    panel.Tag = enableCheckBox;

                input = checkedListBox; // Store reference to the CheckedListBox, not the panel
                _optionsPanel.Controls.Add(panel);
            }
            else
            {
                const int dropZoneWidth = 160;
                const int browseButtonWidth = 30;
                const int rightPadding = 10;
                const int leftPadding = 10;
                const int controlSpacing = 5;

                var inputPanel = new Panel
                {
                    Left = leftPadding,
                    Top = y,
                    Width = ClientSize.Width - (leftPadding + rightPadding),
                    Height = 24,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                    MinimumSize = new Size(300, 24), // Set minimum width
                    Padding = new Padding(0, 0, rightPadding, 0),
                    BorderStyle = BorderStyle.None // No border for individual panels
                };

                input = new TextBox
                {
                    Dock = DockStyle.Fill,
                    BorderStyle = GetBorderStyle(),
                    Margin = new Padding(0, 0, dropZoneWidth + browseButtonWidth + controlSpacing * 2, 0)
                };

                // Apply required field styling to input
                if (optAttr.Required) input.BackColor = _configuration.Theme.RequiredFieldBackgroundColor;

                if (defaultValue != null && !(defaultValue is bool))
                    ((TextBox)input).Text = defaultValue.ToString();

                var browseButton = new Button
                {
                    Text = "...",
                    Width = browseButtonWidth,
                    Dock = DockStyle.Right,
                    Height = 24,
                    FlatStyle = _showControlEdges ? FlatStyle.Standard : FlatStyle.System
                };

                // Determine path type through multiple fallbacks
                var pathType = DeterminePathType(prop);

                browseButton.Click += (s, e) =>
                {
                    if (pathType == PathType.Directory)
                    {
                        using var folderDialog = new FolderBrowserDialog
                        {
                            Description = $"Select folder for {optAttr.LongName ?? prop.Name}",
                            SelectedPath = _workingDirInput.Text,
                            ShowNewFolderButton = true
                        };

                        if (folderDialog.ShowDialog() == DialogResult.OK)
                            ((TextBox)input).Text = folderDialog.SelectedPath;
                    }
                    else if (pathType == PathType.File)
                    {
                        var pathAttr = prop.GetCustomAttribute<PathTypeAttribute>();
                        var isMultiSelect = typeof(IEnumerable<string>).IsAssignableFrom(prop.PropertyType) ||
                                            (prop.PropertyType.IsGenericType &&
                                             prop.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
                                             prop.PropertyType.GetGenericArguments()[0].IsEnum);

                        using var fileDialog = new OpenFileDialog
                        {
                            Title = $"Select file for {optAttr.LongName ?? prop.Name}",
                            InitialDirectory = _workingDirInput.Text,
                            Filter = pathAttr?.Filter ?? "All Files|*.*",
                            Multiselect = isMultiSelect
                        };

                        if (fileDialog.ShowDialog() == DialogResult.OK)
                            ((TextBox)input).Text = string.Join(optAttr.Separator.ToString(), fileDialog.FileNames);
                    }
                    else // PathType.Any - create dual buttons
                    {
                        // For 'Any' type, we'll handle this after creating the panel
                        ShowPathSelectionMenu(browseButton, input, prop, optAttr);
                    }
                };

                dropZone = new Panel
                {
                    Width = dropZoneWidth,
                    Height = 24,
                    Dock = DockStyle.Right,
                    BorderStyle = BorderStyle.FixedSingle,
                    AllowDrop = true,
                    Tag = (input, prop, optAttr)
                };
                var scrollPosition = Point.Empty;

                dropZone.DragEnter += (s, e) =>
                {
                    if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    {
                        e.Effect = DragDropEffects.Copy;
                        // Save current scroll position when drag enters
                        scrollPosition = _optionsPanel.AutoScrollPosition;
                    }
                };

                // Prevent auto-scrolling during drag over
                dropZone.DragOver += (s, e) =>
                {
                    if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    {
                        e.Effect = DragDropEffects.Copy;
                        // Keep restoring the scroll position during drag
                        if (scrollPosition != Point.Empty)
                            _optionsPanel.AutoScrollPosition = new Point(-scrollPosition.X, -scrollPosition.Y);
                    }
                };

                dropZone.DragDrop += (s, e) =>
                {
                    var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    if (files.Length > 0)
                    {
                        var tuple = ((Control, PropertyInfo, OptionAttribute))dropZone.Tag;
                        var textBox = (TextBox)tuple.Item1;
                        var property = tuple.Item2;
                        var optionAttr = tuple.Item3;

                        if (typeof(IEnumerable<string>).IsAssignableFrom(property.PropertyType) ||
                            (property.PropertyType.IsGenericType &&
                             property.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
                             property.PropertyType.GetGenericArguments()[0].IsEnum))
                            // For list properties, join all files with the separator
                            textBox.Text = string.Join(optionAttr.Separator.ToString(), files);
                        else
                            // For single file properties, use the first file
                            textBox.Text = files[0];
                    }

                    // Final restore of scroll position after drop
                    if (scrollPosition != Point.Empty)
                        _optionsPanel.AutoScrollPosition = new Point(-scrollPosition.X, -scrollPosition.Y);
                };
                var dropLabel = new Label
                {
                    Text = typeof(IEnumerable<string>).IsAssignableFrom(prop.PropertyType) ||
                           (prop.PropertyType.IsGenericType &&
                            prop.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
                            prop.PropertyType.GetGenericArguments()[0].IsEnum)
                        ? "Drop files/folders here"
                        : "Drop file/folder here",
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill
                };
                dropZone.Controls.Add(dropLabel);

                // Add controls in the correct order
                inputPanel.Controls.Add(input);
                inputPanel.Controls.Add(browseButton);
                inputPanel.Controls.Add(dropZone);

                _optionsPanel.Controls.Add(inputPanel);
            }

            _optionsPanel.Controls.Add(label);
            _optionInputs[prop.Name] = (prop, input);

            // Track all controls for search/filter
            var container = input.Parent != null && input.Parent != _optionsPanel ? input.Parent : null;
            _allControls[prop.Name] = (label, input, container);

            // Adjust y position based on control height
            if (input is CheckedListBox clb)
            {
                // For CheckedListBox, we need to account for the panel height
                var parentPanel = clb.Parent as Panel;
                if (parentPanel != null && parentPanel != _optionsPanel)
                    y += parentPanel.Height + 20; // Use panel height
                else
                    y += clb.Height + 20; // Fallback to CheckedListBox height
            }
            else
            {
                y += 40;
            }
        }
    }

    private void OnRunClicked(object sender, EventArgs e)
    {
        if (_verbSelector.SelectedItem == null) return;
        var selectedVerb = _verbSelector.SelectedItem.ToString();
        var verbType = _verbTypes[selectedVerb];

        List<string> missingRequired = new();

        // Validate required fields first
        foreach (var (propName, (prop, input)) in _optionInputs)
        {
            var optAttr = prop.GetCustomAttribute<OptionAttribute>();
            if (!optAttr.Required) continue;

            var longName = string.IsNullOrEmpty(optAttr.LongName) ? ConvertToKebabCase(prop.Name) : optAttr.LongName;

            if (input is TextBox textBox && string.IsNullOrWhiteSpace(textBox.Text))
                missingRequired.Add(longName);
            else if (input is ComboBox comboBox && comboBox.SelectedItem == null)
                missingRequired.Add(longName);
            else if (input is CheckedListBox checkedListBox && checkedListBox.CheckedItems.Count == 0)
                missingRequired.Add(longName);
        }

        if (missingRequired.Any())
        {
            MessageBox.Show("Missing required fields: " + string.Join(", ", missingRequired), "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Create the parsed object
        var selectedType = _verbTypes[_verbSelector.SelectedItem.ToString()];
        var parsedObject = Activator.CreateInstance(selectedType);

        // Set property values
        foreach (var kvp in _optionInputs)
        {
            var (prop, control) = kvp.Value;
            object value = null;

            if (control is CheckBox checkBox)
            {
                if (IsNullableType(prop.PropertyType) && checkBox.ThreeState)
                    value = checkBox.CheckState switch
                    {
                        CheckState.Checked => true,
                        CheckState.Unchecked => false,
                        CheckState.Indeterminate => null,
                        _ => null
                    };
                else
                    value = checkBox.Checked;
            }
            else if (control is TextBox textBox)
            {
                var propType = prop.PropertyType;
                var underlyingType = GetUnderlyingType(propType);
                var isNullable = IsNullableType(propType);

                // Handle empty text for nullable types
                if (isNullable && string.IsNullOrWhiteSpace(textBox.Text))
                {
                    value = null;
                }
                else if (underlyingType == typeof(string))
                {
                    value = textBox.Text;
                }
                else if (underlyingType == typeof(int))
                {
                    if (int.TryParse(textBox.Text, out var intValue))
                        value = intValue;
                    else if (!isNullable && !string.IsNullOrWhiteSpace(textBox.Text))
                        continue; // Skip invalid non-nullable values
                }
                else if (underlyingType == typeof(long))
                {
                    if (long.TryParse(textBox.Text, out var longValue))
                        value = longValue;
                    else if (!isNullable && !string.IsNullOrWhiteSpace(textBox.Text))
                        continue;
                }
                else if (underlyingType == typeof(double))
                {
                    if (double.TryParse(textBox.Text, out var doubleValue))
                        value = doubleValue;
                    else if (!isNullable && !string.IsNullOrWhiteSpace(textBox.Text))
                        continue;
                }
                else if (underlyingType == typeof(float))
                {
                    if (float.TryParse(textBox.Text, out var floatValue))
                        value = floatValue;
                    else if (!isNullable && !string.IsNullOrWhiteSpace(textBox.Text))
                        continue;
                }
                else if (underlyingType == typeof(decimal))
                {
                    if (decimal.TryParse(textBox.Text, out var decimalValue))
                        value = decimalValue;
                    else if (!isNullable && !string.IsNullOrWhiteSpace(textBox.Text))
                        continue;
                }
                else if (underlyingType.IsEnum)
                {
                    if (Enum.TryParse(underlyingType, textBox.Text, true, out var enumValue))
                        value = enumValue;
                    else if (!isNullable && !string.IsNullOrWhiteSpace(textBox.Text))
                        continue;
                }
                else if (IsEnumerableType(propType) && propType != typeof(string))
                {
                    var elementType = propType.GetGenericArguments()[0];
                    var values = textBox.Text.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s))
                        .ToList();

                    if (values.Any())
                    {
                        var typedList = Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
                        var addMethod = typedList.GetType().GetMethod("Add");

                        foreach (var item in values)
                        {
                            object convertedItem = item;
                            if (elementType != typeof(string)) convertedItem = Convert.ChangeType(item, elementType);
                            addMethod.Invoke(typedList, new[] { convertedItem });
                        }

                        value = typedList;
                    }
                    // else: leave value as null, don't create empty list
                }
            }
            else if (control is ComboBox comboBox)
            {
                var underlyingType = GetUnderlyingType(prop.PropertyType);
                if (underlyingType.IsEnum && comboBox.SelectedItem != null)
                {
                    var selectedText = comboBox.SelectedItem.ToString();
                    if (selectedText == "<none>" && IsNullableType(prop.PropertyType))
                        value = null;
                    else if (selectedText != "<none>") value = Enum.Parse(underlyingType, selectedText);
                }
            }
            else if (control is CheckedListBox checkedListBox)
            {
                // Handle IEnumerable<Enum>
                if (prop.PropertyType.IsGenericType &&
                    prop.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
                    prop.PropertyType.GetGenericArguments()[0].IsEnum)
                {
                    // Check if there's a nullable enable checkbox
                    CheckBox enableCheckBox = null;
                    if (checkedListBox.Parent != null && checkedListBox.Parent.Tag is CheckBox cb) enableCheckBox = cb;

                    // If nullable and checkbox is unchecked, set to null
                    if (enableCheckBox != null && !enableCheckBox.Checked)
                    {
                        value = null;
                    }
                    else
                    {
                        var enumType = prop.PropertyType.GetGenericArguments()[0];
                        var selectedItems = checkedListBox.CheckedItems.Cast<string>().ToList();

                        if (selectedItems.Any())
                        {
                            var typedList = Activator.CreateInstance(typeof(List<>).MakeGenericType(enumType));
                            var addMethod = typedList.GetType().GetMethod("Add");

                            foreach (var item in selectedItems)
                            {
                                var enumValue = Enum.Parse(enumType, item);
                                addMethod.Invoke(typedList, new[] { enumValue });
                            }

                            value = typedList;
                        }
                        // else: leave value as null, don't create empty list
                    }
                }
            }

            if (value != null || (IsNullableType(prop.PropertyType) && value == null))
                prop.SetValue(parsedObject, value);
        }

        _result = parsedObject;

        // Generate command string using the universal builder
        var commandArgs = CommandArgumentBuilder.BuildCommandString(parsedObject);
        _commandString = $"{_exeName} {commandArgs}";

        // Show command confirmation dialog
        using (var confirmDialog = new CommandConfirmationDialog(_commandString, parsedObject, _exeName))
        {
            var dialogResult = confirmDialog.ShowDialog(this);

            if (dialogResult == DialogResult.Yes)
            {
                // User confirmed execution
                DialogResult = DialogResult.OK;
                Close();
            }
            // else: User cancelled, stay on the form
        }
    }

    public object GetResult()
    {
        return _result;
    }

    public string GetCommandString()
    {
        return _commandString;
    }

    private bool IsEnumerableType(Type type)
    {
        if (type == typeof(string)) return false;
        return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ||
               type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
    }

    private Type GetUnderlyingType(Type type)
    {
        // Handle nullable types
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            return Nullable.GetUnderlyingType(type);
        return type;
    }

    private bool IsNullableType(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    private int GetPropertyTypeSortOrder(Type type)
    {
        // Get the underlying type for nullable types
        var checkType = GetUnderlyingType(type);

        // Define sort order for different type categories
        if (checkType == typeof(bool))
            return 1; // Booleans first
        if (checkType.IsEnum)
            return 2; // Enums second
        if (checkType == typeof(int) || checkType == typeof(long) || checkType == typeof(short) ||
            checkType == typeof(byte))
            return 3; // Integer types
        if (checkType == typeof(double) || checkType == typeof(float) || checkType == typeof(decimal))
            return 4; // Floating point types
        if (checkType == typeof(string))
            return 5; // Strings
        if (IsEnumerableType(type))
        {
            // Check if it's enumerable of enum
            if (type.IsGenericType && type.GetGenericArguments().Length > 0 && type.GetGenericArguments()[0].IsEnum)
                return 6; // IEnumerable<Enum>
            return 7; // Other IEnumerable types
        }

        return 8; // Everything else
    }

    private string QuoteIfNeeded(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
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

    private void OnSearchFilterChanged(object sender, EventArgs e)
    {
        if (_allControls == null) return;

        var searchText = _searchBox.Text.ToLower();
        var filterType = _filterCombo.SelectedItem?.ToString() ?? "All";

        foreach (var kvp in _allControls)
        {
            var (label, input, container) = kvp.Value;
            var propName = kvp.Key;
            var optionInfo = _optionInputs[propName];
            var optAttr = optionInfo.Prop.GetCustomAttribute<OptionAttribute>();

            // Check search criteria - search in all visible text
            var matchesSearch = string.IsNullOrEmpty(searchText) ||
                                propName.ToLower().Contains(searchText) ||
                                (optAttr?.LongName?.ToLower().Contains(searchText) ?? false) ||
                                (optAttr?.HelpText?.ToLower().Contains(searchText) ?? false) ||
                                (label.Text?.ToLower().Contains(searchText) ?? false) ||
                                GetControlText(input).ToLower().Contains(searchText) ||
                                SearchInControlOptions(input, searchText);

            // Check filter criteria
            var matchesFilter = filterType switch
            {
                "Required Only" => optAttr?.Required ?? false,
                "Optional Only" => !(optAttr?.Required ?? false),
                "With Values" => HasValue(input),
                "Empty" => !HasValue(input),
                _ => true // "All"
            };

            var shouldShow = matchesSearch && matchesFilter;

            // Show/hide controls
            label.Visible = shouldShow;
            if (container != null)
                container.Visible = shouldShow;
            else
                input.Visible = shouldShow;
        }
    }

    private bool HasValue(Control input)
    {
        return input switch
        {
            TextBox tb => !string.IsNullOrEmpty(tb.Text),
            CheckBox cb => cb.ThreeState ? cb.CheckState != CheckState.Indeterminate : cb.Checked,
            ComboBox cmb => cmb.SelectedIndex > 0 || (cmb.SelectedIndex == 0 && cmb.Text != "<none>"),
            CheckedListBox clb => clb.CheckedItems.Count > 0,
            _ => false
        };
    }

    private string GetControlText(Control input)
    {
        return input switch
        {
            TextBox tb => tb.Text,
            CheckBox cb => cb.ThreeState
                ? cb.CheckState.ToString()
                : cb.Checked.ToString(),
            ComboBox cmb => cmb.SelectedItem?.ToString() ?? cmb.Text,
            CheckedListBox clb => string.Join(", ", clb.CheckedItems.Cast<object>().Select(item => item.ToString())),
            _ => ""
        };
    }

    private bool SearchInControlOptions(Control input, string searchText)
    {
        switch (input)
        {
            case ComboBox cmb:
                // Search in all combo box items
                foreach (var item in cmb.Items)
                    if (item?.ToString().ToLower().Contains(searchText) ?? false)
                        return true;
                break;
            case CheckedListBox clb:
                // Search in all checklist items
                foreach (var item in clb.Items)
                    if (item?.ToString().ToLower().Contains(searchText) ?? false)
                        return true;
                break;
        }

        return false;
    }

    private PathType DeterminePathType(PropertyInfo prop)
    {
        // 1. Check for custom attribute
        var pathAttr = prop.GetCustomAttribute<PathTypeAttribute>();
        if (pathAttr != null) return pathAttr.PathType;

        // 2. Check property name patterns
        var propName = prop.Name.ToLower();
        var optAttr = prop.GetCustomAttribute<OptionAttribute>();
        var longName = optAttr?.LongName?.ToLower() ?? "";

        // Directory patterns
        var directoryPatterns = new[] { "folder", "directory", "dir", "path", "location", "root", "workspace" };
        foreach (var pattern in directoryPatterns)
            if (propName.Contains(pattern) || longName.Contains(pattern))
                // Check if it's not explicitly a file
                if (!propName.Contains("file") && !longName.Contains("file"))
                    return PathType.Directory;

        // File patterns
        var filePatterns = new[] { "file", "filename", "document", "script", "executable", "dll", "config" };
        foreach (var pattern in filePatterns)
            if (propName.Contains(pattern) || longName.Contains(pattern))
                return PathType.File;

        // 3. Default to Any if no pattern matches
        return PathType.Any;
    }

    private void ShowPathSelectionMenu(Button browseButton, Control input, PropertyInfo prop, OptionAttribute optAttr)
    {
        var contextMenu = new ContextMenuStrip();

        var selectFileItem = new ToolStripMenuItem("Select File...");
        selectFileItem.Click += (s, e) =>
        {
            var pathAttr = prop.GetCustomAttribute<PathTypeAttribute>();
            var isMultiSelect = typeof(IEnumerable<string>).IsAssignableFrom(prop.PropertyType);

            using var fileDialog = new OpenFileDialog
            {
                Title = $"Select file for {optAttr.LongName ?? prop.Name}",
                InitialDirectory = _workingDirInput.Text,
                Filter = pathAttr?.Filter ?? "All Files|*.*",
                Multiselect = isMultiSelect
            };

            if (fileDialog.ShowDialog() == DialogResult.OK)
                ((TextBox)input).Text = string.Join(optAttr.Separator.ToString(), fileDialog.FileNames);
        };

        var selectFolderItem = new ToolStripMenuItem("Select Folder...");
        selectFolderItem.Click += (s, e) =>
        {
            using var folderDialog = new FolderBrowserDialog
            {
                Description = $"Select folder for {optAttr.LongName ?? prop.Name}",
                SelectedPath = _workingDirInput.Text,
                ShowNewFolderButton = true
            };

            if (folderDialog.ShowDialog() == DialogResult.OK) ((TextBox)input).Text = folderDialog.SelectedPath;
        };

        contextMenu.Items.Add(selectFileItem);
        contextMenu.Items.Add(selectFolderItem);

        // Show the context menu at the button location
        contextMenu.Show(browseButton, new Point(0, browseButton.Height));
    }

    public void PreFillOptions(string verbName, Dictionary<string, string> propertyValues)
    {
        // Select the verb
        var index = _verbSelector.Items.IndexOf(verbName);
        if (index >= 0) _verbSelector.SelectedIndex = index;

        // Wait for the options UI to be built
        Application.DoEvents();

        foreach (var (propName, (prop, input)) in _optionInputs)
        {
            var optAttr = prop.GetCustomAttribute<OptionAttribute>();
            var longName = string.IsNullOrEmpty(optAttr.LongName) ? ConvertToKebabCase(prop.Name) : optAttr.LongName;

            if (propertyValues.TryGetValue(longName, out var value))
            {
                if (prop.PropertyType == typeof(bool))
                {
                    if (input is CheckBox cb)
                        cb.Checked = bool.TryParse(value, out var b) && b;
                }
                else if (prop.PropertyType == typeof(bool?))
                {
                    if (input is CheckBox cb)
                    {
                        if (string.IsNullOrWhiteSpace(value))
                            cb.CheckState = CheckState.Indeterminate;
                        else if (bool.TryParse(value, out var b))
                            cb.CheckState = b ? CheckState.Checked : CheckState.Unchecked;
                        else
                            cb.CheckState = CheckState.Indeterminate;
                    }
                }
                else if (input is ComboBox comboBox)
                {
                    comboBox.SelectedItem = value;
                }
                else if (input is CheckedListBox checkedListBox)
                {
                    var items = value.Split(optAttr.Separator);
                    for (var i = 0; i < checkedListBox.Items.Count; i++)
                        checkedListBox.SetItemChecked(i, items.Contains(checkedListBox.Items[i].ToString()));
                }
                else if (input is TextBox textBox)
                {
                    textBox.Text = value;
                }
            }
        }
    }
}
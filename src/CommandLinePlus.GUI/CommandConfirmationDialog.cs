using CommandLinePlus.Shared;

namespace CommandLinePlus.GUI;

public class CommandConfirmationDialog : Form
{
    private TextBox _commandTextBox;
    private Button _copyButton;
    private Button _executeButton;
    private Button _cancelButton;
    private CheckBox _includeDefaultsCheckBox;
    private readonly object _verbObject;
    private readonly string _exeName;
    private string _currentCommand;

    public CommandConfirmationDialog(string command, object verbObject = null, string exeName = "")
    {
        _currentCommand = command;
        _verbObject = verbObject;
        _exeName = exeName;
        InitializeComponents(command);
    }

    private void InitializeComponents(string command)
    {
        Text = "Confirm Command Execution";
        Width = 800;
        Height = 250;
        MinimumSize = new Size(600, 200);
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;

        var label = new Label
        {
            Text = "The following command will be executed:",
            AutoSize = true,
            Location = new Point(10, 10)
        };

        _commandTextBox = new TextBox
        {
            Text = command,
            Multiline = true,
            ReadOnly = true,
            Location = new Point(10, 40),
            Width = 760,
            Height = 80,
            ScrollBars = ScrollBars.Both,
            WordWrap = false,
            BackColor = SystemColors.Control,
            Font = new Font("Consolas", 9),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
        };
        
        // Ensure the text box shows the full command
        _commandTextBox.SelectionStart = 0;
        _commandTextBox.SelectionLength = 0;
        _commandTextBox.ScrollToCaret();

        _includeDefaultsCheckBox = new CheckBox
        {
            Text = "Include default values",
            Location = new Point(10, 130),
            Width = 200,
            Visible = _verbObject != null,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left
        };
        _includeDefaultsCheckBox.CheckedChanged += (s, e) =>
        {
            if (_verbObject != null)
            {
                UpdateCommandDisplay();
            }
        };

        _copyButton = new Button
        {
            Text = "Copy Command",
            Location = new Point(10, 160),
            Width = 100,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left
        };
        _copyButton.Click += (s, e) =>
        {
            Clipboard.SetText(_commandTextBox.Text);
            MessageBox.Show("Command copied to clipboard!", "Success", MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        };

        _executeButton = new Button
        {
            Text = "Execute",
            DialogResult = DialogResult.Yes,
            Location = new Point(670, 160),
            Width = 100,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right
        };

        _cancelButton = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Location = new Point(560, 160),
            Width = 100,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right
        };

        Controls.AddRange([label, _commandTextBox, _includeDefaultsCheckBox, _copyButton, _executeButton, _cancelButton]);
        AcceptButton = _executeButton;
        CancelButton = _cancelButton;
    }

    private void UpdateCommandDisplay()
    {
        if (_verbObject == null) return;

        var commandArgs = CommandArgumentBuilder.BuildCommandString(_verbObject, _includeDefaultsCheckBox.Checked);
        _currentCommand = string.IsNullOrEmpty(_exeName) ? commandArgs : $"{_exeName} {commandArgs}";
        _commandTextBox.Text = _currentCommand;
    }
}
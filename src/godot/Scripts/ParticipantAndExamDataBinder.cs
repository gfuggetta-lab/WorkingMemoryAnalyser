using Godot;
using System;

public partial class ParticipantAndExamDataBinder : Control
{
    [Export] public OptionButton ageInput;
    [Export] public OptionButton sexInput;
    [Export] public OptionButton handednessInput;
    [Export] public CheckBox displayTypeInput;
    [Export] public TextEdit overviewText;

    public override void _Ready()
    {
        ConnectOption(ageInput);
        ConnectOption(sexInput);
        ConnectOption(handednessInput);

        if (displayTypeInput != null)
            displayTypeInput.Toggled += _ => UpdateData();

        if (overviewText != null)
            overviewText.TextChanged += UpdateData;

        UpdateData();
    }

    private void ConnectOption(OptionButton option)
    {
        if (option != null)
            option.ItemSelected += _ => UpdateData();
    }

    private void UpdateData()
    {
        var experimentData = ExperimentShared.data;
        experimentData.Age = GetOptionText(ageInput);
        experimentData.Sex = GetOptionText(sexInput);
        experimentData.Handedness = GetOptionText(handednessInput);
        experimentData.DisplayType = GetDisplayType();

        var experimentName = GetExperimentNameFromOverview();
        if (!string.IsNullOrWhiteSpace(experimentName))
            experimentData.ExperimentName = experimentName;
    }

    private static string GetOptionText(OptionButton option)
    {
        return option == null ? string.Empty : option.Text;
    }

    private string GetDisplayType()
    {
        if (displayTypeInput == null || !displayTypeInput.ButtonPressed)
            return string.Empty;

        return displayTypeInput.Text;
    }

    private string GetExperimentNameFromOverview()
    {
        if (overviewText == null || string.IsNullOrWhiteSpace(overviewText.Text))
            return string.Empty;

        var lines = overviewText.Text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        return lines.Length == 0 ? string.Empty : lines[0].Trim();
    }
}

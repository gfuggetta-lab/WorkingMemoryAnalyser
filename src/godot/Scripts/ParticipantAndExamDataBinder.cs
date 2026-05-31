using Godot;
using MonitorInfo;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

public partial class ParticipantAndExamDataBinder : Control
{
	[Export] public OptionButton ageInput;
	[Export] public OptionButton sexInput;
	[Export] public OptionButton handednessInput;
	[Export] public Control monitorsGroup;
	[Export] public TextEdit overviewText;
	[Export] public Button enableButton;
	[Export] public Button selectExperimentButton;
	[Export] public FileDialog experimentDirectoryDialog;

	private readonly ButtonGroup monitorButtonGroup = new ButtonGroup();
	private readonly Dictionary<BaseButton, ConnectedMonitor> monitorByButton = new Dictionary<BaseButton, ConnectedMonitor>();
	private ConnectedMonitor selectedMonitor;

	[Signal]
	public delegate void dataReceivedEventHandler();

	[Signal]
	public delegate void dataIncompleteEventHandler();


	public override void _Ready()
	{
		ConnectOption(ageInput);
		ConnectOption(sexInput);
		ConnectOption(handednessInput);

		PopulateMonitorButtons();

		if (overviewText != null)
			overviewText.TextChanged += UpdateData;

		if (selectExperimentButton != null)
			selectExperimentButton.Pressed += SelectExperiment;

		if (experimentDirectoryDialog != null)
			experimentDirectoryDialog.DirSelected += OnExperimentDirectorySelected;

		UpdateData();
	}

	private void ConnectOption(OptionButton option)
	{
		if (option != null)
			option.ItemSelected += _ => UpdateData();
	}

	private bool UpdateDataAndVerify()
	{
		var experimentData = ExperimentShared.data;
		experimentData.Age = GetOptionText(ageInput);
		experimentData.Sex = GetOptionText(sexInput);
		experimentData.Handedness = GetOptionText(handednessInput);
		experimentData.DisplayType = GetDisplayType();
		ExperimentShared.SelectedMonitorID = GetMonitorId(selectedMonitor);

		var experimentName = GetExperimentNameFromOverview();
		if (!string.IsNullOrWhiteSpace(experimentName))
			experimentData.ExperimentName = experimentName;
		return true;
	}
	private void UpdateData()
	{
		UpdateDataAndVerify();
	}

	private static string GetOptionText(OptionButton option)
	{
		return option == null ? string.Empty : option.Text;
	}

	private string GetDisplayType()
	{
		if (selectedMonitor == null)
			return string.Empty;

		return FormatMonitorText(selectedMonitor);
	}

	private string GetExperimentNameFromOverview()
	{
		if (overviewText == null || string.IsNullOrWhiteSpace(overviewText.Text))
			return string.Empty;

		var lines = overviewText.Text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
		return lines.Length == 0 ? string.Empty : lines[0].Trim();
	}

	public void CheckParticipantData()
	{
		bool succ = UpdateDataAndVerify();
		if (succ)
			EmitSignal(SignalName.dataReceived);
		else
			EmitSignal(SignalName.dataIncomplete);
	}

	private void SelectExperiment()
	{
		if (experimentDirectoryDialog == null)
			return;

		experimentDirectoryDialog.PopupCenteredRatio(0.8f);
	}

	private void OnExperimentDirectorySelected(string directory)
	{
		ExperimentShared.SourcePath = directory;

		var overviewPath = Path.Combine(directory, "Overview.txt");
		if (!File.Exists(overviewPath))
		{
			GD.PushWarning($"Overview.txt was not found in selected experiment directory: {directory}");
			return;
		}

		if (overviewText != null)
			overviewText.Text = File.ReadAllText(overviewPath);

		UpdateData();
	}

	private void PopulateMonitorButtons()
	{
		if (monitorsGroup == null)
			return;

		ClearOldMonitorButtons();

		var options = new VBoxContainer
		{
			Name = "MonitorOptions"
		};
		options.SetAnchorsPreset(LayoutPreset.FullRect);
		options.OffsetLeft = 8;
		options.OffsetTop = 24;
		options.OffsetRight = -8;
		options.OffsetBottom = -8;
		monitorsGroup.AddChild(options);

		IReadOnlyList<ConnectedMonitor> monitors;
		try
		{
			monitors = MonitorEnumerator.GetConnectedMonitors();
		}
		catch (Exception ex)
		{
			GD.PushWarning($"Unable to enumerate monitors: {ex.Message}");
			return;
		}

		if (monitors.Count == 0)
			return;

		var defaultMonitor = FindMonitorForCurrentGodotScreen(monitors) ?? monitors[0];

		foreach (var monitor in monitors)
		{
			var capturedMonitor = monitor;
			var button = new CheckBox
			{
				Text = FormatMonitorText(capturedMonitor),
				ButtonGroup = monitorButtonGroup,
				TooltipText = capturedMonitor.Bounds.ToString()
			};
			button.Toggled += pressed =>
			{
				if (pressed)
					SelectMonitor(capturedMonitor);
			};

			monitorByButton[button] = capturedMonitor;
			options.AddChild(button);

			if (ReferenceEquals(capturedMonitor, defaultMonitor))
				button.ButtonPressed = true;
		}

		if (selectedMonitor == null)
			SelectMonitor(defaultMonitor);
	}

	private void ClearOldMonitorButtons()
	{
		monitorByButton.Clear();
		selectedMonitor = null;

		foreach (var child in monitorsGroup.GetChildren())
		{
			if (child is BaseButton || child.Name == "MonitorOptions")
			{
				monitorsGroup.RemoveChild(child);
				child.QueueFree();
			}
		}
	}

	private void SelectMonitor(ConnectedMonitor monitor)
	{
		selectedMonitor = monitor;
		ExperimentShared.SelectedMonitorID = GetMonitorId(monitor);
		UpdateData();
	}

	private static string FormatMonitorText(ConnectedMonitor monitor)
	{
		if (monitor == null)
			return string.Empty;

		var widthCm = monitor.PhysWidthMM / 10.0;
		var heightCm = monitor.PhysHeightMM / 10.0;
		return $"{monitor.Name}: {monitor.PixelWidth}x{monitor.PixelHeight} px, {widthCm:0.#}x{heightCm:0.#} cm";
	}

	private static string GetMonitorId(ConnectedMonitor monitor)
	{
		if (monitor == null)
			return string.Empty;

		if (!string.IsNullOrWhiteSpace(monitor.Id))
			return monitor.Id;

		return monitor.Name ?? string.Empty;
	}

	private static ConnectedMonitor FindMonitorForCurrentGodotScreen(IReadOnlyList<ConnectedMonitor> monitors)
	{
		var currentScreen = DisplayServer.WindowGetCurrentScreen();
		var godotScreen = new Rect2I(
			DisplayServer.ScreenGetPosition(currentScreen),
			DisplayServer.ScreenGetSize(currentScreen));

		ConnectedMonitor best = null;
		long bestScore = long.MinValue;

		foreach (var monitor in monitors)
		{
			long score = GetMonitorMatchScore(monitor, godotScreen);
			if (score > bestScore)
			{
				bestScore = score;
				best = monitor;
			}
		}

		return best;
	}

	private static long GetMonitorMatchScore(ConnectedMonitor monitor, Rect2I godotScreen)
	{
		Rectangle bounds = monitor.Bounds;
		long intersection = GetIntersectionArea(
			bounds.Left,
			bounds.Top,
			bounds.Right,
			bounds.Bottom,
			godotScreen.Position.X,
			godotScreen.Position.Y,
			godotScreen.End.X,
			godotScreen.End.Y);

		long score = intersection;
		if (bounds.X == godotScreen.Position.X && bounds.Y == godotScreen.Position.Y)
			score += 10_000_000_000L;
		if (bounds.Width == godotScreen.Size.X && bounds.Height == godotScreen.Size.Y)
			score += 1_000_000_000L;
		if (monitor.IsPrimary)
			score += 1;

		return score;
	}

	private static long GetIntersectionArea(int leftA, int topA, int rightA, int bottomA, int leftB, int topB, int rightB, int bottomB)
	{
		int left = Math.Max(leftA, leftB);
		int top = Math.Max(topA, topB);
		int right = Math.Min(rightA, rightB);
		int bottom = Math.Min(bottomA, bottomB);

		if (right <= left || bottom <= top)
			return 0;

		return (long)(right - left) * (bottom - top);
	}
}

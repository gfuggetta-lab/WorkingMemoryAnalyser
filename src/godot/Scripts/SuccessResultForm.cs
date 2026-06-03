using Godot;
using System.IO;

public partial class SuccessResultForm : Window
{
	public const string DefaultUploadUrl = "https://www.cognitoforms.com/GiorgioFuggetta/WorkingMemoryAnalyserUploadOutputFile2";
	public const string DefaultQuestionnaireUrl = "https://roehamptonpsych.az1.qualtrics.com/jfe/form/SV_9zU7Nf3Ped7Ep8h";

	[Export] public Button openResultsFolderButton;
	[Export] public LinkButton uploadOutputFileLink;
	[Export] public LinkButton questionnaireLink;

	public string OutputDir { get; private set; } = string.Empty;
	public string UploadUrl { get; private set; } = DefaultUploadUrl;
	public string QuestionnaireUrl { get; private set; } = DefaultQuestionnaireUrl;

	private string baseTitle = "Experiment Completed";

	public override void _Ready()
	{
		baseTitle = Title;
		Exclusive = true;

		if (openResultsFolderButton != null)
			openResultsFolderButton.Pressed += OpenResultsFolder;

		if (uploadOutputFileLink != null)
			uploadOutputFileLink.Pressed += OpenUploadUrl;

		if (questionnaireLink != null)
			questionnaireLink.Pressed += OpenQuestionnaireUrl;

		CloseRequested += QueueFree;
		ApplyVisibility();
	}

	public void Configure(
		string outputDir,
		string resultFileName,
		string uploadUrl = DefaultUploadUrl,
		string questionnaireUrl = DefaultQuestionnaireUrl)
	{
		OutputDir = outputDir ?? string.Empty;
		UploadUrl = uploadUrl ?? string.Empty;
		QuestionnaireUrl = questionnaireUrl ?? string.Empty;
		Title = string.IsNullOrWhiteSpace(resultFileName)
			? baseTitle
			: $"{baseTitle} ({resultFileName})";

		ApplyVisibility();
	}

	public void ShowSuccessForm(
		string outputDir,
		string resultFileName,
		string uploadUrl = DefaultUploadUrl,
		string questionnaireUrl = DefaultQuestionnaireUrl)
	{
		Configure(outputDir, resultFileName, uploadUrl, questionnaireUrl);
		PopupCentered();
	}

	public static SuccessResultForm ShowSuccessForm(
		Node parent,
		string outputDir,
		string resultFileName,
		string uploadUrl = DefaultUploadUrl,
		string questionnaireUrl = DefaultQuestionnaireUrl)
	{
		var scene = GD.Load<PackedScene>("res://successresultform.tscn");
		var form = scene.Instantiate<SuccessResultForm>();
		parent.AddChild(form);
		form.ShowSuccessForm(outputDir, resultFileName, uploadUrl, questionnaireUrl);
		return form;
	}

	private void ApplyVisibility()
	{
		if (openResultsFolderButton != null)
			openResultsFolderButton.Visible = !string.IsNullOrWhiteSpace(OutputDir);

		if (uploadOutputFileLink != null)
			uploadOutputFileLink.Visible = !string.IsNullOrWhiteSpace(UploadUrl);

		if (questionnaireLink != null)
			questionnaireLink.Visible = !string.IsNullOrWhiteSpace(QuestionnaireUrl);
	}

	private void OpenResultsFolder()
	{
		if (string.IsNullOrWhiteSpace(OutputDir))
			return;

		if (!Directory.Exists(OutputDir))
		{
			GD.PushWarning($"Results folder does not exist: {OutputDir}");
			return;
		}

		OS.ShellOpen(OutputDir);
	}

	private void OpenUploadUrl()
	{
		OpenUrl(UploadUrl);
	}

	private void OpenQuestionnaireUrl()
	{
		OpenUrl(QuestionnaireUrl);
	}

	private static void OpenUrl(string url)
	{
		if (!string.IsNullOrWhiteSpace(url))
			OS.ShellOpen(url);
	}
}

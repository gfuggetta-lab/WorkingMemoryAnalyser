using Godot;
using System;

public partial class ConsentCheck : Node
{
	[Signal]
	public delegate void consent_receivedEventHandler();

	[Signal]
	public delegate void consent_incompleteEventHandler();
	
	[Export]
	public CheckBox[] mustBeChecked;

	[Export]
	public RichTextLabel[] richTextLabelsWithLinks;

	public void CheckConsent()
	{
		int total = 0;
		int chk = 0;
		foreach(var ck in mustBeChecked)
		{
			if (ck == null) continue;
			total++;
			if (ck.ButtonPressed)
				chk++;
		}
		if (chk == total)
			EmitSignal(SignalName.consent_received);
		else
			EmitSignal(SignalName.consent_incomplete);
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (richTextLabelsWithLinks == null)
			return;

		foreach (var richTextLabel in richTextLabelsWithLinks)
		{
			if (richTextLabel != null)
				richTextLabel.MetaClicked += OpenMetaLink;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private static void OpenMetaLink(Variant meta)
	{
		var target = meta.AsString();
		if (string.IsNullOrWhiteSpace(target))
			return;

		if (!target.Contains("://") && target.Contains("@"))
			target = "mailto:" + target;

		OS.ShellOpen(target);
	}
}

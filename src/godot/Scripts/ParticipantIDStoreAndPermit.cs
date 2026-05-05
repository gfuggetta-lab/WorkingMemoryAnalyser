using Godot;
using System;

public partial class ParticipantIDStoreAndPermit : Node
{
	[Export]
	public ParticipantIDCheck check;
	[Export]
	public Button enableButton;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		RecheckButton();
	}

	public void RecheckButton()
	{
		if (check == null) return;
		var isValid = check.IsValid();
		if (enableButton != null)
			enableButton.Disabled = !isValid;

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}

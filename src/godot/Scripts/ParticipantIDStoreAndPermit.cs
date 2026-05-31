using Godot;
using System;

public partial class ParticipantIDStoreAndPermit : Node
{
	[Export]
	public ParticipantIDCheck check;
	[Export]
	public Button enableButton;
	[Signal]
	public delegate void dataReceivedEventHandler();

	[Signal]
	public delegate void dataIncompleteEventHandler();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		RecheckButton();
	}

	private bool VerifyData()
	{
		if (check == null) return false;
		var isValid = check.IsValid();
		if (isValid && check.BuildID(out var participantId))
			ExperimentShared.data.ParticipantId = participantId;

		return isValid;
	}

	public void RecheckButton()
	{
		bool succ = VerifyData();
		if (enableButton != null)
			enableButton.Disabled = !succ;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void CheckParticipantData()
	{
		var succ = VerifyData();
		if (succ)
			EmitSignal(SignalName.dataReceived);
		else
			EmitSignal(SignalName.dataIncomplete);
	}
}

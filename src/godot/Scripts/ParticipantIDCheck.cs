using Godot;
using System;
using System.Text;

public partial class ParticipantIDCheck : Node
{
	[Export]
	public OptionButton[] combos;
	[Export]
	public Label dstText;
	
	[Signal]
	public delegate void text_refreshEventHandler();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void RefreshIdText()
	{
		if (dstText != null)
		{
			BuildID(out var res);
			dstText.Text = res;
		}
		EmitSignal(SignalName.text_refresh);
	}
	public void RefreshIdTextFromIdx(int ignoredIndex)
	{
		RefreshIdText();
	}

	public static string GetOptText(OptionButton opt)
	{
		if (opt == null) return "";
		if (string.IsNullOrWhiteSpace(opt.Text)) return "";
		return opt.Text;
	}

	public bool BuildID(out string curId)
	{

		StringBuilder bl = new StringBuilder();
		int populated = 0;
		int total = 0;
		foreach(var opt in combos)
		{
			if (opt == null) continue;
			total++;
			string v = GetOptText(opt);
			if (string.IsNullOrEmpty(v))
			{
				bl.Append("_");
				continue;
			}
			populated++;
			bl.Append(v);
		}
		curId = bl.ToString();
		return total == populated;
	}

	public bool IsValid()
	{
		return BuildID(out var _);
	}
}

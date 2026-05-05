using Godot;
using System;

public partial class QuitApp : Node
{

	public void DoQuit()
	{
		GetTree().Quit(0);
	}
}

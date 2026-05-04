using Godot;
using System;

public partial class SceneLoad : Node
{
	[Export]
	public string sceneByName;
	[Export]
	public PackedScene sceneAsTscn;
	[Export]
	public bool replaceScene;
	[Export]
	public Node target;

	public void LoadScene()
	{
		PackedScene sc = null;
		if (sceneAsTscn != null) sc = sceneAsTscn;
		else sc = GD.Load<PackedScene>(sceneByName);
		
		if (replaceScene)
		{
			GD.Print($"replacing: {sc.ResourceName}");
			GetTree().ChangeSceneToPacked(sc);
		}
		else
		{
			GD.Print($"instantiating: {sc.ResourceName}");
			var nd = sc.Instantiate<Node>();
			if (target == null)
				target = this;
			target.AddChild(nd);
		}
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}

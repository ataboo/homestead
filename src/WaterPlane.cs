using Godot;
using System;

public class WaterPlane : Spatial
{
	[Export]
	Camera camera;

	Vector3 startOrigin = Vector3.Zero;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		camera = GetNode<Camera>("/root/Level/Canoe/MainCamera");
		startOrigin = Translation;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		var trailPos = camera.GlobalTransform.origin - camera.GlobalTransform.basis.z * 500f;
		trailPos.y = startOrigin.y;

		var thisTran = GlobalTransform;
		thisTran.origin = trailPos;
		GlobalTransform = thisTran;
	}
}

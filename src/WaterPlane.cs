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
		camera = GetNode<Camera>("/root/Level/MainCamera");
		startOrigin = Translation;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		var trailPos = camera.Translation - camera.Transform.basis.z * 100f;
		trailPos.y = startOrigin.y;

		Translation = trailPos;
		
		// thisTran.origin.x = camera.Transform.origin.x;
		// thisTran.origin.z = camera.Transform.origin.z;
		// thisTran.origin.y = startOrigin.z;
	}
}

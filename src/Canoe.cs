using Godot;
using System;

public class Canoe : RigidBody
{	
	[Export]
	public float speed = 2f;
	
	[Export]
	public float rotationSpeed = 3.5f;

	[Export]
	public float speedLimit = 5f;

	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	public override void _IntegrateForces(PhysicsDirectBodyState state) {
		ApplyForces(state);
	}

	private void ApplyForces(PhysicsDirectBodyState state) {
		if(state.LinearVelocity.Length() < speedLimit) {
			var input = Vector3.Zero;
			if (Input.IsActionPressed("forward")) {
				state.ApplyImpulse(Vector3.Zero, -Transform.basis.z * speed);
			}
			if (Input.IsActionPressed("back")) {
				state.ApplyImpulse(Vector3.Zero, Transform.basis.z * speed);
			}
		}
		
		if (Input.IsActionPressed("right")) {
			state.ApplyTorqueImpulse(Transform.basis.y * -rotationSpeed);
		}
		if (Input.IsActionPressed("left")) {
			state.ApplyTorqueImpulse(Transform.basis.y * rotationSpeed);
		}
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

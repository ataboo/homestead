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

	private Vector2 _inputAxes = Vector2.Zero;

	private Position3D _autopilotTarget;

	private float axisSpeed = 2f;

	public bool IsUnderPlayerControl => _autopilotTarget == null;

	public bool SpinWildlyUnderAutopilot = true;

	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	private void TakePlayerInput(float delta) {
		var dAxis = delta * axisSpeed;
		
		var input = Vector2.Zero;

		if (Input.IsActionPressed("forward")) {
			input.y += 1f;
		}
		
		if (Input.IsActionPressed("back")) {
			input.y -= 1f;
		}
		
		if (Input.IsActionPressed("left")) {
			input.x -= 1f;
		}
		
		if (Input.IsActionPressed("right")) {
			input.x += 1f;
		}

		if(input.y == 0f) {
			input.y = _inputAxes.y > 0f ? -1f : _inputAxes.y < 0f ? 1f : 0f;
		}
		if(input.x == 0f) {
			input.x = _inputAxes.x > 0f ? -1f : _inputAxes.x < 0f ? 1f : 0f;
		}

		_inputAxes.x = Mathf.Clamp(_inputAxes.x + input.x * dAxis, -1f, 1f);
		_inputAxes.y = Mathf.Clamp(_inputAxes.y + input.y * dAxis, -1f, 1f);
	}

	public override void _Process(float delta) {
		TakePlayerInput(delta);
	}

	public override void _IntegrateForces(PhysicsDirectBodyState state) {
		ApplyForces(state);
	}

	private void ApplyForces(PhysicsDirectBodyState state) {
		var playerForceScalar = IsUnderPlayerControl ? 1f : 0.2f;

		if(state.LinearVelocity.Length() < speedLimit) {
			var input = Vector3.Zero;
			if(_inputAxes.y != 0) {
				state.ApplyImpulse(Vector3.Zero, -Transform.basis.z * speed * _inputAxes.y * playerForceScalar);
			}
		}
		
		if(_inputAxes.x != 0) {
			state.ApplyTorqueImpulse(Transform.basis.y * -rotationSpeed * _inputAxes.x * playerForceScalar);
		}

		if(!IsUnderPlayerControl) {
			if(SpinWildlyUnderAutopilot) {
				state.ApplyTorqueImpulse(Transform.basis.y * -rotationSpeed);
			}

			var targetDir = _autopilotTarget.GlobalTransform.LookingAt(GlobalTransform.origin, Vector3.Up);
			state.LinearVelocity = state.LinearVelocity.LinearInterpolate(targetDir.basis.z * 5f, 5/60f);
		}
	}

	public void SetAutopilotTarget(Position3D position) {
		_autopilotTarget = position;
	}
}

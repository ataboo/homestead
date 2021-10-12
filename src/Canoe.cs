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

	private AnimationPlayer _animation;

	private CrateControl _berryCrate;

	private Position3D _cargoPos;

	private float axisSpeed = 2f;

	public bool IsUnderPlayerControl => _autopilotTarget == null;

	private bool _boatSpinning;

	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_animation = GetNode<AnimationPlayer>("AnimationPlayer");
		_berryCrate = GetNode<CrateControl>("/root/Level/Scenery/Props/BerryCrate");
		_cargoPos = GetNode<Position3D>("CargoPos");

		_berryCrate.SetTargetPos(_cargoPos, true);
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
			if(Mathf.Abs(_inputAxes.y) < dAxis) {
				_inputAxes.y = 0f;
			} else {
				input.y = _inputAxes.y > 0f ? -1f : _inputAxes.y < 0f ? 1f : 0f;
			}
		}
		if(input.x == 0f) {
			if(Mathf.Abs(_inputAxes.x) < dAxis) {
				_inputAxes.x = 0f;
			} else {
				input.x = _inputAxes.x > 0f ? -1f : _inputAxes.x < 0f ? 1f : 0f;
			}
		}

		_inputAxes.x = Mathf.Clamp(_inputAxes.x + input.x * dAxis, -1f, 1f);
		_inputAxes.y = Mathf.Clamp(_inputAxes.y + input.y * dAxis, -1f, 1f);
	}

	public override void _Process(float delta) {
		TakePlayerInput(delta);

		UpdatePaddleAnimation(delta);
	}

	private void UpdatePaddleAnimation(float delta) {
		bool moving = false;
		if(_inputAxes.y > 0.1f || Mathf.Abs(_inputAxes.x) > 0) {
			_animation.Play("PaddleForward");
			moving = true;
		} else if (_inputAxes.y < -0.1f) {
			_animation.Play("PaddleBackward");
			moving = true;
		} else {
			_animation.Play("PaddleIdle");
		}

		if(moving) {
			_animation.PlaybackSpeed = Mathf.Lerp(1, 5f, LinearVelocity.Length() / speedLimit);
		} else {
			_animation.PlaybackSpeed = 1f;
		}
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
			var xInput = _inputAxes.y < 0f ? -_inputAxes.x : _inputAxes.x;
			state.ApplyTorqueImpulse(Transform.basis.y * -rotationSpeed * xInput * playerForceScalar);
		}

		if(!IsUnderPlayerControl) {
			if(_boatSpinning) {
				state.ApplyTorqueImpulse(Transform.basis.y * -rotationSpeed);
			} else {
				var globalTran = GlobalTransform;
				var basisOffset = Mathf.Clamp(globalTran.basis.z.SignedAngleTo(_autopilotTarget.GlobalTransform.basis.z, Vector3.Up) * 10f, -rotationSpeed, rotationSpeed);
				
				if(Mathf.Abs(basisOffset) > 0.01f) {
					state.ApplyTorqueImpulse(Transform.basis.y * basisOffset);
				}
			}

			var targetDir = _autopilotTarget.GlobalTransform.LookingAt(GlobalTransform.origin, Vector3.Up);
			var velocity = state.LinearVelocity.LinearInterpolate(targetDir.basis.z * 5f, 5/60f);
			velocity.y = 0;
			if(velocity.Length() > 0.01f) {
				state.LinearVelocity = velocity;
			}
		}
	}

	public void SetAutopilotTarget(Position3D position, bool boatSpinning) {
		_autopilotTarget = position;
		_boatSpinning = position != null && boatSpinning;
	}
}

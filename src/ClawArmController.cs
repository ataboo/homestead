using Godot;
using System;

public class ClawArmController : Spatial
{
	private AnimationPlayer _bendPlayer;
	private AnimationPlayer _clawPlayer;
	private Spatial _clawModel;
	private Vector3 _initialModelTranslation;

	private float _t = 0f;

	private Spatial _targetSpatial;

	private Vector3 _targetOffset;

	private Vector3 _targetPos;

	private Vector3 _targetLookPos;
	
	private bool _bobbing = true;

	private bool _surfaced = false;

	[Export]
	private float bobPeriod = 3f;

	[Export]
	private float bobAmplitude = 0.2f;

	[Export]
	private float belowWaterHeight = -2f;

	[Export]
	private float aboveWaterHeight = 0.8f;

	public float movementSpeed = 1f;

	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_bendPlayer = GetNode<AnimationPlayer>("ClawModel/BendPlayer");
		_clawPlayer = GetNode<AnimationPlayer>("ClawModel/ClawPlayer");

		SetClawOpen(false);
		SetBentDown(false);

		_targetOffset = Vector3.Zero;
		TrackPosition(GlobalTransform.origin, GlobalTransform.origin - GlobalTransform.basis.z * 1f, _surfaced);


		_clawModel = GetNode<Spatial>("ClawModel");

		_initialModelTranslation = _clawModel.Translation;
	}

    public override void _Process(float delta) {
		if(_bobbing) {
			_t += delta;
			var theta = _t * Mathf.Pi * 2 / bobPeriod;
			_clawModel.Translation = new Vector3(_clawModel.Translation.x, bobAmplitude * Mathf.Sin(theta) + _initialModelTranslation.y, _clawModel.Translation.z);
		}

		if(_targetSpatial != null) {
			TrackPosition(_targetSpatial.GlobalTransform.origin - _targetSpatial.Transform.basis.Xform(_targetOffset), _targetSpatial.GlobalTransform.origin, _surfaced);
		}

		var globalTran = GlobalTransform;
		_targetLookPos.y = GlobalTransform.origin.y;
		var lookTran = globalTran.LookingAt(_targetLookPos, Vector3.Up);
		globalTran.basis = lookTran.basis.LerpTowards(lookTran.basis, movementSpeed * delta);
		globalTran.origin = GlobalTransform.origin.MoveToward(_targetPos, movementSpeed * delta);

		GlobalTransform = globalTran; 
	}

	public void TrackTarget(Spatial target, Vector3 offset, bool surfaced) {
		_surfaced = surfaced;
		_targetSpatial = target;
		_targetOffset = offset;
	}

	public void StopTrackingTarget() {
		_targetSpatial = null;
	}

	public float RangeToTarget() {
		return (_targetPos - GlobalTransform.origin).Length();
	}

	public void TrackPosition(Vector3 position, Vector3 lookTarget, bool surfaced) {
		position.y = surfaced ? aboveWaterHeight : belowWaterHeight;
		lookTarget.y = GlobalTransform.origin.y;
		
		_targetPos = position;
		_targetLookPos = lookTarget;
	}

	public void SetClawOpen(bool open) {
		PlayFromNow(_clawPlayer, "ClawOpen", open);
	}

	public void SetBentDown(bool bent) {
		PlayFromNow(_bendPlayer, "Grab", bent);
	}

	private void PlayFromNow(AnimationPlayer player, string animationName, bool forwards) {
		GD.Print($"Playing from now: {animationName}, {forwards}");
		
		var nextAnimation = player.GetAnimation(animationName);
		var playPos = forwards ? 0f : nextAnimation.Length;
		if(!string.IsNullOrEmpty(player.CurrentAnimation)) {
			playPos = player.CurrentAnimationPosition;
		}

		if(forwards) {
			player.Play(animationName);
		} else {
			player.PlayBackwards(animationName);
		}

		player.Seek(Mathf.Min(playPos, nextAnimation.Length));
	}
}

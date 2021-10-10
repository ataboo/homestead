using Godot;
using System;

public class ClawArmController : Spatial
{
	private AnimationPlayer bendPlayer;
	private AnimationPlayer clawPlayer;

	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		bendPlayer = GetNode<AnimationPlayer>("Claw/BendPlayer");
		clawPlayer = GetNode<AnimationPlayer>("Claw/ClawPlayer");

		SetClawOpen(true);
		SetBentDown(false);
	}

	public void SetClawOpen(bool open) {
		PlayFromNow(clawPlayer, "ClawOpen", open);
	}

	public void SetBentDown(bool bent) {
		PlayFromNow(bendPlayer, "Grab", bent);
	}

	private void PlayFromNow(AnimationPlayer player, string animationName, bool forwards) {
		GD.Print($"Playing from now: {animationName}, {forwards}");
		
		var playPos = player.IsPlaying() ? player.CurrentAnimationPosition : 0f;
		if(forwards) {
			player.Play(animationName);
		} else {
			player.PlayBackwards(animationName);
		}

		player.Seek(playPos);
	}
}

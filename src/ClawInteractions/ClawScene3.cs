using Godot;
using System;

public class ClawScene3 : Spatial
{
    private ClawArmController _claw;

    private AudioStreamPlayer3D _clawVoiceSource;

    private RandomizedAudioSource _clawSplashPlayer;

    [Signal]
    public delegate void SceneDoneSignal();

    private bool fired = false;

    public override void _Ready()
    {
        _claw = GetNode<ClawArmController>("ClawSpatial");
        _clawVoiceSource = GetNode<AudioStreamPlayer3D>("ClawSpatial/HSVoiceSource");
        _clawSplashPlayer = GetNode<RandomizedAudioSource>("ClawSpatial/ClawSplash");
    }

    void _on_ClawScene3_body_entered(Node body) {
        if(!fired && body is Canoe c) {
            GD.Print("Body is Canoe!");
            fired = true;
            ActionRoutine(c);
        }
    }
    
    async void ActionRoutine(Canoe canoe) {
        _claw.Visible = true;
        _claw.movementSpeed = 3f;

        GD.Print("ClawScene3: Surfacing...");
        _clawSplashPlayer.PlayRandomClip();

        _claw.SetClawOpen(true);
        for(int i=0; i<15; i++) {

            _claw.TrackPosition(_claw.GlobalTransform.origin, canoe.GlobalTransform.origin, true);
            await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
        }

        _clawVoiceSource.Play(0);

        for(int i=0; i<80; i++) {
            _claw.TrackPosition(_claw.GlobalTransform.origin, canoe.GlobalTransform.origin, true);
            await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
        }


         GD.Print("ClawScene3: Diving...");
        _claw.movementSpeed = 3f;
        _claw.TrackPosition(_claw.GlobalTransform.origin, canoe.GlobalTransform.origin, false);
        _clawSplashPlayer.PlayRandomClip();

        await ToSignal(GetTree().CreateTimer(3f), "timeout");

        GD.Print("ClawScene3: Disposing...");
        QueueFree();
    }
}

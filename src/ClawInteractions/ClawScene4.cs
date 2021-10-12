using Godot;
using System;

public class ClawScene4 : Spatial
{
    private ClawArmController _claw;

    private AudioStreamPlayer3D _clawVoiceSource;

    private RandomizedAudioSource _clawSplashPlayer;

    private MusicPlayerControl _musicPlayer;

    private CameraOrbit _camControl;

    private Canoe _canoe;

    [Export]
    public AudioStream sc8;

    private AudioStreamPlayer3D _canoeVoice;

    private Position3D _fertCamPos;

    private Position3D _farmCamPos;

    [Signal]
    public delegate void SceneDoneSignal();

    private bool fired = false;

    public override void _Ready()
    {
        _canoe = GetNode<Canoe>("/root/Level/Canoe");
        _canoeVoice = _canoe.GetNode<AudioStreamPlayer3D>("MCVoiceSource");
        _musicPlayer = _canoe.GetNode<MusicPlayerControl>("Music");
        _claw = GetNode<ClawArmController>("ClawSpatial");
        _clawVoiceSource = GetNode<AudioStreamPlayer3D>("ClawSpatial/HSVoiceSource");
        _clawSplashPlayer = GetNode<RandomizedAudioSource>("ClawSpatial/ClawSplash");
        _camControl = _canoe.GetNode<CameraOrbit>("/root/Level/Canoe/CameraOrbit");
        _fertCamPos = GetNode<Position3D>("FertCamPos");
        _farmCamPos = GetNode<Position3D>("FarmCamPos");
    }

    public void _on_HomesteadDropoff_SceneDoneSignal() {
        if(!fired) {
            GD.Print("Starting scene 4");
            fired = true;
            ActionRoutine(_canoe);
        }
    }
    
    async void ActionRoutine(Canoe canoe) {
        _claw.Visible = true;
        _claw.movementSpeed = 5f;

        var clawTran = _claw.GlobalTransform;
        clawTran.origin = canoe.GlobalTransform.origin - canoe.Transform.basis.Xform(new Vector3(-3f, 2f, 3f));
        _claw.GlobalTransform = clawTran;

        _claw.TrackTarget(canoe, new Vector3(-3, 0, 3), false);

        _musicPlayer.QueueTrackChange((MusicPlayerControl.TPiano | MusicPlayerControl.TGuitarPluck));
        
        await ToSignal(GetTree().CreateTimer(1.5f), "timeout");

        GD.Print("ClawScene4: Surfacing...");
        _clawSplashPlayer.PlayRandomClip();
        _claw.TrackTarget(canoe, new Vector3(3f, 0, 3f), true);
        await ToSignal(GetTree().CreateTimer(1f), "timeout");

        _claw.SetClawOpen(true);

        await ToSignal(GetTree().CreateTimer(1f), "timeout");

        _clawVoiceSource.Play(0);

        await ToSignal(GetTree().CreateTimer(8.5f), "timeout");

         GD.Print("ClawScene4: Diving...");
        _claw.movementSpeed = 3f;
        _claw.StopTrackingTarget();
        _claw.TrackPosition(_claw.GlobalTransform.origin, canoe.GlobalTransform.origin, false);
        _clawSplashPlayer.PlayRandomClip();

        _canoeVoice.Stream = sc8;
        _canoeVoice.Play(0);

        await ToSignal(GetTree().CreateTimer(3f), "timeout");

        _camControl.SetFixedCameraPos(_fertCamPos);
        await ToSignal(GetTree().CreateTimer(3.5f), "timeout");

        _camControl.SetFixedCameraPos(_farmCamPos);
        await ToSignal(GetTree().CreateTimer(3.5f), "timeout");

        _camControl.SetFixedCameraPos(null);

        EmitSignal(nameof(SceneDoneSignal));

        GD.Print("ClawScene4: Disposing...");
        QueueFree();
    }
}

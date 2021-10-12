using Godot;
using System;

public class ClawScene2 : Spatial
{
    private ClawArmController _claw;

    private AudioStreamPlayer3D _clawVoiceSource;

    private RandomizedAudioSource _clawSplashPlayer;

    private MusicPlayerControl _musicPlayer;

    private CameraOrbit _cameraControl;

    private Position3D _dockCamPos;

    [Signal]
    public delegate void SceneDoneSignal();

    private bool fired = false;

    public override void _Ready()
    {
        _claw = GetNode<ClawArmController>("ClawSpatial");
        _clawVoiceSource = GetNode<AudioStreamPlayer3D>("ClawSpatial/HSVoiceSource");
        _clawSplashPlayer = GetNode<RandomizedAudioSource>("ClawSpatial/ClawSplash");
        _musicPlayer = GetNode<MusicPlayerControl>("/root/Level/Canoe/Music");
        _cameraControl = GetNode<CameraOrbit>("/root/Level/Canoe/CameraOrbit");
        _dockCamPos = GetNode<Position3D>("DockCamPos");
    }

    void _on_ClawScene2_body_entered(Node body) {
        if(!fired && body is Canoe c) {
            GD.Print("Body is Canoe!");
            fired = true;
            ActionRoutine(c);
        }
    }
    
    async void ActionRoutine(Canoe canoe) {
        _claw.Visible = true;
        _claw.movementSpeed = 5f;

        var clawTran = _claw.GlobalTransform;
        clawTran.origin = canoe.GlobalTransform.origin - canoe.Transform.basis.Xform(new Vector3(3f, 2f, 3f));
        _claw.GlobalTransform = clawTran;

        _claw.TrackTarget(canoe, new Vector3(3, 0, 3), false);

        _musicPlayer.QueueTrackChange((MusicPlayerControl.TPiano | MusicPlayerControl.TGuitarPluck));
        
        await ToSignal(GetTree().CreateTimer(1.5f), "timeout");

        GD.Print("ClawScene2: Surfacing...");
        _clawSplashPlayer.PlayRandomClip();
        _claw.TrackTarget(canoe, new Vector3(3f, 0, 3f), true);
        await ToSignal(GetTree().CreateTimer(1f), "timeout");

        _claw.SetClawOpen(true);

        await ToSignal(GetTree().CreateTimer(1f), "timeout");

        _clawVoiceSource.Play(0);

        await ToSignal(GetTree().CreateTimer(4.5f), "timeout");

         GD.Print("ClawScene2: Diving...");
        _claw.movementSpeed = 3f;
        _claw.StopTrackingTarget();
        _claw.TrackPosition(_claw.GlobalTransform.origin, canoe.GlobalTransform.origin, false);
        _clawSplashPlayer.PlayRandomClip();

        _cameraControl.SetFixedCameraPos(_dockCamPos);

        await ToSignal(GetTree().CreateTimer(4f), "timeout");
        
        _cameraControl.SetFixedCameraPos(null);

        GD.Print("ClawScene2: Disposing...");
        QueueFree();
    }
}

using Godot;
using System;

public class ClawScene1 : Spatial
{
    [Export]
    public AudioStream mc1;

    [Export]
    public AudioStream mc2;
    
    [Export]
    public AudioStream mc3;
    
    [Export]
    public AudioStream mc4;
    
    [Export]
    public AudioStream h1;
    
    [Export]
    public AudioStream mc5;
    
    [Export]
    public AudioStream h2;

    private Position3D[] _rapidPathPoints;
    private int _rapidPathIdx;

    private ClawArmController _claw;

    private AudioStreamPlayer3D _mcVoiceSource;

    private AudioStreamPlayer3D _claw1VoiceSource;

    private RandomizedAudioSource _clawSplashPlayer;

    private MusicPlayerControl _musicPlayer;

    [Signal]
    public delegate void SceneDoneSignal();

    private bool fired = false;

    public override void _Ready()
    {
        _claw = GetNode<ClawArmController>("ClawSpatial");
        _rapidPathIdx = 0;
        _rapidPathPoints = new []{
            GetNode<Position3D>("RapidPath1"),
            GetNode<Position3D>("RapidPath2"),
            GetNode<Position3D>("RapidPath3")
        };

        _mcVoiceSource = GetNode<AudioStreamPlayer3D>("/root/Level/Canoe/MCVoiceSource");
        _claw1VoiceSource = GetNode<AudioStreamPlayer3D>("ClawSpatial/HSVoiceSource");
        _clawSplashPlayer = GetNode<RandomizedAudioSource>("ClawSpatial/ClawSplash");
        _musicPlayer = GetNode<MusicPlayerControl>("/root/Level/Canoe/Music");
    }

    void _on_ClawScene1_body_entered(Node body) {
        if(!fired && body is Canoe c) {
            GD.Print("Body is Canoe!");
            fired = true;
            ActionRoutine(c);
            NavigationRoutine(c);
        }
    }
    
    async void NavigationRoutine(Canoe canoe) {
        for(int i=0; i<_rapidPathPoints.Length; i++) {
            GD.Print($"ClawScene1: Canoe NavPoint {i}");
            while((canoe.GlobalTransform.origin - _rapidPathPoints[i].GlobalTransform.origin).LengthSquared() > 2f) {
                canoe.SetAutopilotTarget(_rapidPathPoints[i], true);
                await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
            }
        }

        canoe.SetAutopilotTarget(null, false);
    }

    async void ActionRoutine(Canoe canoe) {
        _claw.Visible = true;
        
        _claw.movementSpeed = 3f;

        _mcVoiceSource.Stream = mc1;
        _mcVoiceSource.Play(0);

        _musicPlayer.QueueTrackChange((MusicPlayerControl.TPiano | MusicPlayerControl.TGuitarPluck));

        await ToSignal(GetTree().CreateTimer(4f), "timeout");

        _mcVoiceSource.Stream = mc2;
        _mcVoiceSource.Play(0);

        GD.Print("ClawScene1: Surfacing...");
        _claw.TrackPosition(_claw.GlobalTransform.origin, canoe.GlobalTransform.origin, true);
        await ToSignal(GetTree().CreateTimer(1f), "timeout");
        _mcVoiceSource.Stream = mc3;
        _mcVoiceSource.Play(0);
        await ToSignal(GetTree().CreateTimer(1f), "timeout");

        GD.Print("ClawScene1: Tracking target...");
        _claw.movementSpeed = 5f;
        _claw.TrackTarget(canoe, new Vector3(0, 0, 3.2f), true);
        _claw.SetClawOpen(true);

       

        while(true) {
            await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
            if(_claw.RangeToTarget() < 1f) {
                GD.Print("ClawScene1: Bending...");
                _claw.SetClawOpen(false);
                _claw.SetBentDown(true);
                break;
            }
        }

        _mcVoiceSource.Stream = mc4;
        _mcVoiceSource.Play(0);

        await ToSignal(GetTree().CreateTimer(6.5f), "timeout");

        _claw.SetBentDown(false);
        _claw.SetClawOpen(true);
        _clawSplashPlayer.PlayRandomClip();

        _claw1VoiceSource.Stream = h1;
        _claw1VoiceSource.Play(0);

        _claw.TrackTarget(canoe, new Vector3(3f, 0, 3f), true);

        EmitSignal(nameof(SceneDoneSignal));

        await ToSignal(GetTree().CreateTimer(2.5f), "timeout");

        _mcVoiceSource.Stream = mc5;
        _mcVoiceSource.Play(0);

        await ToSignal(GetTree().CreateTimer(2.0f), "timeout");

        _mcVoiceSource.Stream = h2;
        _mcVoiceSource.Play(0);

        await ToSignal(GetTree().CreateTimer(8.5f), "timeout");

        GD.Print("ClawScene1: Diving...");
        _claw.movementSpeed = 3f;
        _claw.StopTrackingTarget();
        _claw.TrackPosition(_claw.GlobalTransform.origin, canoe.GlobalTransform.origin, false);
        _clawSplashPlayer.PlayRandomClip();
        
        await ToSignal(GetTree().CreateTimer(3f), "timeout");

        _musicPlayer.QueueTrackChange(MusicPlayerControl.TDrums | MusicPlayerControl.TGuitarStrum | MusicPlayerControl.TPiano);

        GD.Print("ClawScene1: Disposing...");
        QueueFree();
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

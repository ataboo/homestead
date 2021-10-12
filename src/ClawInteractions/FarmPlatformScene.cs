using Godot;
using System;

public class FarmPlatformScene : Spatial
{
    // [Export]
    // public AudioStream h8;
    
    // [Export]
    // public AudioStream mc7;
    
    // [Export]
    // public AudioStream h6;

    // [Export]
    // public AudioStream electricBoom;

    // [Export]
    // public AudioStream farBoom;

    [Export]
    public AudioStream h11;


    private AnimationPlayer _animationPlayer;
    
    private AnimationPlayer _animationPlayer2;

    private Position3D _dockPos;

    private Position3D _fertDropPoint;

    private CrateControl _fertCrate;

    private CrateControl _foodCrate;

    private Position3D _armGrabPoint;

    private AudioStreamPlayer3D _clawVoiceSource;

    private RandomizedAudioSource _clawSplashPlayer;

    private MusicPlayerControl _musicPlayer;

    private CollisionShape _trigger;

    [Signal]
    public delegate void SceneDoneSignal();

    private bool fired = false;

    public override void _Ready()
    {
        _trigger = GetNode<CollisionShape>("CollisionShape");

        var farmPlatform = GetNode<Spatial>("/root/Level/Scenery/Buildings/FarmPlatform");
        _dockPos = farmPlatform.GetNode<Position3D>("DockPoint");
        _fertDropPoint = farmPlatform.GetNode<Position3D>("FertDropPoint");
        _animationPlayer = farmPlatform.GetNode<AnimationPlayer>("AnimationPlayer");
        _foodCrate = farmPlatform.GetNode<CrateControl>("FoodCrate");
        _fertCrate = GetNode<CrateControl>("/root/Level/Scenery/Homestead/FertilizerCrate");
        _armGrabPoint = farmPlatform.GetNode<Position3D>("ClawArm1/Skeleton/BoneAttachment5/ArmGrabPoint/ArmGrabTransformed");

        _clawVoiceSource = farmPlatform.GetNode<AudioStreamPlayer3D>("ClawArm1/Skeleton/BoneAttachment5/HSVoiceSource");
        _clawSplashPlayer = farmPlatform.GetNode<RandomizedAudioSource>("ClawArm1/Skeleton/BoneAttachment5/ClawSplash");
        _musicPlayer = GetNode<MusicPlayerControl>("/root/Level/Canoe/Music");
    }

    void _on_FarmExchange_body_entered(Node body) {
        if(!fired && body is Canoe c) {
            GD.Print("Body is Canoe!");
            fired = true;
            ActionRoutine(c);
        }
    }

    public void _on_FertilizerPickup_SceneDoneSignal() {
        _trigger.Disabled = false;
    }
    
    async void ActionRoutine(Canoe canoe) {
        _animationPlayer.PlaybackSpeed = 0.75f;
        // _musicPlayer.QueueTrackChange((MusicPlayerControl.TPiano | MusicPlayerControl.TDrums));
        
        // await ToSignal(GetTree().CreateTimer(1.5f), "timeout");

        canoe.SetAutopilotTarget(_dockPos, false);

        var cargoPos = canoe.GetNode<Position3D>("CargoPos");

        while((canoe.GlobalTransform.origin - _dockPos.GlobalTransform.origin).Length() > 0.5f) {
            GD.Print("Waiting to dock...");
            await ToSignal(GetTree().CreateTimer(.1f), "timeout");
        }
        
        _animationPlayer.Play("FarmExchange");

        //0.7
        await _animationPlayer.WaitUntilPlayPos(this, 0.7f);
        _clawSplashPlayer.PlayRandomClip();
        
        //2.6
        await _animationPlayer.WaitUntilPlayPos(this, 2.6f);

        GD.Print($"Frame: {_animationPlayer.CurrentAnimationPosition}, expected: 2.6");

        _fertCrate.SetTargetPos(_armGrabPoint);

        //5.3
        await _animationPlayer.WaitUntilPlayPos(this, 5.3f);


        GD.Print($"Frame: {_animationPlayer.CurrentAnimationPosition}, expected: 5.3");

        _fertCrate.SetTargetPos(_fertDropPoint);

        //7.2
        await _animationPlayer.WaitUntilPlayPos(this, 7.2f);

        _foodCrate.SetTargetPos(_armGrabPoint);

        //10.1
        await _animationPlayer.WaitUntilPlayPos(this, 10.1f);

        _foodCrate.SetTargetPos(cargoPos);

        _clawVoiceSource.Stream = h11;
        _clawVoiceSource.Play(0);

        //14.5
        await _animationPlayer.WaitUntilPlayPos(this, 14.5f);
        
        _clawSplashPlayer.PlayRandomClip();

        canoe.SetAutopilotTarget(null, false);


        await ToSignal(GetTree().CreateTimer(1f), "timeout");

        _musicPlayer.QueueTrackChange(MusicPlayerControl.TDrums|MusicPlayerControl.TGuitarStrum);

        EmitSignal(nameof(SceneDoneSignal));

        GD.Print("DropoffScene: Disposing...");
        QueueFree();
    }
}

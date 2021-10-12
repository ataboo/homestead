using Godot;
using System;

public class FertilizerPickupScene : Spatial
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
    public AudioStream h8;


    private AnimationPlayer _animationPlayer1;
    
    private AnimationPlayer _animationPlayer2;

    private Position3D _dockPos;

    private Position3D _pickupPos;

    private CrateControl _fertCrate;

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

        var homesteadNode = GetNode<Spatial>("/root/Level/Scenery/Homestead");
        _dockPos = homesteadNode.GetNode<Position3D>("FertDockPos");
        _pickupPos = homesteadNode.GetNode<Position3D>("FertPickup");
        _animationPlayer1 = homesteadNode.GetNode<AnimationPlayer>("AnimationPlayer1");
        _fertCrate = homesteadNode.GetNode<CrateControl>("FertilizerCrate");

        _clawVoiceSource = homesteadNode.GetNode<AudioStreamPlayer3D>("ClawArm1/Skeleton/BoneAttachment5/HSVoiceSource");
        _clawSplashPlayer = homesteadNode.GetNode<RandomizedAudioSource>("ClawArm1/Skeleton/BoneAttachment5/ClawSplash");
        _musicPlayer = GetNode<MusicPlayerControl>("/root/Level/Canoe/Music");
    }

    void _on_FertilizerPickup_body_entered(Node body) {
        if(!fired && body is Canoe c) {
            GD.Print("Body is Canoe!");
            fired = true;
            ActionRoutine(c);
        }
    }

    public void _on_ClawScene4_SceneDoneSignal() {
        _trigger.Disabled = false;
    }
    
    async void ActionRoutine(Canoe canoe) {
        _animationPlayer1.PlaybackSpeed = 0.5f;
        // _musicPlayer.QueueTrackChange((MusicPlayerControl.TPiano | MusicPlayerControl.TDrums));
        
        // await ToSignal(GetTree().CreateTimer(1.5f), "timeout");

        canoe.SetAutopilotTarget(_dockPos, false);

        while((canoe.GlobalTransform.origin - _dockPos.GlobalTransform.origin).Length() > 0.5f) {
            GD.Print("Waiting to dock...");
            await ToSignal(GetTree().CreateTimer(.1f), "timeout");
        }
        
        _animationPlayer1.Play("ClawGrabFertilizer");
        _fertCrate.SetTargetPos(_pickupPos);

        await ToSignal(GetTree().CreateTimer(1f), "timeout");
        _clawSplashPlayer.PlayRandomClip();
        
        await ToSignal(GetTree().CreateTimer(8f), "timeout");

        _fertCrate.SetTargetPos(canoe.GetNode<Position3D>("CargoPos"));

        _clawVoiceSource.Stream = h8;
        _clawVoiceSource.Play(0);

        canoe.SetAutopilotTarget(null, false);

        await ToSignal(GetTree().CreateTimer(1f), "timeout");
        _clawSplashPlayer.PlayRandomClip();

        _musicPlayer.QueueTrackChange(MusicPlayerControl.TPiano | MusicPlayerControl.TDrums);

        await ToSignal(GetTree().CreateTimer(3f), "timeout");

        EmitSignal(nameof(SceneDoneSignal));

        GD.Print("DropoffScene: Disposing...");
        QueueFree();
    }
}

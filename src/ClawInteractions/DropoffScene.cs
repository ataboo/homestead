using Godot;
using System;

public class DropoffScene : Spatial
{
    [Export]
    public AudioStream h4;
    
    [Export]
    public AudioStream mc7;
    
    [Export]
    public AudioStream h6;

    [Export]
    public AudioStream electricBoom;

    [Export]
    public AudioStream farBoom;


    private Position3D _clawGrabPos;

    private AnimationPlayer _animationPlayer1;
    
    private AnimationPlayer _animationPlayer2;

    private Position3D _dockPos;

    private Position3D _deckPos;

    private CrateControl _berryCrate;

    private AudioStreamPlayer3D _clawVoiceSource;

    private AudioStreamPlayer3D _mcVoiceSource;

    private AudioStreamPlayer3D _farBoomPlayer;

    private RandomizedAudioSource _clawSplashPlayer;

    private MusicPlayerControl _musicPlayer;

    private AudioStreamPlayer3D _clawAssPlayer;

    private HomesteadCoreEffects _coreEffects;

    private Particles _arm2DamageParticles;

    private Particles _smokeParticles;

    [Signal]
    public delegate void SceneDoneSignal();

    private bool fired = false;

    public override void _Ready()
    {
        var homesteadNode = GetNode<Spatial>("/root/Level/Scenery/Homestead");
        _dockPos = homesteadNode.GetNode<Position3D>("DeliveryPoint");
        _deckPos = homesteadNode.GetNode<Position3D>("DeckPoint");
        _animationPlayer1 = homesteadNode.GetNode<AnimationPlayer>("AnimationPlayer1");
        _animationPlayer2 = homesteadNode.GetNode<AnimationPlayer>("AnimationPlayer2");
        _mcVoiceSource = GetNode<AudioStreamPlayer3D>("/root/Level/Canoe/MCVoiceSource");
		_berryCrate = GetNode<CrateControl>("/root/Level/Scenery/Props/BerryCrate");
        _farBoomPlayer = GetNode<AudioStreamPlayer3D>("/root/Level/Scenery/EMPScene/BoomPlayer");
        _clawAssPlayer = GetNode<AudioStreamPlayer3D>("ClawAssSound");
        _coreEffects = homesteadNode.GetNode<HomesteadCoreEffects>("CoreEffects");
        _arm2DamageParticles = homesteadNode.GetNode<Particles>("Arm2DamageParticles");

        _clawVoiceSource = homesteadNode.GetNode<AudioStreamPlayer3D>("ClawArm1/Skeleton/BoneAttachment5/HSVoiceSource");
        _clawSplashPlayer = homesteadNode.GetNode<RandomizedAudioSource>("ClawArm1/Skeleton/BoneAttachment5/ClawSplash");
        _clawGrabPos = homesteadNode.GetNode<Spatial>("ClawArm1/Skeleton/BoneAttachment5/GrabPos") as Position3D;
        _musicPlayer = GetNode<MusicPlayerControl>("/root/Level/Canoe/Music");
        _smokeParticles = GetNode<Particles>("/root/Level/PlayerTriggers/FinalEncounter/SmokeParticles");
    }

    void _on_HomesteadDropoff_body_entered(Node body) {
        if(!fired && body is Canoe c) {
            GD.Print("Body is Canoe!");
            fired = true;
            ActionRoutine(c);
        }
    }
    
    async void ActionRoutine(Canoe canoe) {
        _animationPlayer1.PlaybackSpeed = 1;
        _animationPlayer2.PlaybackSpeed = 1;

        _musicPlayer.QueueTrackChange((MusicPlayerControl.TPiano | MusicPlayerControl.TDrums));
        
        await ToSignal(GetTree().CreateTimer(1.5f), "timeout");

        canoe.SetAutopilotTarget(_dockPos, false);

        while((canoe.GlobalTransform.origin - _dockPos.GlobalTransform.origin).Length() > 0.5f) {
            GD.Print("Waiting to dock...");
            await ToSignal(GetTree().CreateTimer(.1f), "timeout");
        }
        
        _animationPlayer1.Play("CrateGrab2");

        
        await ToSignal(GetTree().CreateTimer(4.25f), "timeout");

        _berryCrate.SetTargetPos(_deckPos);

        await ToSignal(GetTree().CreateTimer(2f), "timeout");

        _clawVoiceSource.Stream = h4;
        _clawVoiceSource.Play(0);

        await ToSignal(GetTree().CreateTimer(1f), "timeout");
        
        _farBoomPlayer.Stream = electricBoom;
        _farBoomPlayer.Play(0);
        _musicPlayer.HardStop();

        _smokeParticles.Emitting = true;

        _coreEffects.Shutdown();

        await ToSignal(GetTree().CreateTimer(3.5f), "timeout");
        
        _mcVoiceSource.Stream = mc7;
        _mcVoiceSource.Play();

        await ToSignal(GetTree().CreateTimer(3.5f), "timeout");

        _animationPlayer2.Play("ArmAss2Fall");
        _arm2DamageParticles.Emitting = true;

        _clawAssPlayer.Play(0);

        _coreEffects.Startup();
        
        await ToSignal(GetTree().CreateTimer(4.5f), "timeout");


        _animationPlayer1.Play("ArmRecover");
        _animationPlayer1.Seek(0);

        _musicPlayer.Play(MusicPlayerControl.TStrings);

        await ToSignal(GetTree().CreateTimer(1f), "timeout");

        _musicPlayer.QueueTrackChange(MusicPlayerControl.TStrings | MusicPlayerControl.TGuitarStrum | MusicPlayerControl.TDrums);

        _clawVoiceSource.Stream = h6;
        _clawVoiceSource.Play(0);

        await ToSignal(GetTree().CreateTimer(8f), "timeout");
        canoe.SetAutopilotTarget(null, false);
        
        await ToSignal(GetTree().CreateTimer(8f), "timeout");

        _farBoomPlayer.Stream = farBoom;
        _farBoomPlayer.Play(0);

        _musicPlayer.QueueTrackChange(MusicPlayerControl.TGuitarStrum | MusicPlayerControl.TDrums | MusicPlayerControl.TPiano);

        _smokeParticles.Emitting = false;

        await ToSignal(GetTree().CreateTimer(6f), "timeout");
        _farBoomPlayer.Stream = farBoom;
        _farBoomPlayer.Play(0);

        await ToSignal(GetTree().CreateTimer(6f), "timeout");

        _arm2DamageParticles.Emitting = false;

        EmitSignal(nameof(SceneDoneSignal));

        GD.Print("DropoffScene: Disposing...");
        QueueFree();
    }
}

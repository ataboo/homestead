using Godot;
using System;

public class HomesteadDeath : Area
{
    [Export]
    public AudioStream h12;

    [Export]
    public AudioStream mc11;

    [Export]
    public AudioStream farElectricBoom;

    private AudioStreamPlayer3D _farBoomPlayer;

    private AudioStreamPlayer3D _clawVoiceSource;

    private AudioStreamPlayer3D _mcVoiceSource;

    private RandomizedAudioSource _clawSplashPlayer;

    private MusicPlayerControl _musicPlayer;

    private CollisionShape _trigger;

    private AnimationPlayer _animationPlayer;

    
    private HomesteadCoreEffects _coreEffects;

    private Particles _arm2DamageParticles;

    private Particles _smokeParticles;

    private ClawScene3 _clawScene3;

    [Signal]
    public delegate void SceneDoneSignal();

    private bool fired = false;

    public override void _Ready()
    {
        _trigger = GetNode<CollisionShape>("CollisionShape");

        var homesteadNode = GetNode<Spatial>("/root/Level/Scenery/Homestead");
        _mcVoiceSource = GetNode<AudioStreamPlayer3D>("/root/Level/Canoe/MCVoiceSource");
        _farBoomPlayer = GetNode<AudioStreamPlayer3D>("/root/Level/Scenery/EMPScene/BoomPlayer");
        _coreEffects = homesteadNode.GetNode<HomesteadCoreEffects>("CoreEffects");
        _arm2DamageParticles = homesteadNode.GetNode<Particles>("Arm2DamageParticles");

        _animationPlayer = homesteadNode.GetNode<AnimationPlayer>("AnimationPlayer1");

        _clawVoiceSource = homesteadNode.GetNode<AudioStreamPlayer3D>("ClawArm1/Skeleton/BoneAttachment5/HSVoiceSource");
        _clawSplashPlayer = homesteadNode.GetNode<RandomizedAudioSource>("ClawArm1/Skeleton/BoneAttachment5/ClawSplash");
        _musicPlayer = GetNode<MusicPlayerControl>("/root/Level/Canoe/Music");
        _smokeParticles = GetNode<Particles>("/root/Level/PlayerTriggers/FinalEncounter/SmokeParticles");

        _clawScene3 = GetNode<ClawScene3>("/root/Level/PlayerTriggers/ClawScene3");
    }

    void _on_HomesteadDeath_body_entered(Node body) {
        if(!fired && body is Canoe c) {
            GD.Print("Body is Canoe!");
            fired = true;
            ActionRoutine(c);
        }
    }

    public void _on_FarmExchange_SceneDoneSignal() {
        _trigger.Disabled = false;
    }
    
    async void ActionRoutine(Canoe canoe) {
        var mcSource = canoe.GetNode<AudioStreamPlayer3D>("MCVoiceSource");

        if(IsInstanceValid(_clawScene3)) {
            _clawScene3.QueueFree();
        }

        _animationPlayer.PlaybackSpeed = 1f;

        _farBoomPlayer.Stream = farElectricBoom;
        _farBoomPlayer.Play(0);

        _smokeParticles.Emitting = true;

        await ToSignal(GetTree().CreateTimer(2f), "timeout");

        mcSource.Stream = mc11;
        mcSource.Play(0);

        await ToSignal(GetTree().CreateTimer(2f), "timeout");

        _clawVoiceSource.Stream = h12;
        _clawVoiceSource.Play();

        await ToSignal(GetTree().CreateTimer(2f), "timeout");

        _coreEffects.Shutdown();
        
        _musicPlayer.HardStop();

        _coreEffects.Explode();
        
        await ToSignal(GetTree().CreateTimer(10f), "timeout");

        EmitSignal(nameof(SceneDoneSignal));

        GD.Print("DropoffScene: Disposing...");
        QueueFree();
    }

}

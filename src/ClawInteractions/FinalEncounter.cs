using Godot;
using System;

public class FinalEncounter : Area
{
    [Export]
    public AudioStream sc1;
    
    [Export]
    public AudioStream sc2;

    [Export]
    public AudioStream sc3;

    [Export]
    public AudioStream mc12;

    private AudioStreamPlayer3D _grinderPlayer;

    private bool fired = false;

    private bool rapidsFired = false;

    private bool finalDialogueFired = false;
     
    private CollisionShape _trigger;

    private MusicPlayerControl _musicPlayer;

    private Position3D _rapidsPullPos;

    private CollisionShape _rapids2Blocker;
    
    [Export]
    public AudioStream _grinder1;

    [Export]
    public AudioStream _grinder2;

    private Position3D _finalDialogPos;

    private AudioStreamPlayer3D _scavangerPlayer;

    private AudioStreamPlayer3D _scavangerPlayer2;

    public Particles[] _smallSmokeParticles;

    private Particles _bigSmokeParticles;

    private AnimationPlayer _scav1Player;

    private AnimationPlayer _scav2Player;

    public override void _Ready()
    {
        _trigger = GetNode<CollisionShape>("CollisionShape");

        var empScene = GetNode<Spatial>("/root/Level/Scenery/EMPScene");
        _grinderPlayer = empScene.GetNode<AudioStreamPlayer3D>("GrinderPlayer");

        _musicPlayer = GetNode<MusicPlayerControl>("/root/Level/Canoe/Music");
        _rapidsPullPos = GetNode<Position3D>("RapidsPullPos");
        _rapids2Blocker = GetNode<CollisionShape>("/root/Level/Scenery/Rapids/Rapids2/Blocker");

        _finalDialogPos = GetNode<Position3D>("FinalDialogPullPos");

        _scavangerPlayer = empScene.GetNode<AudioStreamPlayer3D>("FinalDialogPlayer");
        _scavangerPlayer2 = empScene.GetNode<AudioStreamPlayer3D>("FinalDialogPlayer2");

        _bigSmokeParticles = GetNode<Particles>("SmokeParticles");
        _smallSmokeParticles = new Particles[] {
            GetNode<Particles>("SmallSmoke1"),
            GetNode<Particles>("SmallSmoke2"),
            GetNode<Particles>("SmallSmoke3")
        };

        _scav1Player = empScene.GetNode<AnimationPlayer>("Final Scene/AnimationPlayer1");
        _scav2Player = empScene.GetNode<AnimationPlayer>("Final Scene/AnimationPlayer2");
    }

    void _on_FinalEncounter_body_entered(Node body) {
        if(!fired && body is Canoe c) {
            GD.Print("Body is Canoe!");
            fired = true;
            InitialActionRoutine(c);
        }
    }

    void _on_FinalRapidsArea_body_entered(Node body) {
        if(fired && !rapidsFired && body is Canoe c) {
            GD.Print("Rapids movement action started");
            RapidsMovementAction(c);

            foreach(var smallSmoke in _smallSmokeParticles) {
                smallSmoke.Emitting = true;
            }
            _bigSmokeParticles.Emitting = false;

            rapidsFired = true;
        }
    }

    public void _on_FinalDialogStart_body_entered(Node body) {
        if(!finalDialogueFired && body is Canoe c) {
            GD.Print("Rapids movement action started");
            FinalDialogAction(c);

            finalDialogueFired = true;
        }
    }

    async void RapidsMovementAction(Canoe c) {
        c.SetAutopilotTarget(_rapidsPullPos, true);

        _musicPlayer.QueueTrackChange((MusicPlayerControl.TStrings | MusicPlayerControl.TTrashLid));

        while((c.GlobalTransform.origin - _rapidsPullPos.GlobalTransform.origin).Length() > 2f) {
            await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
        }

        c.SetAutopilotTarget(null, false);

        _rapids2Blocker.Disabled = false;
        _grinderPlayer.Stream = _grinder1;
        _grinderPlayer.Play(0);
    }

    public void _on_HomesteadDeath_SceneDoneSignal() {
        _trigger.Disabled = false;
    }

    async void FinalDialogAction(Canoe c) {
        _musicPlayer.FadeOut(5f);
        var mcPlayer = c.GetNode<AudioStreamPlayer3D>("MCVoiceSource");

        await ToSignal(GetTree().CreateTimer(2f), "timeout");

        _scavangerPlayer.Stream = sc1;
        _scavangerPlayer.Play(0);

        await ToSignal(GetTree().CreateTimer(2f), "timeout");

        _scavangerPlayer2.Stream = sc2;
        _scavangerPlayer2.Play(0);

        await ToSignal(GetTree().CreateTimer(3f), "timeout");

        _scavangerPlayer.Stream = sc3;
        _scavangerPlayer.Play(0);

        await ToSignal(GetTree().CreateTimer(3f), "timeout");

        mcPlayer.Stream = mc12;
        mcPlayer.Play(0);

        _scav1Player.Play("Scav1Alert");
        _scav2Player.Play("Scav2Alert");

        await ToSignal(GetTree().CreateTimer(0.5f), "timeout");

        await ToSignal(GetTree().CreateTimer(3f), "timeout");

        GetTree().ChangeScene("res://stage/credits.tscn");
    }

    async void InitialActionRoutine(Canoe canoe) {
        _rapids2Blocker.Disabled = true;
        _grinderPlayer.Stream = _grinder2;
        _grinderPlayer.Play(0);

        await ToSignal(GetTree().CreateTimer(2f), "timeout");

        _musicPlayer.Play((MusicPlayerControl.TStrings));

        _scav1Player.Play("Scav1Idle");
        _scav2Player.Play("Scav2Idle");
        _scav2Player.PlaybackSpeed = 1.1f;
    }



//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

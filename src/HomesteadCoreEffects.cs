using Godot;
using System;

public class HomesteadCoreEffects : AudioStreamPlayer3D
{
    [Export]
    public AudioStream startup;

    [Export]
    public AudioStream shutdown;

    [Export]
    public AudioStream running;

    [Export]
    public AudioStream explode;

    private AudioStreamPlayer3D _secondAudio;

    private MeshInstance[] _coreMeshes;

    private MeshInstance[] _offCoreMeshes;

    private Particles _smokeParticles;

    public override void _Ready()
    {
        _coreMeshes = new MeshInstance[] {
            GetNode<MeshInstance>("../Homestead2/WarpCoreBall"), 
            GetNode<MeshInstance>("../Homestead2/WarpCoreColumn")
        };

        _offCoreMeshes = new MeshInstance[] {
            GetNode<MeshInstance>("../Homestead2/WarpCoreBallOff"), 
            GetNode<MeshInstance>("../Homestead2/WarpCoreColumnOff")
        };

        _secondAudio = GetNode<AudioStreamPlayer3D>("SecondAudio");

        _smokeParticles = GetNode<Particles>("CoreSmoke");
        
        this.Stream = running;
        this.Play(0);
    }

    public void Shutdown() {
        foreach(var mesh in _coreMeshes) {
            mesh.Visible = false;
        }

        foreach(var mesh in _offCoreMeshes) {
            mesh.Visible = true;
        }
        this.Stream = shutdown;
        this.Play();

        GetTree().CallGroup("Villagers", "SetLookPos", GlobalTransform.origin);
    }

    public void Startup() {
        StartupActions();
        
        GetTree().CallGroup("Villagers", "ClearLookPos");
    }

    public void Explode() {
        _secondAudio.Stream = explode;
        _secondAudio.Play();

        _smokeParticles.Emitting = true;
    }

    private async void StartupActions() {
        Stream = startup;
        Play();

        await ToSignal(GetTree().CreateTimer(startup.GetLength()), "timeout");

        foreach(var mesh in _coreMeshes) {
            mesh.Visible = true;
        }

        foreach(var mesh in _offCoreMeshes) {
            mesh.Visible = false;
        }

        Stream = running;
        Play();
    }
}

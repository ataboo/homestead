using Godot;
using System;

public class Rapids1Controller : StaticBody
{
    CollisionShape _blocker;
    Particles _particles;

    public override void _Ready()
    {
        _particles = GetNode<Particles>("Particles");
        _blocker = GetNode<CollisionShape>("Blocker");
        _blocker.Disabled = true;
    }

    public void _on_ClawScene1_SceneDoneSignal() {
        GD.Print("Rapids 1 blocker active");
        _blocker.Disabled = false;
    }

    public void _on_CanoeDetector_OnCanoeEntered() {
        GD.Print("Rapids 1 starting particles");
        _particles.Emitting = true;
    }

    public void _on_CanoeDetector_OnCanoeExited() {
        _particles.Emitting = false;
        GD.Print("Rapids 1 stopping particles");
    }
}

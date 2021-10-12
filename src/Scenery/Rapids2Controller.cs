using Godot;
using System;

public class Rapids2Controller : StaticBody
{
    CollisionShape _blocker;
    Particles _particles;

    public override void _Ready()
    {
        _particles = GetNode<Particles>("Particles");
        _blocker = GetNode<CollisionShape>("Blocker");
        _blocker.Disabled = false;
        _particles.Emitting = false;
    }

    public void _on_ClawScene5_Done() {
        GD.Print("Rapids 2 blocker disabled");
        _blocker.Disabled = true;
    }

    public void _on_CanoeDetector_OnCanoeEntered() {
        GD.Print("Rapids 2 starting particles");
        _particles.Emitting = true;
    }

    public void _on_CanoeDetector_OnCanoeExited() {
        _particles.Emitting = false;
        GD.Print("Rapids 2 stopping particles");
    }
}

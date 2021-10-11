using Godot;
using System;

public class CanoeDetector : Area
{
    [Signal]
    public delegate void OnCanoeEntered();
    
    [Signal]
    public delegate void OnCanoeExited();

    public void _on_CanoeDetector_body_entered(Node body) {
        if(body is Canoe) {
            EmitSignal(nameof(OnCanoeEntered));
        }
    }
    
    public void _on_CanoeDetector_body_exited(Node body) {
        if(body is Canoe) {
            EmitSignal(nameof(OnCanoeExited));
        }
    }
}

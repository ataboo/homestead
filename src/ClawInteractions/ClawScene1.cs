using Godot;
using System;

public class ClawScene1 : Spatial
{
    private Position3D[] _rapidPathPoints;
    private int _rapidPathIdx;

    private ClawArmController _claw;

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
                canoe.SetAutopilotTarget(_rapidPathPoints[i]);
                await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
            }
        }

        canoe.SetAutopilotTarget(null);
    }

    async void ActionRoutine(Canoe canoe) {
        _claw.movementSpeed = 3f;

        await ToSignal(GetTree().CreateTimer(4f), "timeout");

        GD.Print("ClawScene1: Surfacing...");
        _claw.TrackPosition(_claw.GlobalTransform.origin, canoe.GlobalTransform.origin, true);
        await ToSignal(GetTree().CreateTimer(2f), "timeout");
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

        //TODO play the line
        await ToSignal(GetTree().CreateTimer(3f), "timeout");

        _claw.SetBentDown(false);
        _claw.SetClawOpen(true);

        _claw.TrackTarget(canoe, new Vector3(3f, 0, 3f), true);

        EmitSignal(nameof(SceneDoneSignal));

        //TODO play the line
        await ToSignal(GetTree().CreateTimer(8f), "timeout");

        GD.Print("ClawScene1: Diving...");
        _claw.movementSpeed = 3f;
        _claw.StopTrackingTarget();
        _claw.TrackPosition(_claw.GlobalTransform.origin, canoe.GlobalTransform.origin, false);
        
        await ToSignal(GetTree().CreateTimer(5f), "timeout");

        GD.Print("ClawScene1: Disposing...");
        QueueFree();
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

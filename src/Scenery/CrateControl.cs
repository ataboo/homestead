using Godot;
using System;

public class CrateControl : Spatial
{
    private Position3D _targetPos;

    public void SetTargetPos(Position3D targetPos, bool teleport = false) {
        _targetPos = targetPos;
        if(teleport) {
            GlobalTransform = targetPos.GlobalTransform;
        }
    } 

    public override void _Process(float delta)
    {
        if(_targetPos == null) {
            return;
        }

        var globalTran = GlobalTransform;
        globalTran.origin = globalTran.origin.LinearInterpolate(_targetPos.GlobalTransform.origin, 40f * delta);
        globalTran.basis = globalTran.basis.LerpTowards(_targetPos.GlobalTransform.basis, 40f * delta);
        GlobalTransform = globalTran;
    }
}

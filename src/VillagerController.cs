using Godot;
using System;

public class VillagerController : Spatial
{
    private AnimationPlayer _animator;

    private Basis _originalBasis;

    private Position3D _lookPos;

    public override void _Ready()
    {
        _animator = GetNode<AnimationPlayer>("AnimationPlayer");
        _originalBasis = GlobalTransform.basis;

        _animator.Play("ArmatureAction");
    }

    public void SetLookPos (Vector3 lookPos)
    {
        var targetLook = GlobalTransform.LookingAt(lookPos, Vector3.Up);
        LookAtAction(targetLook.basis);   
    }

    public void ClearLookPos() {
        LookAtAction(_originalBasis);
    }

    async void LookAtAction(Basis targetBasis) {
        var delta = 1/60f;
        var targetQuat = targetBasis.Quat().Normalized();

        while((GlobalTransform.basis.Quat() - targetQuat).Length > 0.1f) {
            var tran = GlobalTransform;
            tran.basis = new Basis(tran.basis.Quat().Normalized().Slerp(targetQuat, 1f * delta));
            GlobalTransform = tran;
            await ToSignal(GetTree().CreateTimer(delta), "timeout");
        }
    }
}

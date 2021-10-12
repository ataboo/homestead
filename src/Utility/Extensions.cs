using Godot;
using System;
using System.Threading.Tasks;

public static class Extensions
{
    public static Basis LerpTowards(this Basis bA, Basis bB, float weight) {
        bA.x = bA.x.LinearInterpolate(bB.x, weight);
		bA.y = bA.y.LinearInterpolate(bB.y, weight);
		bA.z = bA.z.LinearInterpolate(bB.z, weight);

        return bA;
    }

    public static float SignedAngleTo(this Vector3 thisVect, Vector3 otherVect, Vector3 up) {
        var angle = thisVect.AngleTo(otherVect);

        return thisVect.Cross(otherVect).Dot(up) < 0 ? -angle : angle;
    }

    public static async Task WaitUntilPlayPos(this AnimationPlayer player, Node node, float position) {
        if(position > player.CurrentAnimationLength) {
            throw new ArgumentOutOfRangeException();
        }

        while(player.CurrentAnimationPosition < position) {
            await node.ToSignal(node.GetTree().CreateTimer(0.05f), "timeout");
        }
    }
}

using Godot;
using System;

public static class Extensions
{
    public static Basis LerpTowards(this Basis bA, Basis bB, float weight) {
        bA.x = bA.x.LinearInterpolate(bB.x, weight);
		bA.y = bA.y.LinearInterpolate(bB.y, weight);
		bA.z = bA.z.LinearInterpolate(bB.z, weight);

        return bA;
    }
}

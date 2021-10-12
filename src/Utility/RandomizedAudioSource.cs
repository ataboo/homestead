using Godot;
using System;

public class RandomizedAudioSource : AudioStreamPlayer3D
{
    [Export]
    public AudioStream[] clips;

    [Export]
    public float pitchDeviation = 0.1f;

    private RandomNumberGenerator rand;

    public override void _Ready()
    {
        rand = new RandomNumberGenerator();
        rand.Randomize();
    }

    public void PlayRandomClip() {
        this.Stream = clips[rand.RandiRange(0, clips.Length-1)];
        this.PitchScale = 1 + rand.RandfRange(-pitchDeviation, pitchDeviation);
        this.Play(0);
    }
}

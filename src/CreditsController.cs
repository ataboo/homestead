using Godot;
using System;

public class CreditsController : MarginContainer
{
    private MusicPlayerControl _music;
    
    [Export]
    public float creditsSpeed = 50f;

    public override void _Ready()
    {
        _music = GetNode<MusicPlayerControl>("/root/CreditsScene/Music");
        
        MusicActions();

        Input.SetMouseMode(Input.MouseMode.Visible);
    }

    public override void _Input(InputEvent @event)
    {
        if(@event.IsActionPressed("ui_cancel")) {
            GetTree().Quit();
        }
    }

    public override void _Process(float delta)
    {
        if(RectPosition.y >= 0) {
            var pos = RectPosition;
            pos.y -= creditsSpeed * delta;
            RectPosition = pos;
        }
    }

    private async void MusicActions() {
        _music.Play(MusicPlayerControl.TGuitarPluck);
        await ToSignal(GetTree().CreateTimer(2f), "timeout");

        _music.QueueTrackChange((MusicPlayerControl.TGuitarPluck | MusicPlayerControl.TPiano));
        
        await ToSignal(GetTree().CreateTimer(14.4f), "timeout");

        _music.QueueTrackChange((MusicPlayerControl.TDrums | MusicPlayerControl.TPiano | MusicPlayerControl.TGuitarStrum));

        await ToSignal(GetTree().CreateTimer(14.4f), "timeout");
        
        _music.QueueTrackChange((MusicPlayerControl.TDrums | MusicPlayerControl.TPiano | MusicPlayerControl.TGuitarStrum | MusicPlayerControl.TStrings));
    }
}

using Godot;
using System;

public class MusicPlayerControl : Node
{
    public const int TDrums = 1<<0;
    public const int TPiano = 1<<1;
    public const int TStrings = 1<<2;
    public const int TGuitarPluck = 1<<3;
    public const int TGuitarStrum = 1<<4;
    public const int TTrashLid = 1<<5;

    AudioStreamPlayer drums;

    AudioStreamPlayer piano;
    
    AudioStreamPlayer strings;

    AudioStreamPlayer guitarPluck;

    AudioStreamPlayer guitarStrum;

    AudioStreamPlayer trashLid;

    private AudioStreamPlayer[] _allPlayers;

    private int _startingTrackMask = (TGuitarPluck);

    private int _queuedTrackMask;

    private int _lastTrackMask;

    public override void _Ready() {
        drums = GetNode<AudioStreamPlayer>("MDrums");
        piano = GetNode<AudioStreamPlayer>("MPiano");
        strings = GetNode<AudioStreamPlayer>("MStrings");
        guitarPluck = GetNode<AudioStreamPlayer>("MGuitarPluck");
        guitarStrum = GetNode<AudioStreamPlayer>("MGuitarStrum");
        trashLid = GetNode<AudioStreamPlayer>("MTrashlid");

        _allPlayers = new[]{
            drums,
            piano,
            strings,
            guitarPluck,
            guitarStrum,
            trashLid
        };

        ToggleTracks(_startingTrackMask);
    }

    public void QueueTrackChange(int trackMask) {
        ChangeTrackAction(trackMask);
    }

    public void _on_TownEnterTrigger_OnCanoeEntered() {
        GD.Print("Fired town enter change");
        QueueTrackChange(TDrums | TGuitarStrum | TPiano);
    }

    private async void ChangeTrackAction(int trackMask) {
        _queuedTrackMask = trackMask;
        var timeToEnd = drums.Stream.GetLength() - drums.GetPlaybackPosition();
        await ToSignal(GetTree().CreateTimer(timeToEnd), "timeout");

        if(_queuedTrackMask >= 0) {
            ToggleTracks(_queuedTrackMask);
            _queuedTrackMask = -1;
        }
    }

    private void ToggleTracks(int trackMask) {
        ToggleTrack(drums, trackMask, TDrums);
        ToggleTrack(piano, trackMask, TPiano);
        ToggleTrack(strings, trackMask, TStrings);
        ToggleTrack(guitarPluck, trackMask, TGuitarPluck);
        ToggleTrack(guitarStrum, trackMask, TGuitarStrum);
        ToggleTrack(trashLid, trackMask, TTrashLid);
        _lastTrackMask = _queuedTrackMask;
    }

    private void ToggleTrack(AudioStreamPlayer player, int trackMask, int trackBit) {
        if(player == null) {
            GD.Print("Why is null?");
            return;
        }

        player.VolumeDb = (trackMask & trackBit) != 0 ? 0f : -80f;
    }

    public void HardStop() {
        foreach(var player in _allPlayers) {
            player.StreamPaused = true;
            player.Seek(0);
        }
    }

    public void FadeOut(float fadeLength) {
        FadeOutAction(fadeLength);
    }

    private async void FadeOutAction(float fadeLength) {
        var volume = 1f;
        var dVol = 0.1f / fadeLength;
        while(volume > 0f) {
            volume -= dVol;
            foreach(var player in _allPlayers) {
                player.VolumeDb = Mathf.Min(player.VolumeDb, ((1-volume) * -80f));
            }
            
            await ToSignal(GetTree().CreateTimer(.1f), "timeout");
        }

        foreach(var player in _allPlayers) {
            player.StreamPaused = true;
            player.VolumeDb = -80f;
        }
    }

    public void Play(int trackMask) {
        ToggleTracks(trackMask);
        foreach(var player in _allPlayers) {
            player.StreamPaused = false;
            player.Play(0);
        }
    }
}

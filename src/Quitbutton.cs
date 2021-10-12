using Godot;
using System;

public class Quitbutton : Button
{
    public void _on_Quit_pressed() {
        GetTree().Quit();
    }
}

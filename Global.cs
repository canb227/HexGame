using Godot;
using System;
using System.Diagnostics;

public partial class Global : Node
{
    public static Game game;
    public static Layout layout;

    public static Global instance;

    public override void _Ready()
    {
        Global instance = this;
    }
}

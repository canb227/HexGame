using Godot;
using System;
using System.Diagnostics;
using Steamworks;

public partial class Global : Node
{
    public static Layout layout;
    public static Camera camera;

    public static Global instance;

    public override void _Ready()
    {
        Global instance = this;
        SteamClient.Init(480, true);
    }
}

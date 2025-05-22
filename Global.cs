using Godot;
using System;
using System.Diagnostics;
using Steamworks;

public partial class Global : Node
{
    //Steam stuff
    public static uint STEAM_APP_ID = 480;
    public static ulong steamID = 0;

    //Abusing singletons
    public static Global instance;

    //Register global variables here with "public static"
    //If the compiler gives you trouble with Global.[varname], try using Global.instance.[varname]
    public static Layout layout;
    public static Camera camera;


    //This ready codeblock is the first non-engine code to run anywhere in the game, since Global is autoloaded by Godot before anything else
    public override void _Ready()
    {
        //also abusing singletons
        Global instance = this;

        SteamInit();
    }

    public void SteamInit()
    {
        SteamClient.Init(STEAM_APP_ID, true);
        if (SteamClient.IsValid)
        {
            steamID = SteamClient.SteamId.Value;
            GD.Print("Steam ID: " + steamID);
        }
        else
        {
            GD.Print("Steam not initialized");
        }
    }

    public void Log(string message)
    {
        GD.Print(message);
    }
}

using Godot;
using System;
using System.Diagnostics;
using Steamworks;

public partial class Global : Node
{
    //Steam stuff
    public const uint STEAM_APP_ID = 480;
    public const bool DISABLE_STEAM_DEBUG = false;
    public static ulong clientID = 0;

    //Abusing singletons
    public static Global instance;

    //Register global variables here with "public static"
    //If the compiler gives you trouble with Global.[varname], try using Global.instance.[varname]
    public static Layout layout;
    public static Camera camera;
    public static GameManager gameManager;
    public static NetworkPeer networkPeer;

    //This ready codeblock is the first non-engine code to run anywhere in the game, since Global is autoloaded by Godot before anything else
    public override void _Ready()
    {
        //also abusing singletons
        Global instance = this;
        gameManager = new GameManager();
        SteamInit();
    }

    public void SteamInit()
    {
        if (DISABLE_STEAM_DEBUG)
        {
            return;
        }
        
        if (SteamAPI.Init())
        {
            clientID = SteamUser.GetSteamID().m_SteamID;
            Global.debugLog("Steam ID: " + clientID);
        }
        else
        {
            Global.debugLog("Steam not initialized");
        }
    }

    public static void networkLog(string message, ulong timestamp, bool server )
    {
        if(server)
        {
           GD.Print("[SERVER][" + timestamp + "] " + message);
        }
        else
        {
           GD.Print("[CLIENT][" + timestamp + "] " + message);
        }
    }

    public static void debugLog(string message, bool addTimestamp = true)
    {
        if (addTimestamp)
        {
            GD.Print("[DEBUG][" + Time.GetTimeStringFromSystem() + "] " + message);
        }
        else
        {
            GD.Print("[DEBUG] " + message);
        }

    }

    internal static ulong getTick()
    {
        return Time.GetTicksMsec();
    }
}

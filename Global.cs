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

    //Steam Callbacks
    protected Callback<GameRichPresenceJoinRequested_t> m_GameRichPresenceJoinRequested;

    //Abusing singletons
    public static Global instance;

    //Register global variables here with "public static"
    //If the compiler gives you trouble with Global.[varname], try using Global.instance.[varname]
    public static Layout layout;
    public static GameManager gameManager;
    public static NetworkPeer networkPeer;
    public static MenuManager menuManager;
    public static HexGameCamera camera;
    

    //This ready codeblock is the first non-engine code to run anywhere in the game, since Global is autoloaded by Godot before anything else
    public override void _Ready()
    {
        //also abusing singletons
        Global instance = this;

        SteamInit();
        
        m_GameRichPresenceJoinRequested = Callback<GameRichPresenceJoinRequested_t>.Create(OnGameRichPresenceJoinRequested);

        SteamApps.GetLaunchCommandLine(out string commandLine, 1024);
        if (!string.IsNullOrEmpty(commandLine))
        {
            Global.debugLog("Launch Command Line: " + commandLine);
        }
        else
        {
            Global.debugLog("No Launch Command Line found");
        }
    }

    public void OnGameRichPresenceJoinRequested(GameRichPresenceJoinRequested_t pCallback)
    {
        Global.debugLog("Game Rich Presence Join Requested: " + pCallback.m_rgchConnect);
        string connectString = (pCallback.m_rgchConnect);
        Global.debugLog("Connect String: " + connectString);
        Global.menuManager.JoinLobby(ulong.Parse(connectString));


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

    public override void _Process(double delta)
    {
        SteamAPI.RunCallbacks();
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

    internal static Texture2D GetMediumSteamAvatar(ulong id)
    {
        int avatarHandle = SteamFriends.GetMediumFriendAvatar(new CSteamID(id));
        SteamUtils.GetImageSize(avatarHandle, out uint width, out uint height);
        int size = (int)(width * height * 4); // RGBA format
        byte[] avatarData = new byte[size];
        SteamUtils.GetImageRGBA(avatarHandle, avatarData, size);
        Image testImage = Image.CreateFromData((int)width, (int)height, false, Image.Format.Rgba8, avatarData);
        ImageTexture texture = ImageTexture.CreateFromImage(testImage);
        return texture;
    }
}

using Godot;
using System;
using System.Diagnostics;
using Steamworks;
using ImGuiNET;
using System.Collections.Concurrent;
using System.Linq;

public partial class Global : Node
{

    public const bool PRINTDEBUG = true; //Set to false to disable debug messages

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
    public static Lobby lobby;
    public static NetworkPeer networkPeer;
    public static MenuManager menuManager;
    public static HexGameCamera camera;

    bool ShowDebugConsole = false;
    
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }

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
            Global.Log("Launch Command Line: " + commandLine);
        }
        else
        {
            Global.Log("No Launch Command Line found");
        }

        if (CommandParser.LOOKATME) { } 
    }

    public void OnGameRichPresenceJoinRequested(GameRichPresenceJoinRequested_t pCallback)
    {
        Global.Log("Game Rich Presence Join Requested: " + pCallback.m_rgchConnect);
        string connectString = (pCallback.m_rgchConnect);
        Global.Log("Connect String: " + connectString);
        Global.lobby.AttemptToJoinLobby(ulong.Parse(connectString));


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
            Global.Log("Steam ID: " + clientID);
        }
        else
        {
            Global.Log("Steam not initialized");
        }
    }

    public override void _Process(double delta)
    {
        SteamAPI.RunCallbacks();
        if (Input.IsActionPressed("debug"))
        {
            ShowDebugConsole = true;
        }
        if (ShowDebugConsole)
        {
            debugConsole();
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

    public static void Log(string message, bool addTimestamp = true, LogLevel logLevel = LogLevel.Debug)
    {
        String messagePrefix = "[NULL]";
        switch (logLevel)
        {
            case LogLevel.Debug:
                messagePrefix = "[DEBUG]";
                break;
            case LogLevel.Info:
                messagePrefix = "[INFO]";
                break;
            case LogLevel.Warning:
                messagePrefix = "[WARNING]";
                break;
            case LogLevel.Error:
                messagePrefix = "[ERROR]";
                break;
        }
        if (addTimestamp)
        {
            messagePrefix += "[" + Time.GetTimeStringFromSystem() + "] ";
        }

        if (logLevel==LogLevel.Debug && !PRINTDEBUG)
        {
            return;
        }
        else
        {
            GD.Print(messagePrefix + message);
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


    public void debugConsole()
    {
        ImGui.Begin("Debug Console");
        if (ImGui.Button("Give Full Vision"))
        {
            var visibleHexes = Global.gameManager.game.localPlayerRef.visibleGameHexDict;
            var visibilityChanged = Global.gameManager.game.localPlayerRef.visibilityChangedList;
            var seen = Global.gameManager.game.localPlayerRef.seenGameHexDict;
            foreach (GameHex hex in Global.gameManager.game.mainGameBoard.gameHexDict.Values)
            {
                visibleHexes.TryAdd(hex.hex, 10);
                seen.TryAdd(hex.hex, true);
                visibilityChanged.Add(hex.hex);
            }
            Global.gameManager.graphicManager.UpdateGraphic(Global.gameManager.game.mainGameBoard.id, GraphicUpdateType.Update);
        }
        if (ImGui.Button("Wireframe Mode"))
        {
            if (GetViewport().DebugDraw == Viewport.DebugDrawEnum.Wireframe)
            {
                GetViewport().DebugDraw = Viewport.DebugDrawEnum.Disabled;
            }
            else
            {
                GetViewport().DebugDraw = Viewport.DebugDrawEnum.Wireframe;
            }
            
        }
        if (ImGui.Button("Close Debug Menu"))
        {
            ShowDebugConsole = false;
        }
        ImGui.End();
    }
}

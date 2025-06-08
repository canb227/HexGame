using Godot;
using System;
using Steamworks;
using System.Collections.Generic;
using NetworkMessages;
using System.Runtime.InteropServices.JavaScript;

public partial class Lobby : Control
{

    VBoxContainer PlayersListBox;
    Dictionary<ulong, LobbyStatus> PlayerStatuses = new();
    Button StartGameButton;
    bool singleplayer = false;
    bool isHost = false;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        NetworkPeer.PlayerJoinedEvent += OnPlayerJoinEvent;
        NetworkPeer.LobbyMessageReceivedEvent += OnLobbyMessageReceived;
        PlayersListBox = GetNode<VBoxContainer>("PlayerListBox/ScrollContainer/Players/PlayersVbox");
        StartGameButton = GetNode<Button>("b_newgame");
        StartGameButton.Disabled = true;
    }

    public void CreateLobby()
    {
        Global.debugLog("Creating new lobby.");
        if (singleplayer)
        {
            Global.debugLog("Lobby in singleplayer mode.");
        }
        else
        {
            Global.debugLog("Lobby in online mode. Activating Steam Rich Presence Join to Player.");
            SteamFriends.SetRichPresence("status", "In a lobby");
            SteamFriends.SetRichPresence("connect", Global.clientID.ToString());
        }
        isHost = true;
        AddNewPlayerToLobby(Global.clientID, true);
    }

    private void AddNewPlayerToLobby(ulong id, bool self)
    {
        Control PlayerListItem = GD.Load<PackedScene>("res://graphics/ui/menus/playerListItem.tscn").Instantiate<Control>();
        PlayerListItem.GetNode<Label>("playername").Text = SteamFriends.GetFriendPersonaName(new CSteamID(id));
        PlayerListItem.GetNode<TextureRect>("icon").Texture = Global.GetMediumSteamAvatar(id);
        if(self)
        {
            PlayerListItem.GetNode<CheckButton>("ReadyButton").Toggled += onReadyChanged;
            PlayerListItem.GetNode<OptionButton>("factionselect").ItemSelected += OnFactionChange;
            PlayerListItem.GetNode<OptionButton>("teamselect").ItemSelected += OnTeamChange;
        }
        else
        {
            PlayerListItem.GetNode<CheckButton>("ReadyButton").Disabled = true;
            PlayerListItem.GetNode<OptionButton>("factionselect").Disabled = true;
            PlayerListItem.GetNode<OptionButton>("teamselect").Disabled = true;
        }
        PlayerListItem.Name = id.ToString();
        PlayersListBox.AddChild(PlayerListItem);
        PlayerStatuses.Add(id, new LobbyStatus() { IsHost = isHost, IsReady = false, Faction = 0, Team = 1 });
    }

    public void JoinLobby(ulong hostID)
    {
        Global.networkPeer.JoinToPeer(hostID);
        Global.debugLog("Joining lobby: " + hostID);
        SteamFriends.SetRichPresence("status", "In a lobby");
        SteamFriends.SetRichPresence("connect", Global.clientID.ToString());
    }

    private void OnLobbyMessageReceived(LobbyMessage lobbyMessage)
    {

        Global.debugLog("Lobby message received: " + lobbyMessage.MessageType + " from " + lobbyMessage.Sender);
        if (lobbyMessage.Sender == Global.clientID && lobbyMessage.MessageType != "startgame")
        {
            Global.debugLog("Ignoring own lobby message: " + lobbyMessage.MessageType);
            return; // Ignore own messages except for startgame
        }
        switch (lobbyMessage.MessageType)
        {
            case "status":
                PlayerStatuses[lobbyMessage.Sender] = lobbyMessage.LobbyStatus;
                Control PlayerListItem = PlayersListBox.GetNode<Control>(lobbyMessage.Sender.ToString());
                PlayerListItem.GetNode<OptionButton>("teamselect").Selected = (int)lobbyMessage.LobbyStatus.Team - 1;
                PlayerListItem.GetNode<OptionButton>("factionselect").Selected = (int)lobbyMessage.LobbyStatus.Faction;
                PlayerListItem.GetNode<CheckButton>("ReadyButton").ButtonPressed = lobbyMessage.LobbyStatus.IsReady;
                CheckIfGameCanStart();
                break;
            case "leave":
                Global.debugLog("Player left the lobby: " + lobbyMessage.Sender);
                if (PlayerStatuses.ContainsKey(lobbyMessage.Sender))
                {
                    PlayerStatuses.Remove(lobbyMessage.Sender);
                    Control playerItem = PlayersListBox.GetNode<Control>(lobbyMessage.Sender.ToString());
                    if (playerItem != null)
                    {
                        playerItem.QueueFree();
                    }
                }
                break;
            case "startgame":
                Global.debugLog("Starting game from lobby message");
                if (isHost)
                {
                    Global.gameManager.startGame((int)PlayerStatuses[Global.clientID].Team);
                }
                else
                {
                    Global.gameManager.startGame((int)PlayerStatuses[Global.clientID].Team);
                }
                break;
            default:
                Global.debugLog("Unknown lobby message type: " + lobbyMessage.MessageType);
                break;
        }

    }
    private void onReadyChanged(bool toggledOn)
    {
        LobbyStatus status = PlayerStatuses[Global.clientID];
        status.IsReady = toggledOn;
        PlayerStatuses[Global.clientID] = status;
        UpdateLobbyPeers();
    }
    private void OnTeamChange(long index)
    {
        Global.debugLog("team change button pressed, index: " + index);
        LobbyStatus status = PlayerStatuses[Global.clientID];
        status.Team = (ulong)index + 1;
        PlayerStatuses[Global.clientID] = status;
        UpdateLobbyPeers();
    }

    private void OnFactionChange(long index)
    {
        LobbyStatus status = PlayerStatuses[Global.clientID];
        status.Team = (ulong)index;
        PlayerStatuses[Global.clientID] = status;
        UpdateLobbyPeers();
    }

    private void UpdateLobbyPeers()
    {
        LobbyMessage lobbyMessage = new LobbyMessage();
        lobbyMessage.Sender = Global.clientID;
        lobbyMessage.MessageType = "status";
        lobbyMessage.LobbyStatus = PlayerStatuses[Global.clientID];
        Global.networkPeer.LobbyMessageAllPeers(lobbyMessage);

        Global.debugLog("Lobby change detected, updating other peers");
        CheckIfGameCanStart();
    }

    private void CheckIfGameCanStart()
    {
        bool AllPlayersReady = true;
        foreach (var status in PlayerStatuses.Values)
        {
            if (status.IsReady == false)
            {
                AllPlayersReady = false;
            }
        }
        if (!isHost)
        {
            GetNode<Label>("ReadyStatus").Text = "Only host can start";
        }
        else if (!AllPlayersReady)
        {
            GetNode<Label>("ReadyStatus").Text = "Players not ready";
        }
        else
        {
            GetNode<Label>("ReadyStatus").Text = "All players are ready!";
            StartGameButton.Disabled = false;
        }
    }

    public void OnLoadButtonPressed()
    {
        Godot.FileDialog dialog = new Godot.FileDialog();
        dialog.Mode = Godot.FileDialog.ModeEnum.Windowed;
        dialog.FileMode = FileDialog.FileModeEnum.OpenFile;
        dialog.Title = "Load Game";
        dialog.OkButtonText = "Load";
        dialog.UseNativeDialog = true;
        dialog.Show();
        
        AddChild(dialog);
        dialog.MoveToCenter();
        dialog.FileSelected += OnFileSelected;

    }

    private void OnFileSelected(string path)
    {
        string trimmedPath = path.Substring(path.LastIndexOf("/") + 1);
        Global.debugLog("File selected: " + trimmedPath);
        Global.gameManager.game = Global.gameManager.LoadGame(trimmedPath);

        GetNode<Label>("NewGameStatus").Text = "GAME LOADED: " + trimmedPath;
        GetNode<ColorRect>("newgamehide").Visible = true;
    }

    private void OnPlayerJoinEvent(ulong playerID)
    {
        Global.debugLog("Player joined to Lobby: " + playerID);
        AddNewPlayerToLobby(Global.clientID, false);
    }

    public override void _ExitTree()
    {
        SteamFriends.ClearRichPresence();
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

	public void OnInviteButtonPressed()
    {
        Global.debugLog("Invite button pressed");
        Steamworks.SteamFriends.ActivateGameOverlayInviteDialog(Steamworks.SteamUser.GetSteamID());
    }

    public void OnStartGameButtonPressed()
    {
        Global.debugLog("Start game button pressed. Team Num: " + ((int)PlayerStatuses[Global.clientID].Team).ToString());
        //Global.gameManager.startGame((int)PlayerStatuses[Global.clientID].Team);
        LobbyMessage lobbyMessage = new LobbyMessage();
        lobbyMessage.Sender = Global.clientID;
        lobbyMessage.MessageType = "startgame";
        Global.networkPeer.LobbyMessageAllPeers(lobbyMessage);
        
    }


}

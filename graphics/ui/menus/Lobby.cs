using Godot;
using System;
using Steamworks;
using System.Collections.Generic;
using NetworkMessages;

public partial class Lobby : Control
{

    VBoxContainer PlayersListBox;
    Dictionary<ulong, LobbyStatus> PlayerStatuses = new();
    Button StartGameButton;
    bool connected = false;
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

    private void OnLobbyMessageReceived(LobbyMessage lobbyMessage)
    {

        Global.debugLog("Lobby message received: " + lobbyMessage.MessageType + " from " + lobbyMessage.Sender);
        switch (lobbyMessage.MessageType)
        {
            case "status":
                if (!PlayerStatuses.ContainsKey(lobbyMessage.Sender))
                {
                    Global.debugLog("Adding new player to lobby: " + lobbyMessage.Sender);
                    AddPlayer(lobbyMessage.Sender);
                }
                else
                {
                    Global.debugLog("Updating existing player status: " + lobbyMessage.Sender);
                }
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

    public void ResetLobbyPlayerList()
    {
        Global.debugLog("Resetting lobby player list");
        foreach (Node node in PlayersListBox.GetChildren())
        {
            node.QueueFree();
        }
        PlayerStatuses.Clear();
        AddSelf(isHost);
        foreach (ulong id in Global.networkPeer.GetConnectedPeers())
        {
            AddPlayer(id);
        }
        CheckIfGameCanStart();
    }

    public void CreateLobby()
    {
        Global.debugLog("Creating lobby");
        SteamFriends.SetRichPresence("status", "In a lobby");
        SteamFriends.SetRichPresence("connect", Global.clientID.ToString());
        isHost = true;
        ResetLobbyPlayerList();
    }

    public void JoinLobby(ulong hostID)
    {
        Global.debugLog("Joining lobby: " + hostID);
        SteamFriends.SetRichPresence("status", "In a lobby");
        SteamFriends.SetRichPresence("connect", Global.clientID.ToString());
        Global.networkPeer.JoinToPeer(hostID);
        ResetLobbyPlayerList();
    }

    private void AddSelf(bool asHost = false)
    {
        Control PlayerListItem = GD.Load<PackedScene>("res://graphics/ui/menus/playerListItem.tscn").Instantiate<Control>();
        PlayerListItem.GetNode<Label>("playername").Text = SteamFriends.GetFriendPersonaName(new CSteamID(Global.clientID));
        PlayerListItem.GetNode<TextureRect>("icon").Texture = Global.GetMediumSteamAvatar(Global.clientID);
        PlayersListBox.AddChild(PlayerListItem);
        PlayerListItem.GetNode<CheckButton>("ReadyButton").Toggled += onReadyChanged;
        PlayerListItem.GetNode<OptionButton>("factionselect").ItemSelected += OnFactionChange;
        PlayerListItem.GetNode<OptionButton>("teamselect").ItemSelected += OnTeamChange;
        PlayerStatuses.Add(Global.clientID, new LobbyStatus() { IsHost=asHost, IsReady=false, Faction=0, Team=1 });
    }

    private void OnTeamChange(long index)
    {
        Global.debugLog("team change button pressed, index: " + index);
        LobbyStatus status = PlayerStatuses[Global.clientID];
        status.Team = (ulong)index+1;
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

    private void AddPlayer(ulong id)
    {
        Global.debugLog("Adding player to lobby UI: " + id);
        Control PlayerListItem = GD.Load<PackedScene>("res://graphics/ui/menus/playerListItem.tscn").Instantiate<Control>();
        PlayerListItem.GetNode<Label>("playername").Text = SteamFriends.GetFriendPersonaName(new CSteamID(id));
        PlayerListItem.GetNode<TextureRect>("icon").Texture = Global.GetMediumSteamAvatar(id);
        PlayerListItem.Name= id.ToString();
        PlayersListBox.AddChild(PlayerListItem);
        PlayerStatuses.Add(Global.clientID, new LobbyStatus() { IsHost = false, IsReady = false, Faction = 0, Team = 1 });
    }

    private void onReadyChanged(bool toggledOn)
    {
        LobbyStatus status = PlayerStatuses[Global.clientID];
        status.IsReady = toggledOn;
        PlayerStatuses[Global.clientID] = status;
        UpdateLobbyPeers();
    }



    private void UpdateLobbyPeers()
    {
        LobbyMessage lobbyMessage = new LobbyMessage();
        lobbyMessage.Sender = Global.clientID;
        lobbyMessage.MessageType = "status";
        lobbyMessage.LobbyStatus = PlayerStatuses[Global.clientID];
        Global.networkPeer.MessageAllPeers(lobbyMessage);

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
        AddPlayer(Global.clientID);
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

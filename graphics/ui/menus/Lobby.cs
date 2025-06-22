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
        NetworkPeer.ChatMessageReceivedEvent += NetworkPeer_ChatMessageReceivedEvent;
        PlayersListBox = GetNode<VBoxContainer>("PlayerListBox/ScrollContainer/Players/PlayersVbox");
        StartGameButton = GetNode<Button>("b_newgame");
        StartGameButton.Disabled = true;

        foreach (var size in Enum.GetNames(typeof(MapGenerator.MapSize)))
        {
            GetNode<OptionButton>("newgameoptions/worldgensize").AddItem(size);
        }
        GetNode<OptionButton>("newgameoptions/worldgensize").Selected = (int)MapGenerator.MapSize.Small;

        foreach (var type in Enum.GetNames(typeof(MapGenerator.MapType)))
        {
            GetNode<OptionButton>("newgameoptions/worldgentype").AddItem(type);
        }
        GetNode<OptionButton>("newgameoptions/worldgentype").Selected = (int)MapGenerator.MapType.DebugSquare;

    }

    private void NetworkPeer_ChatMessageReceivedEvent(Chat chat)
    {
        GetNode<RichTextLabel>("chat/chatbox/chattext").AppendText(
            "[" + SteamFriends.GetFriendPersonaName(new CSteamID(chat.Sender)) + "]: " + chat.Message + "\n");
    }

    private void onChatSendButtonPressed()
    {
        TextEdit chatInput = GetNode<TextEdit>("chat/chatbar");
        string message = chatInput.Text.Trim();
        Global.networkPeer.ChatAllPeers(message);
        chatInput.Text = "";
    }

    public void CreateLobby()
    {
        Global.Log("Creating new lobby.");
        if (singleplayer)
        {
            Global.Log("Lobby in singleplayer mode.");
        }
        else
        {
            Global.Log("Lobby in online mode. Activating Steam Rich Presence Join to Player.");
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
        Global.Log("Joining lobby: " + hostID);
        SteamFriends.SetRichPresence("status", "In a lobby");
        SteamFriends.SetRichPresence("connect", Global.clientID.ToString());
        AddNewPlayerToLobby(Global.clientID, true);
    }

    private void OnLobbyMessageReceived(LobbyMessage lobbyMessage)
    {

        Global.Log("Lobby message received: " + lobbyMessage.MessageType + " from " + lobbyMessage.Sender);
        if (lobbyMessage.Sender == Global.clientID && lobbyMessage.MessageType == "status")
        {
            Global.Log("Ignoring own lobby message: " + lobbyMessage.MessageType);
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
                Global.Log("Player left the lobby: " + lobbyMessage.Sender);
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
                Global.Log("Starting game from lobby message");
                //Global.debugLog(lobbyMessage.SavePayload);
                Global.menuManager.ClearMenus();
                Global.menuManager.loadingScreen.Show();


                if (GetNode<CheckButton>("newgameoptions/debugmode").ButtonPressed)
                {
                    Global.gameManager.game = new Game(0);
                    Global.gameManager.game.mainGameBoard.InitGameBoardFromData(lobbyMessage.MapData.MapData_, (int)(lobbyMessage.MapData.MapWidth-1), (int)(lobbyMessage.MapData.MapHeight - 1));
                    Global.gameManager.game.AddPlayer(10, 0, 0, new Godot.Color(Colors.White));
                    Global.gameManager.startGame(0);
                }
                else
                {
                    Global.gameManager.game = new Game((int)PlayerStatuses[Global.clientID].Team);
                    Global.gameManager.game.mainGameBoard.InitGameBoardFromData(lobbyMessage.MapData.MapData_, (int)(lobbyMessage.MapData.MapWidth - 1), (int)(lobbyMessage.MapData.MapHeight - 1));
                    Global.gameManager.game.AddPlayer(10, 0, 0, new Godot.Color(Colors.White));
                    foreach (ulong playerID in PlayerStatuses.Keys)
                    {
                        Global.Log("Adding player to game with ID: " + playerID + " and teamNum: " + PlayerStatuses[playerID].Team);
                        Global.gameManager.game.AddPlayer(10, (int)PlayerStatuses[playerID].Team, playerID, new Godot.Color(Colors.Blue));
                    }
                    Global.gameManager.startGame((int)PlayerStatuses[Global.clientID].Team);
                }
                Global.menuManager.ClearMenus();
                break;
            case "loadgame":
                Global.Log("Loading game from host");
                Global.gameManager.game = Global.gameManager.LoadGameRaw(lobbyMessage.MapData.MapData_);
                GetNode<Label>("NewGameStatus").Text = "GAME LOADED";
                GetNode<ColorRect>("newgamehide").Visible = true;
                break;
            default:
                Global.Log("Unknown lobby message type: " + lobbyMessage.MessageType);
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
        Global.Log("team change button pressed, index: " + index);
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

        Global.Log("Lobby change detected, updating other peers");
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
        Global.Log("File selected: " + trimmedPath);

        Game loaded = Global.gameManager.LoadGame(trimmedPath);

        LobbyMessage lobbyMessage = new LobbyMessage();
        lobbyMessage.Sender = Global.clientID;
        lobbyMessage.MessageType = "loadgame";
        lobbyMessage.MapData.MapData_ = Global.gameManager.SaveGameRaw(loaded);
        Global.networkPeer.LobbyMessageAllPeers(lobbyMessage);


    }

    private void OnPlayerJoinEvent(ulong playerID)
    {
        Global.Log("Player joined to Lobby: " + playerID);
        AddNewPlayerToLobby(playerID, false);
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
        Global.Log("Invite button pressed");
        Steamworks.SteamFriends.ActivateGameOverlayInviteDialog(Steamworks.SteamUser.GetSteamID());

    }

    public async void OnStartGameButtonPressed()
    {
        Global.Log("Start game button pressed. Team Num: " + ((int)PlayerStatuses[Global.clientID].Team).ToString());
        Global.menuManager.ClearMenus();
        Global.menuManager.loadingScreen.Show();
        await ToSignal(GetTree().CreateTimer(0.1f), SceneTreeTimer.SignalName.Timeout);


        MapGenerator mapGenerator = new MapGenerator();

        mapGenerator.mapSize = (MapGenerator.MapSize)GetNode<OptionButton>("newgameoptions/worldgensize").Selected;




        mapGenerator.numberOfPlayers = (int)(PlayerStatuses.Count+ GetNode<HSlider>("newgameoptions/ainumber").Value);
        mapGenerator.numberOfHumanPlayers = PlayerStatuses.Count;
        mapGenerator.generateRivers = false;
        mapGenerator.resourceAmount = MapGenerator.ResourceAmount.Medium;
        mapGenerator.mapType = (MapGenerator.MapType)GetNode<OptionButton>("newgameoptions/worldgentype").Selected;

        string mapData = mapGenerator.GenerateMap();
        //Global.debugLog("Map generated: " + mapData);

        LobbyMessage lobbyMessage = new LobbyMessage();
        lobbyMessage.Sender = Global.clientID;
        lobbyMessage.MessageType = "startgame";

        MapData mapDataMessage = new();
        mapDataMessage.MapData_ = mapData;
        mapDataMessage.MapHeight = (uint)(mapGenerator.bottom + 1);
        mapDataMessage.MapWidth = (uint)(mapGenerator.right + 1);

        lobbyMessage.MapData = mapDataMessage;

        Global.networkPeer.LobbyMessageAllPeers(lobbyMessage);
        
    }



}

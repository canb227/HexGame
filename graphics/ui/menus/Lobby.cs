using Godot;
using System;
using Steamworks;
using System.Collections.Generic;
using NetworkMessages;
using System.Runtime.InteropServices.JavaScript;
using System.Linq;

public partial class Lobby : Control
{

    public List<Color> PlayerColors = new()
    {
        {Colors.Red},
        {Colors.Green},
        {Colors.Blue},
        {Colors.Yellow},
        {Colors.Cyan},
        {Colors.Magenta},
        {Colors.Orange},
        {Colors.Purple},
        {Colors.Brown},
        {Colors.Gray},
        {Colors.Pink},
        {Colors.Lime},
        {Colors.Teal},
        {Colors.Violet},
        {Colors.Gold},
        {Colors.Silver},
        {Colors.Beige},
        {Colors.Maroon},
        {Colors.Olive},
        {Colors.Turquoise},
        {Colors.Salmon},
        {Colors.Lavender},
    };

    VBoxContainer PlayersListBox;
    public Dictionary<ulong, LobbyStatus> PlayerStatuses = new();
    Button StartGameButton;
    bool singleplayer = false;
    bool isHost = false;
    int teamCounter = 0;
    List<int> teamNums = new List<int>();
    List<int> teamColors = new();
    private int MAXTEAMS = 99;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        NetworkPeer.PlayerJoinedEvent += OnPlayerJoinEvent;
        NetworkPeer.LobbyMessageReceivedEvent += OnLobbyMessageReceived;
        NetworkPeer.ChatMessageReceivedEvent += NetworkPeer_ChatMessageReceivedEvent;
        GetNode<Button>("addAI").Pressed += onAddAIButtonPressed;
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
        Global.networkPeer.ChatAllPeersAndSelf(message);
        chatInput.Text = "";
    }

    private void onAddAIButtonPressed()
    {
        if (!isHost && !singleplayer)
        {
            Global.Log("Only host can add AI players to the lobby.");
            return;
        }
        Global.Log("Adding AI player to lobby.");
        Random rng =new Random();
        ulong aiID = (ulong)rng.NextInt64();
        
        LobbyMessage lobbyMessage = new LobbyMessage();
        lobbyMessage.Sender = Global.clientID;
        lobbyMessage.MessageType = "addAI";
        LobbyStatus aiStatus = new LobbyStatus()
        {
            Id = aiID,
            IsHost = isHost,
            IsReady = true,
            Faction = 0,
            Team = GetNextTeamNum(),
            ColorIndex = GetNextTeamColor(),
            IsAI = true
        };
        lobbyMessage.LobbyStatus = aiStatus;
        Global.networkPeer.LobbyMessageAllPeersAndSelf(lobbyMessage);
        
    }

    private int GetNextTeamColor()
    {
        if (teamColors.Count == 0)
        {
            Global.Log("No more team colors available, returning default color index 0.");
            return 0; // Return default color index if no colors are available
        }
        int ret = teamColors[0];
        teamColors.RemoveAt(0);
        return ret;
    }

    public void CreateLobby()
    {
        for (int i = 1; i < MAXTEAMS; i++)
        {
            teamNums.Add(i);
        }
        for (int i = 0; i < PlayerColors.Count; i++)
        {
            teamColors.Add(i);
        }
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

        AddNewPlayerToLobby(Global.clientID, GetNextTeamNum(), GetNextTeamColor(), true, false);
    }

    private int GetNextTeamNum()
    {
        int ret = teamNums[0];
        teamNums.RemoveAt(0);
        return ret;
    }

    private void AddNewPlayerToLobby(ulong id, int teamNum, int teamColorIndex, bool self, bool ai)
    {
        Control PlayerListItem = GD.Load<PackedScene>("res://graphics/ui/menus/playerListItem.tscn").Instantiate<Control>();
        bool isReady = false;
        if (!ai)
        {
            PlayerListItem.GetNode<Label>("playername").Text = SteamFriends.GetFriendPersonaName(new CSteamID(id));
            PlayerListItem.GetNode<TextureRect>("icon").Texture = Global.GetMediumSteamAvatar(id);
        }
        else
        {
            PlayerListItem.GetNode<TextureRect>("icon").Texture = new GradientTexture2D()
            {
                Width = 64,
                Height = 64,
                Gradient = new Gradient()
                {
                    Colors = [Colors.Gray]
                }
            };
            PlayerListItem.GetNode<Label>("playername").Text = "Bot: " + id.ToString().Substring(0,5);
            PlayerListItem.GetNode<CheckButton>("ReadyButton").ButtonPressed = true;
            isReady = true;
        }
        foreach (Color color in PlayerColors)
        {
            GradientTexture2D icon = new GradientTexture2D();
            icon.Width = 24;
            icon.Height = 24;
            Gradient gradient = new Gradient();
            gradient.Colors = [color];
            icon.Gradient = gradient;
            PlayerListItem.GetNode<OptionButton>("colorselect").AddIconItem(icon,"");
        }
        foreach (var faction in Enum.GetNames(typeof(FactionType)))
        {
            PlayerListItem.GetNode<OptionButton>("factionselect").AddItem(faction);
        }
        if (self || (ai && isHost))
        {
            PlayerListItem.GetNode<CheckButton>("ReadyButton").Toggled += (index) => onReadyChanged(index, id);
            PlayerListItem.GetNode<OptionButton>("factionselect").ItemSelected += (index) => OnFactionChange(index, id);
            PlayerListItem.GetNode<OptionButton>("teamselect").ItemSelected += (index) => OnTeamChange(index, id);
            PlayerListItem.GetNode<OptionButton>("colorselect").ItemSelected += (index) => OnColorChange(index,id);
            PlayerListItem.GetNode<Button>("kick").Pressed += () => OnKickButtonPressed(id);
        }
        else
        {
            
            PlayerListItem.GetNode<CheckButton>("ReadyButton").Disabled = true;
            PlayerListItem.GetNode<OptionButton>("factionselect").Disabled = true;
            PlayerListItem.GetNode<OptionButton>("teamselect").Disabled = true;
            PlayerListItem.GetNode<OptionButton>("colorselect").Disabled = true;
            PlayerListItem.GetNode<Button>("kick").Disabled = true;
        }
        PlayerListItem.GetNode<OptionButton>("teamselect").Disabled = true;
        PlayerListItem.GetNode<OptionButton>("colorselect").Selected = teamColorIndex; // Color index is 0-indexed in the code, but 1-indexed in the UI
        PlayerListItem.GetNode<OptionButton>("teamselect").Selected = teamNum-1; // Teams are 1-indexed in the UI, but 0-indexed in the code
        PlayerListItem.Name = id.ToString();
        PlayersListBox.AddChild(PlayerListItem);

        PlayerStatuses.Add(id, new LobbyStatus() { Id=id, IsHost = isHost, IsReady = isReady, Faction = 0, Team = teamNum, ColorIndex=teamColorIndex, IsAI=ai });
    }

    private void OnKickButtonPressed(ulong id)
    {
        if (isHost)
        {
            LobbyMessage lobbyMessage = new LobbyMessage();
            lobbyMessage.Sender = Global.clientID;
            lobbyMessage.MessageType = "kick";
            lobbyMessage.LobbyStatus = PlayerStatuses[id];
            teamNums.Add(PlayerStatuses[id].Team);
            teamNums.Sort();
            teamColors.Add(PlayerStatuses[id].ColorIndex);
            teamColors.Sort();
            Global.networkPeer.LobbyMessageAllPeersAndSelf(lobbyMessage);

        }
        else
        {

        }
        
    }

    private void OnColorChange(long index, ulong id)
    {
        Global.Log("color change button pressed, index: " + index);
        LobbyStatus status = PlayerStatuses[id];
        status.ColorIndex = (int)index;
        PlayerStatuses[id] = status;
        UpdateLobbyPeers(id);
    }

    public void JoinLobby(ulong hostID)
    {
        Global.networkPeer.JoinToPeer(hostID);
        Global.Log("Joining lobby: " + hostID);
        SteamFriends.SetRichPresence("status", "In a lobby");
        SteamFriends.SetRichPresence("connect", Global.clientID.ToString());
        //AddNewPlayerToLobby(Global.clientID, 0, true, false);
    }

    private void OnLobbyMessageReceived(LobbyMessage lobbyMessage)
    {

        Global.Log("Lobby message received: " + lobbyMessage.MessageType + " from " + lobbyMessage.Sender);
        if (lobbyMessage.Sender == Global.clientID && lobbyMessage.MessageType == "status")
        {
            Global.Log("Ignoring own lobby message: " + lobbyMessage.MessageType);
            //return; // Ignore own messages except for startgame
        }
        switch (lobbyMessage.MessageType)
        {
            case "status":
                PlayerStatuses[lobbyMessage.LobbyStatus.Id] = lobbyMessage.LobbyStatus;
                Control PlayerListItem = PlayersListBox.GetNode<Control>(lobbyMessage.Sender.ToString());
                PlayerListItem.GetNode<OptionButton>("teamselect").Selected = lobbyMessage.LobbyStatus.Team - 1;
                PlayerListItem.GetNode<OptionButton>("factionselect").Selected = lobbyMessage.LobbyStatus.Faction;
                PlayerListItem.GetNode<CheckButton>("ReadyButton").ButtonPressed = lobbyMessage.LobbyStatus.IsReady;
                PlayerListItem.GetNode<OptionButton>("colorselect").Selected = lobbyMessage.LobbyStatus.ColorIndex;
                CheckIfGameCanStart();
                break;
            case "leave":
                Global.Log("Player left the lobby: " + lobbyMessage.LobbyStatus.Id);
                if (PlayerStatuses.ContainsKey(lobbyMessage.LobbyStatus.Id))
                {
                    if (isHost)
                    {
                        teamNums.Add(PlayerStatuses[lobbyMessage.LobbyStatus.Id].Team);
                        teamNums.Sort();
                        teamColors.Add(PlayerStatuses[lobbyMessage.LobbyStatus.Id].ColorIndex);
                        teamColors.Sort();
                    }
                    PlayerStatuses.Remove(lobbyMessage.LobbyStatus.Id);
                    Control playerItem = PlayersListBox.GetNode<Control>(lobbyMessage.LobbyStatus.Id.ToString());
                    if (playerItem != null)
                    {
                        playerItem.QueueFree();
                    }

                }
                break;
            case "startgame":
                Global.Log("Starting game from lobby message");
                StartGame(lobbyMessage);
                break;
            case "loadgame":
                Global.Log("Loading game from host");
                Global.gameManager.game = Global.gameManager.LoadGameRaw(lobbyMessage.MapData.MapData_);
                GetNode<Label>("NewGameStatus").Text = "GAME LOADED";
                GetNode<ColorRect>("newgamehide").Visible = true;
                break;
            case "kick":
                Global.Log("Host Kicked player from lobby: " + lobbyMessage.LobbyStatus.Id);
                if (lobbyMessage.LobbyStatus.Id==Global.clientID)
                {
                    Global.Log("You have been kicked from the lobby. Returning to main menu.");
                    LeaveLobby();
                    
                    

                    return;
                }
                else if (PlayerStatuses.ContainsKey(lobbyMessage.LobbyStatus.Id))
                {
                    PlayerStatuses.Remove(lobbyMessage.LobbyStatus.Id);
                    Control playerItem = PlayersListBox.GetNode<Control>(lobbyMessage.LobbyStatus.Id.ToString());
                    if (playerItem != null)
                    {
                        playerItem.QueueFree();
                    }
                }
                break;
            case "addAI":
                Global.Log("Adding AI player with ID: " + lobbyMessage.LobbyStatus.Id + "to lobby from message: " + lobbyMessage.Sender);
                AddNewPlayerToLobby(lobbyMessage.LobbyStatus.Id, lobbyMessage.LobbyStatus.Team, lobbyMessage.LobbyStatus.ColorIndex, false, true);
                break;
            case "addPlayer":
                Global.Log("Adding player with ID: " + lobbyMessage.LobbyStatus.Id + " to lobby from message: " + lobbyMessage.Sender);
                AddNewPlayerToLobby(lobbyMessage.LobbyStatus.Id, lobbyMessage.LobbyStatus.Team, lobbyMessage.LobbyStatus.ColorIndex, false, false);
                break;
            default:
                Global.Log("Unknown lobby message type: " + lobbyMessage.MessageType);
                break;
        }

    }

    private void LeaveLobby()
    {
        LobbyMessage lobbyMessage = new LobbyMessage();
        lobbyMessage.Sender = Global.clientID;
        lobbyMessage.MessageType = "leave";
        lobbyMessage.LobbyStatus = PlayerStatuses[Global.clientID];
        Global.networkPeer.LobbyMessageAllPeersAndSelf(lobbyMessage);
        Global.networkPeer.DisconnectFromAllPeers();
        Global.menuManager.ChangeMenu(MenuManager.UI_Mainmenu);
    }

    private void StartGame(LobbyMessage lobbyMessage)
    {
        //Global.debugLog(lobbyMessage.SavePayload);
        Global.menuManager.ChangeMenu(MenuManager.UI_LoadingScreen);
        
        /*if (GetNode<CheckButton>("newgameoptions/debugmode").ButtonPressed)
        {
            Global.gameManager.game = new Game(0);
            Global.gameManager.game.mainGameBoard.InitGameBoardFromData(lobbyMessage.MapData.MapData_, (int)(lobbyMessage.MapData.MapWidth - 1), (int)(lobbyMessage.MapData.MapHeight - 1));
            Global.gameManager.game.AddPlayer(10, 0, 0, Godot.Colors.Black, false);
            foreach (ulong playerID in PlayerStatuses.Keys)
            {
                if (playerID!=Global.clientID)
                {
                    Global.Log("Adding player to game with ID: " + playerID + " and teamNum: " + PlayerStatuses[playerID].Team + " and color: " + PlayerColors[(int)PlayerStatuses[playerID].ColorIndex].ToString());
                    Global.gameManager.game.AddPlayer(10, (int)PlayerStatuses[playerID].Team, playerID, PlayerColors[(int)PlayerStatuses[playerID].ColorIndex], PlayerStatuses[playerID].IsAI);
                }
            }
            Global.gameManager.isHost = isHost;
            Global.gameManager.startGame(0);
        }
        else
        {

        }*/


        Global.gameManager.game = new Game((int)PlayerStatuses[Global.clientID].Team);
        Global.gameManager.game.mainGameBoard.InitGameBoardFromData(lobbyMessage.MapData.MapData_, (int)(lobbyMessage.MapData.MapWidth - 1), (int)(lobbyMessage.MapData.MapHeight - 1));
        //Global.gameManager.game.AddPlayer(10, 0, 0, Godot.Colors.Black,true); 
        foreach (ulong playerID in PlayerStatuses.Keys)
        {
            Global.Log("Adding player to game with ID: " + playerID + " and teamNum: " + PlayerStatuses[playerID].Team + " and color: " + PlayerColors[(int)PlayerStatuses[playerID].ColorIndex].ToString());
            Global.gameManager.game.AddPlayer(10, (int)PlayerStatuses[playerID].Team, playerID, PlayerColors[(int)PlayerStatuses[playerID].ColorIndex], PlayerStatuses[playerID].IsAI);
        }
        Global.gameManager.isHost = isHost;

        Global.gameManager.startGame((int)PlayerStatuses[Global.clientID].Team);
        Global.menuManager.ClearMenus();
    }

    private void onReadyChanged(bool toggledOn, ulong id)
    {
        LobbyStatus status = PlayerStatuses[id];
        status.IsReady = toggledOn;
        PlayerStatuses[id] = status;
        UpdateLobbyPeers(id);
    }
    private void OnTeamChange(long index, ulong id)
    {
        Global.Log("team change button pressed, index: " + index);
        LobbyStatus status = PlayerStatuses[id];
        status.Team = (int)(index + 1);
        PlayerStatuses[id] = status;
        UpdateLobbyPeers(id);
    }

    private void OnFactionChange(long index, ulong id)
    {
        LobbyStatus status = PlayerStatuses[id];
        status.Team = (int)index;
        PlayerStatuses[id] = status;
        UpdateLobbyPeers(id);
    }

    private void UpdateLobbyPeers(ulong id)
    {
        LobbyMessage lobbyMessage = new LobbyMessage();
        lobbyMessage.Sender = Global.clientID;
        lobbyMessage.MessageType = "status";
        lobbyMessage.LobbyStatus = PlayerStatuses[id];
        Global.networkPeer.LobbyMessageAllPeersAndSelf(lobbyMessage);

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
        Global.networkPeer.LobbyMessageAllPeersAndSelf(lobbyMessage);


    }

    private void OnPlayerJoinEvent(ulong playerID)
    {
        Global.Log("Player joined to Lobby: " + playerID);
        if (isHost)
        { 
            Global.Log("Adding player to lobby: " + playerID);
            LobbyMessage lobbyMessage = new LobbyMessage();
            lobbyMessage.Sender = Global.clientID;
            lobbyMessage.MessageType = "addPlayer";
            LobbyStatus lobbyStatus = new LobbyStatus()
            {
                Id = playerID,
                IsHost = false,
                IsReady = false,
                Faction = 0,
                Team = GetNextTeamNum(),
                ColorIndex = GetNextTeamColor(),
                IsAI = false
            };
            lobbyMessage.LobbyStatus = lobbyStatus;
            Global.networkPeer.LobbyMessageAllPeersAndSelf(lobbyMessage);
        }
        else
        {
            
        }

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
        Global.menuManager.ChangeMenu(MenuManager.UI_LoadingScreen);
        await ToSignal(GetTree().CreateTimer(0.1f), SceneTreeTimer.SignalName.Timeout);


        MapGenerator mapGenerator = new MapGenerator();

        mapGenerator.mapSize = (MapGenerator.MapSize)GetNode<OptionButton>("newgameoptions/worldgensize").Selected;




        mapGenerator.numberOfPlayers = (int)(PlayerStatuses.Count);
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

        Global.networkPeer.LobbyMessageAllPeersAndSelf(lobbyMessage);
        
    }



}

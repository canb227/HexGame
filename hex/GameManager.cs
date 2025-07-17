using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System.Security.AccessControl;
using NetworkMessages;


[GlobalClass]
public partial class GameManager : Node
{
    private const bool DEBUGNETWORK = true;

    public bool isHost = false;
    public static GameManager instance;
    public GraphicManager graphicManager;
    public AIManager AIManager;
    public Game game;
    public AudioManager audioManager = new();
    public bool gameStarted = false;

    public Dictionary<int,ulong> teamNumToPlayerID = new Dictionary<int, ulong>();

    public GameManager()
    {
        instance = this;
        Global.gameManager = this;
        game = new Game();
        AddChild(audioManager);
        //startTerrainDemo();
    }


    public void SaveGame(String filePath)
    {
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new HexJsonConverter() }
        };
        string json = JsonSerializer.Serialize(game, options);
        Godot.FileAccess fileAccess = Godot.FileAccess.Open(filePath, Godot.FileAccess.ModeFlags.Write);
        fileAccess.StorePascalString(json);
        fileAccess.Close();
        GD.Print("Game data saved to file: " + filePath);
    }

    public string SaveGameRaw()
    {
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new HexJsonConverter() }
        };
        string json = JsonSerializer.Serialize(game, options);
        return json;
    }

    public string SaveGameRaw(Game game)
    {
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new HexJsonConverter() }
        };
        string json = JsonSerializer.Serialize(game, options);
        return json;
    }
    public Game LoadGame(String filePath)
    {
        Global.Log("Loading Game from file: " + filePath);
        Godot.FileAccess fileAccess = Godot.FileAccess.Open(filePath, Godot.FileAccess.ModeFlags.Read);
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new HexJsonConverter() }
        };
        Game retVal = JsonSerializer.Deserialize<Game>(fileAccess.GetPascalString(), options);
        fileAccess.Close();
        return retVal;
    }

    public Game LoadGameRaw(string rawSave)
    {
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new HexJsonConverter() }
        };
        Game retVal = JsonSerializer.Deserialize<Game>(rawSave, options);
        return retVal;
    }




    public void startGame(int teamNum)
    {

        Global.Log("Starting Game as team: " + teamNum);
        Layout pointyReal = new Layout(Layout.pointy, new Point(10, 10), new Point(0, 0));
        Layout pointy = new Layout(Layout.pointy, new Point(-10, 10), new Point(0, 0));
        Global.layout = pointy;
        game.localPlayerTeamNum = teamNum;


        Global.lobby.lobbyPeerStatuses[Global.clientID].IsLoaded = true;
        LobbyMessage lobbyMessage = new LobbyMessage();
        lobbyMessage.Sender = Global.clientID;
        lobbyMessage.LobbyStatus = Global.lobby.lobbyPeerStatuses[Global.clientID];
        lobbyMessage.MessageType = "loaded";
        Global.networkPeer.LobbyMessageAllPeersAndSelf(lobbyMessage);
        if (isHost)
        {
            Global.Log($"Done loading. I'm the host so its time to pick Spawn Locations and communicate them.");
        }
        else
        {
            Global.Log($"Done loading. Notifying peers and waiting to get Founder spawn from Host");
        }


        //MoveCameraToStartLocation();
    }

    public void HostInitGame()
    {
        if (isHost)
        {
            SpawnPlayers();
            this.AIManager = new AIManager();
            AIManager.InitAI();
            SpawnPlayers();
        }
    }

    public void SpawnPlayers()
    {
        Global.Log("Spawning Players. Total: " + game.playerDictionary.Count);
        foreach (Player player in game.playerDictionary.Values)
        {
            if (player.teamNum == -99) //TODO: IMPLEMENT NON MAJOR PLAYER SPAWNING
            {

            }
            else
            {
                Global.Log($"Attempting to find good spawn location for team:{player.teamNum}");
                Hex spawnLocation = GetPlayerSpawnHex(player);
                Global.Log($"Found:{spawnLocation}, spawning a Founder unit for that team.");
                Global.gameManager.SpawnUnit("Founder", player.teamNum, spawnLocation, false, false);
            }
        }
    }

    public List<Hex> FindRecommendedSettleLocations()
    {
        Global.Log($"Searching for good spawn locations.");
        List<Hex> retval = new List<Hex>();
        Dictionary<Hex, GameHex> map = Global.gameManager.game.mainGameBoard.gameHexDict;
        foreach (Hex hex in map.Keys)
        {
            if (isGoodSettleLocation(hex))
            {
                retval.Add(hex);
            }
        }
        Global.Log($"Found {retval.Count} good spots.");
        return retval;
    }

    private bool isGoodSettleLocation(Hex hex)
    {
        bool hasFreshWater = true;
        int resourcesInRange = 0;
        int distanceToOthers = Global.gameManager.game.mainGameBoard.gameHexDict[hex].rangeToNearestCity;
        int normalTilesInRange = 0;
        List<Hex> toCheck = hex.WrappingRange(4,Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.top, Global.gameManager.game.mainGameBoard.bottom);
        foreach (Hex h in toCheck)
        {
            GameHex gHex = Global.gameManager.game.mainGameBoard.gameHexDict[h];
            if (gHex.resourceType!=ResourceType.None)
            {
                resourcesInRange++;
            }
            if (gHex.terrainType!=TerrainType.Ocean && gHex.terrainType!=TerrainType.Mountain && gHex.terrainType!=TerrainType.Coast)
            {
                normalTilesInRange++;
            }
        }
        if (resourcesInRange >= 2 && distanceToOthers<5 && normalTilesInRange>10)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private Hex GetPlayerSpawnHex(Player player)
    {
        return PickRandomValidHex();
        /*
        Random rng = new Random();
        List<Hex> list = FindRecommendedSettleLocations();
        return list[rng.Next(list.Count)];*/
    }

    public Hex PickRandomValidHex()
    {
        List<Hex> list = new List<Hex>();
        foreach (Hex hex in game.mainGameBoard.gameHexDict.Keys)
        {
            if (game.mainGameBoard.gameHexDict[hex].terrainType == TerrainType.Flat && game.mainGameBoard.gameHexDict[hex].units.Count == 0 && game.mainGameBoard.gameHexDict[hex].district == null && game.mainGameBoard.gameHexDict[hex].resourceType==ResourceType.None)
            {
                list.Add(hex);
            }
        }
        
        Random rng = new Random();
        return list[rng.Next(list.Count)];
    }

    public void startDebugGame(string savePath, int teamNum)
    {
        Global.Log("Starting Game");
        Layout pointyReal = new Layout(Layout.pointy, new Point(10, 10), new Point(0, 0));
        Layout pointy = new Layout(Layout.pointy, new Point(-10, 10), new Point(0, 0));
        Global.layout = pointy;
        game = LoadGame(savePath);
        game.localPlayerTeamNum = teamNum;
        InitGraphics(game, Global.layout);
        Global.menuManager.ClearMenus();
        gameStarted = true;

    }

    private void startTerrainDemo()
    {
        Global.Log("Starting Game");
        Layout pointyReal = new Layout(Layout.pointy, new Point(10, 10), new Point(0, 0));
        Layout pointy = new Layout(Layout.pointy, new Point(-10, 10), new Point(0, 0));
        Global.layout = pointy;

        if (game.mainGameBoard == null)
        {
/*            game = GameTests.TestSlingerCombat();
            SaveGame("test.txt");
            game = LoadGame("test.txt");*/
        }

        InitGraphics(game, Global.layout);
        Global.menuManager.ClearMenus();
        gameStarted = true;
        Global.Log("NEWTURN: " + game.turnManager.currentTurn + "///////////////////////////////////////////NEWTURN" + game.turnManager.currentTurn + "///////////////////////////////////////////////////////NEWTURN" + game.turnManager.currentTurn + "///////////////////////////////////////////NEWTURN" + game.turnManager.currentTurn + "");

    }

    private void InitGraphics(Game game, Layout layout)
    {
        Global.Log("Initializing Graphics");
        graphicManager = new GraphicManager(layout);
        if (Global.gameManager.game.mainGameBoard != null)
        {
            graphicManager.NewGameBoard(Global.gameManager.game.mainGameBoard);
        }
        graphicManager.Name = "GraphicManager";
        AddChild(graphicManager);
    }

    public bool TryGetGraphicManager(out GraphicManager manager)
    {
        if (graphicManager != null)
        {
            manager = graphicManager;
            return true;
        }
        manager = null;
        return false;
    }


    public override void _PhysicsProcess(double delta)
    {
        if (game== null || game.turnManager == null)
        {
            return;
        }
        if (game.teamManager.relationships.ContainsKey(0) && game.localPlayerTeamNum!=0)
        {
            //game.turnManager.EndCurrentTurn(0);
        }

        if (game.teamManager.relationships.ContainsKey(2))
        {
            //game.turnManager.EndCurrentTurn(2);
        }

        List<int> waitingForPlayerList = game.turnManager.CheckTurnStatus();
        if (graphicManager!=null&& !waitingForPlayerList.Any())
        {
            game.turnManager.StartNewTurn();
            graphicManager.StartNewTurn();
        }
        else
        {
            //push waitingForPlayerList to UI
        }
    }

    

    public void MoveUnit(int unitID, Hex hex, bool isEnemy, bool local = true)
    {
        
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructMoveUnitCommand(unitID, hex, isEnemy));
            return;
        }
        else
        {
            Global.Log("Network (or loopback) move command recevied, executing.");
        }


        Unit unit = SearchUnitByID(unitID);
        if (unit == null)
        {
            Global.Log("Unit is null"); //TODO - Potential Desync
            return;
        }

        GameHex target = game.mainGameBoard.gameHexDict[hex];
        if (target == null)
        {
            Global.Log("Target hex is null");//TODO - Potential Desync
            return;
        }

        if (isEnemy != Global.gameManager.game.mainGameBoard.gameHexDict[hex].IsEnemyPresent(unit.teamNum))
        {
            Global.Log("DESYNC ALARM");
        }
        try
        {
            unit.MoveTowards(target, Global.gameManager.game.teamManager, isEnemy);
        }
        catch (Exception e)
        {
            Global.Log("Error moving unit: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    private Unit SearchUnitByID(int unitID)
    {
        if (Global.gameManager.game.unitDictionary.TryGetValue(unitID, out Unit unit))
        {
            return unit;
        }
        else
        {
            return null;
        }
    }

    public void ActivateAbility(int unitID, string AbilityName, Hex Target, bool local = true) 
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructActivateAbilityCommand(unitID, AbilityName, Target));
            return;
        }

        Unit unit = SearchUnitByID(unitID);
        if (unit == null)
        {
            Global.Log("Unit is null"); //TODO - Potential Desync
            return;
        }

        GameHex target = game.mainGameBoard.gameHexDict[Target];
        if (target == null)
        {
            Global.Log("Target hex is null"); //TODO - Potential Desync
            return;
        }

        UnitAbility ability = unit.abilities.Find(x => x.name == AbilityName);
        if (ability == null)
        {
            Global.Log("Ability is null"); //TODO - Potential Desync
            return;
        }

        try
        {
            ability.ActivateAbility(target);
        }
        catch (Exception e)
        {
            Global.Log("Error activating ability: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    public void AddToProductionQueue(int cityID, string name, Hex targetHex, bool front = false, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructAddToProductionQueueCommand(cityID,name,targetHex,front));
            return;
        }

        City city = Global.gameManager.game.cityDictionary[cityID];
        if (city == null)
        {
            Global.Log("City is null"); //TODO - Potential Desync
            return;
        }

        if (front)
        {
            try
            {
                city.AddToFrontOfQueue(name, targetHex);
            }
            catch (Exception e)
            {
                Global.Log("Error changing production queue: " + e.Message); //TODO - Potential Desync
                throw;
            }
        }
        else
        {
            try
            {
                city.AddToQueue(name, targetHex);
            }
            catch (Exception e)
            {
                Global.Log("Error changing production queue: " + e.Message); //TODO - Potential Desync
                throw;
            }
        }

    }

    public void RemoveFromProductionQueue(int cityID, int index, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructRemoveFromProductionQueueCommand(cityID, index));
            return;
        }

        City city = Global.gameManager.game.cityDictionary[cityID];
        if (city == null)
        {
            Global.Log("City is null"); //TODO - Potential Desync
            return;
        }

        try
        {
            city.RemoveFromQueue(index);
        }
        catch (Exception e)
        {
            Global.Log("Error changing production queue: " + e.Message); //TODO - Potential Desync
            throw;
        }

    }

    public void MoveToFrontOfProductionQueue(int cityID, int index, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructMoveToFrontOfProductionQueueCommand(cityID, index));
            return;
        }

        City city = Global.gameManager.game.cityDictionary[cityID];
        if (city == null)
        {
            Global.Log("City is null"); //TODO - Potential Desync
            return;
        }

        try
        {
            city.MoveToFrontOfProductionQueue(index);
        }
        catch (Exception e)
        {
            Global.Log("Error moving to front of prod queue: " + e.Message); //TODO - Potential Desync
            throw;
        }

    }


    public void ExpandToHex(int cityID, Hex Target, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructExpandToHexCommand(cityID, Target));
            return;
        }

        City city = Global.gameManager.game.cityDictionary[cityID];
        if (city == null)
        {
            Global.Log("City is null"); //TODO - Potential Desync
            return;
        }

        try
        {
            city.ExpandToHex(Target);
        }
        catch (Exception e)
        {
            Global.Log("Error expanding to hex: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    public void DevelopDistrict(int cityID, Hex Target, DistrictType districtType, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructDevelopDistrictCommand(cityID, Target, districtType));
            return;
        }

        City city = Global.gameManager.game.cityDictionary[cityID];
        if (city == null)
        {
            Global.Log("City is null"); //TODO - Potential Desync
            return;
        }

        try
        {
            city.DevelopDistrict(Target, districtType);
        }
        catch (Exception e)
        {
            Global.Log("Error developing district: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    public void RenameCity(int cityID, string name, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructRenameCityCommand(cityID, name));
            return;
        }

        City city = Global.gameManager.game.cityDictionary[cityID];
        if (city == null)
        {
            Global.Log("City is null"); //TODO - Potential Desync
            return;
        }

        try
        {
            city.RenameCity(name);
        }
        catch (Exception e)
        {
            Global.Log("Error renaming city: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    public void SelectResearch(int teamNum, string techName, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructSelectResearchCommand(teamNum, techName));
            return;
        }

        try
        {
            Global.gameManager.game.playerDictionary[teamNum].SelectResearch(techName);
        }
        catch (Exception e)
        {
            Global.Log("Error selecting research tech: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    public void SelectCulture(int teamNum, string cultureName, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructSelectCultureCommand(teamNum, cultureName));
            return;
        }

        try
        {
            Global.gameManager.game.playerDictionary[teamNum].SelectCultureResearch(cultureName);
        }
        catch (Exception e)
        {
            Global.Log("Error selecting culture tech: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    public void AddResourceAssignment(int cityID, ResourceType resourceType, Hex sourceHex, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructAddResourceAssignmentCommand(cityID, resourceType, sourceHex));
            return;
        }

        City city = Global.gameManager.game.cityDictionary[cityID];
        if (city == null)
        {
            Global.Log("City is null"); //TODO - Potential Desync
            return;
        }

        try
        {
            Global.gameManager.game.playerDictionary[city.teamNum].AddResource(sourceHex, resourceType, city);
        }
        catch (Exception e)
        {
            Global.Log("Error adding resource assignment: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    public void RemoveResourceAssignment(int teamNum, Hex sourceHex, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructRemoveResourceAssignmentCommand(teamNum, sourceHex));
            return;
        }

        try
        {
            Global.gameManager.game.playerDictionary[teamNum].RemoveResource(sourceHex);
        }
        catch (Exception e)
        {
            Global.Log("Error adding resource assignment: " + e.Message); //TODO - Potential Desync
            throw;
        }

    }
    
    public void EndTurn(int teamNum, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructEndTurnCommand(teamNum));
            return;
        }

        try
        {
            Global.gameManager.game.turnManager.EndCurrentTurn(teamNum);
        }
        catch (Exception e)
        {
            Global.Log("Error ending turn: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    internal void ExecutePendingDeal(int dealID, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructExecutePendingDealCommand(dealID));
            return;
        }

        try
        {
            Global.gameManager.game.teamManager.ExecuteDeal(dealID);
        }
        catch (Exception e)
        {
            Global.Log("Error executing pending deal: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    internal void RemovePendingDeal(int dealID, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructRemovePendingDealCommand(dealID));
            return;
        }

        try
        {
            Global.gameManager.game.teamManager.RemoveDeal(dealID);
        }
        catch (Exception e)
        {
            Global.Log("Error removing pending deal: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    internal void AddPendingDeal(int dealID, int fromTeamNum, int toTeamNum, List<DiplomacyAction> requests, List<DiplomacyAction> offers, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructAddPendingDealCommand(dealID,fromTeamNum,toTeamNum,requests,offers));
            return;
        }

        try
        {
            Global.gameManager.game.teamManager.AddPendingDeal(new DiplomacyDeal(dealID,fromTeamNum, toTeamNum, offers, requests));
        }
        catch (Exception e)
        {
            Global.Log("Error adding pending deal: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    internal void NewExportRoute(int fromCityID, int toCityID, YieldType yieldType, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructNewExportRouteCommand(fromCityID, toCityID, yieldType));
            return;
        }

        
        try
        {
            int teamNum = Global.gameManager.game.cityDictionary[fromCityID].teamNum;
            Global.gameManager.game.playerDictionary[teamNum].NewExportRoute(fromCityID, toCityID, yieldType);
        }
        catch (Exception e)
        {
            Global.Log("Error adding export route: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    internal void RemoveExportRoute(int fromCityID, int toCityID, YieldType yieldType, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructRemoveExportRouteCommand(fromCityID, toCityID, yieldType));
            return;
        }


        try
        {
            int teamNum = Global.gameManager.game.cityDictionary[fromCityID].teamNum;
            Global.gameManager.game.playerDictionary[teamNum].RemoveExportRoute(fromCityID, toCityID, yieldType);
        }
        catch (Exception e)
        {
            Global.Log("Error remove export route: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    internal void NewTradeRoute(int fromCityID, int toCityID, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructNewTradeRouteCommand(fromCityID, toCityID));
            return;
        }


        try
        {
            int teamNum = Global.gameManager.game.cityDictionary[fromCityID].teamNum;
            Global.gameManager.game.playerDictionary[teamNum].NewTradeRoute(fromCityID, toCityID);
        }
        catch (Exception e)
        {
            Global.Log("Error adding trade route: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    internal void RemoveTradeRoute(int fromCityID, int toCityID, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructRemoveTradeRouteCommand(fromCityID, toCityID));
            return;
        }


        try
        {
            int teamNum = Global.gameManager.game.cityDictionary[fromCityID].teamNum;
            Global.gameManager.game.playerDictionary[teamNum].RemoveTradeRoute(fromCityID, toCityID);
        }
        catch (Exception e)
        {
            Global.Log("Error removing trade route: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    internal void AssignPolicyCard(int teamNum, int policyCardID, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructAssignPolicyCardCommand(teamNum, policyCardID));
            return;
        }


        try
        {
            Global.gameManager.game.playerDictionary[teamNum].AssignPolicyCard(policyCardID);
        }
        catch (Exception e)
        {
            Global.Log("Error assigning policy card: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    internal void UnassignPolicyCard(int teamNum, int policyCardID, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructUnassignPolicyCardCommand(teamNum, policyCardID));
            return;
        }


        try
        {
            Global.gameManager.game.playerDictionary[teamNum].UnassignPolicyCard(policyCardID);
        }
        catch (Exception e)
        {
            Global.Log("Error unassigning policy card: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    internal void SetGovernment(int teamNum, GovernmentType govType, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructSetGovernmentCommand(teamNum, govType));
            return;
        }


        try
        {
            Global.gameManager.game.playerDictionary[teamNum].SetGovernment(govType);
        }
        catch (Exception e)
        {
            Global.Log("Error setting government: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    internal void SpawnUnit(string unitType, int teamNum, Hex position, bool stackable, bool flexible, bool local = true)
    {
        if (local)
        {
            
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructSpawnUnitCommand(unitType, teamNum, position, stackable, flexible));
            return;
        }

        try
        {
            Global.Log($"Got a command over network (or loopback) to spawn a unit of type {unitType} for team {teamNum} at position {position}.");
            Unit newUnit = new(unitType, 0, game.playerDictionary[teamNum].GetNextUniqueID(), teamNum);
            GameHex location = Global.gameManager.game.mainGameBoard.gameHexDict[position];
            if (location.SpawnUnit(newUnit,stackable,flexible)!=true)
            {
                Global.Log($"Error spawning unit "); //TODO - Potential Desync
            }

            if (unitType=="Founder" && teamNum==Global.gameManager.game.localPlayerTeamNum )
            {
                //Look I'm sorry the code to start the game for real is just here inside the spawn unit command - it saved me from writing another network message type.
                Global.Log("SpawnUnit command for my team's founder. Starting game and moving camera to here.");
                InitGraphics(game, Global.layout);
                Global.menuManager.ClearMenus();
                game.turnManager.StartNewTurn();
                graphicManager.StartNewTurn();
                gameStarted = true;
            }
        }
        catch (Exception e)
        {
            Global.Log("Error spawning unit: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    internal void SetDiplomaticState(int teamNumOne, int teamNumTwo, DiplomaticState diplomaticState, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructSetDiplomaticStateCommand(teamNumOne, teamNumTwo, diplomaticState));
            return;
        }


        try
        {
            Global.gameManager.game.teamManager.SetDiplomaticState(teamNumOne,teamNumTwo,diplomaticState);
        }
        catch (Exception e)
        {
            Global.Log("Error setting diplomatic state: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }
}

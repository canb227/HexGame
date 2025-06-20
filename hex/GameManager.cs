using Godot;
using NetworkMessages;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using Godot;
using System.Linq;
using Google.Protobuf.WellKnownTypes;

[GlobalClass]
public partial class GameManager : Node
{
    private const bool DEBUGNETWORK = true;

    public static GameManager instance;
    public GraphicManager graphicManager;
    public Game game;

    public Dictionary<int,ulong> teamNumToPlayerID = new Dictionary<int, ulong>();

    public GameManager()
    {
        instance = this;
        Global.gameManager = this;
        game = new Game();
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
        Global.menuManager.ClearMenus();
        Global.Log("Starting Game as team: " + teamNum);
        Layout pointyReal = new Layout(Layout.pointy, new Point(10, 10), new Point(0, 0));
        Layout pointy = new Layout(Layout.pointy, new Point(-10, 10), new Point(0, 0));
        Global.layout = pointy;

        if (game.mainGameBoard == null)
        {
            game = GameTests.TestSlingerCombat();
            SaveGame("test.txt");
            game = LoadGame("test.txt");
        }


        game.localPlayerTeamNum = teamNum;

        InitGraphics(game, Global.layout);

        SpawnPlayers();
        Global.menuManager.Hide();
        Global.menuManager.ClearMenus();
        //graphicManager.StartNewTurn();
    }

    public void SpawnPlayers()
    {
        Global.Log("Spawning Players. Total: " + game.playerDictionary.Count);
        for (int i = 0; i < game.playerDictionary.Count; i++)
        {
            //Global.debugLog("Spawning player: " + game.playerDictionary[i]);
            if (game.playerDictionary[i].teamNum == 0)
            {
                //Global.debugLog("Skipping player spawn for team 0");
            }
            else
            {
                Unit playerSettler = new Unit("Founder", game.GetUniqueID(game.playerDictionary[i].teamNum), game.playerDictionary[i].teamNum);
                game.mainGameBoard.gameHexDict[PickRandomValidHex()].SpawnUnit(playerSettler, false, true);
            }

        }



    }

    public Hex PickRandomValidHex()
    {
        List<Hex> list = new List<Hex>();
        foreach (Hex hex in game.mainGameBoard.gameHexDict.Keys)
        {
            if (game.mainGameBoard.gameHexDict[hex].terrainType == TerrainType.Flat && game.mainGameBoard.gameHexDict[hex].units.Count == 0 && game.mainGameBoard.gameHexDict[hex].district == null)
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
        Global.menuManager.Hide();
        Global.menuManager.ClearMenus();
    }

    private void startTerrainDemo()
    {
        Global.Log("Starting Game");
        Layout pointyReal = new Layout(Layout.pointy, new Point(10, 10), new Point(0, 0));
        Layout pointy = new Layout(Layout.pointy, new Point(-10, 10), new Point(0, 0));
        Global.layout = pointy;

        if (game.mainGameBoard == null)
        {
            game = GameTests.TestSlingerCombat();
            SaveGame("test.txt");
            game = LoadGame("test.txt");
        }

        InitGraphics(game, Global.layout);
        Global.menuManager.Hide();
        Global.menuManager.ClearMenus();
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
            game.turnManager.EndCurrentTurn(0);
        }

        if (game.teamManager.relationships.ContainsKey(2))
        {

            game.turnManager.EndCurrentTurn(2);
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
            Global.Log("Local move command recevied, sending to network and loopback.");
            Global.networkPeer.CommandAllPeers(CommandParser.ConstructMoveUnitCommand(unitID, hex, isEnemy));
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
            return;
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
            Global.networkPeer.CommandAllPeers(CommandParser.ConstructActivateAbilityCommand(unitID, AbilityName, Target));
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
            return;
        }
    }

    public void AddToProductionQueue(int cityID, string name, Hex targetHex, bool front = false, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeers(CommandParser.ConstructAddToProductionQueueCommand(cityID,name,targetHex,front));
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
                return;
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
                return;
            }
        }

    }

    public void RemoveFromProductionQueue(int cityID, int index, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeers(CommandParser.ConstructRemoveFromProductionQueueCommand(cityID, (ulong)index));
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
            return;
        }

    }

    public void MoveToFrontOfProductionQueue(int cityID, int index, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeers(CommandParser.ConstructMoveToFrontOfProductionQueueCommand(cityID, (ulong)index));
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
            return;
        }

    }


    public void ExpandToHex(int cityID, Hex Target, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeers(CommandParser.ConstructExpandToHexCommand(cityID, Target));
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
            return;
        }
    }

    public void DevelopDistrict(int cityID, Hex Target, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeers(CommandParser.ConstructDevelopDistrictCommand(cityID, Target));
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
            city.DevelopDistrict(Target);
        }
        catch (Exception e)
        {
            Global.Log("Error developing district: " + e.Message); //TODO - Potential Desync
            return;
        }
    }
}

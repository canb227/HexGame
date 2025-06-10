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
public partial class GameManager: Node
{
    public static GameManager instance;
    public GraphicManager graphicManager;
    public Game game;
    

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
        Global.debugLog("Loading Game from file: " + filePath);
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
        Global.debugLog("Starting Game");
        Layout pointyReal = new Layout(Layout.pointy, new Point(10, 10), new Point(0, 0));
        Layout pointy = new Layout(Layout.pointy, new Point(-10, 10), new Point(0, 0));
        Global.layout = pointy;

        if (game.mainGameBoard == null)
        {
            game = GameTests.TestSlingerCombat();
            SaveGame("test.txt");
            game = LoadGame("test.txt");
        }

        
        game.localPlayerTeamNum=teamNum;
        InitGraphics(game, Global.layout);
        Global.menuManager.Hide();
        Global.menuManager.ClearMenus();
    }

    public void startDebugGame(string savePath, int teamNum)
    {
        Global.debugLog("Starting Game");
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
        Global.debugLog("Starting Game");
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
        Global.debugLog("Initializing Graphics");
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
        game.turnManager.EndCurrentTurn(0);
        game.turnManager.EndCurrentTurn(2);
        List<int> waitingForPlayerList = game.turnManager.CheckTurnStatus();
        if (!waitingForPlayerList.Any())
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
            Global.debugLog("Local move command recevied, sending to network and loopback.");
            Global.networkPeer.CommandAllPeers(CommandParser.ConstructMoveUnitCommand(unitID, hex, isEnemy));
            return;
        }
        else
        {
            Global.debugLog("Network (or loopback) move command recevied, executing.");
        }


        Unit unit = SearchUnitByID(unitID);
        if (unit == null)
        {
            Global.debugLog("Unit is null"); //TODO - Potential Desync
            return;
        }

        GameHex target = game.mainGameBoard.gameHexDict[hex];
        if (target == null)
        {
            Global.debugLog("Target hex is null");//TODO - Potential Desync
            return;
        }

        if (isEnemy != Global.gameManager.game.mainGameBoard.gameHexDict[hex].IsEnemyPresent(unit.teamNum))
        {
            Global.debugLog("DESYNC ALARM");
        }
        try
        {
            unit.MoveTowards(target, Global.gameManager.game.teamManager, isEnemy);
        }
        catch (Exception e)
        {
            Global.debugLog("Error moving unit: " + e.Message); //TODO - Potential Desync
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
            Global.debugLog("Unit is null"); //TODO - Potential Desync
            return;
        }

        GameHex target = game.mainGameBoard.gameHexDict[Target];
        if (target == null)
        {
            Global.debugLog("Target hex is null");//TODO - Potential Desync
            return;
        }

        UnitAbility ability = unit.abilities.Find(x => x.name == AbilityName);
        if (ability == null)
        {
            Global.debugLog("Ability is null"); //TODO - Potential Desync
            return;
        }

        try
        {
            ability.ActivateAbility(target);
        }
        catch (Exception e)
        {
            Global.debugLog("Error activating ability: " + e.Message); //TODO - Potential Desync
            return;
        }
    }

    public void ChangeProductionQueue(City city, List<ProductionQueueType> queue, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeers(CommandParser.ConstructChangeProductionQueueCommand(city,queue));
            return;
        }

        if (city == null)
        {
            Global.debugLog("City is null"); //TODO - Potential Desync
            return;
        }

        if (queue == null)
        {
            Global.debugLog("Queue is null"); //TODO - Potential Desync
            return;
        }

        try
        {
            //city.SetProductionQueue(queue);
        }
        catch (Exception e)
        {
            Global.debugLog("Error changing production queue: " + e.Message); //TODO - Potential Desync
            return;
        }
    }
}

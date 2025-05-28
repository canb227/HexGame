using Godot;
using NetworkMessages;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using Godot;
using System.Linq;

[GlobalClass]
public partial class GameManager: Node
{
    public static GameManager instance;
    public GraphicManager graphicManager;
    public HexGameCamera camera;
    public Game game;
    

    public GameManager()
    {
        instance = this;
        Global.gameManager = this;
    }


    public void SaveGame(String filePath)
    {
        JsonSerializerOptions options = new JsonSerializerOptions 
        { 
            WriteIndented = true, 
            Converters = { new HexJsonConverter() } 
        };
        string json = JsonSerializer.Serialize(game, options);
        File.WriteAllText(filePath, json);
        GD.Print("Game data saved!");
    }


    public Game LoadGame(String filePath)
    {
        StreamReader sr = new StreamReader(filePath);
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new HexJsonConverter() }
        };
        return JsonSerializer.Deserialize<Game>(sr.ReadToEnd(), options);
    }

    public void startGame()
    {
        Global.debugLog("Starting Game");
        Layout pointyReal = new Layout(Layout.pointy, new Point(10, 10), new Point(0, 0));
        Layout pointy = new Layout(Layout.pointy, new Point(-10, 10), new Point(0, 0));
        Global.layout = pointy;

        game = GameTests.TestSlingerCombat();
        SaveGame("C:/Users/jeffe/Desktop/Stuff/HexGame/game_data.txt");
        Game loadedgame = LoadGame("C:/Users/jeffe/Desktop/Stuff/HexGame/game_data.txt");
        InitGraphics(loadedgame, Global.layout);
        Global.menuManager.Hide();
        Global.menuManager.ClearMenus();
    }

    private void InitGraphics(Game game, Layout layout)
    {
        Global.debugLog("Initializing Graphics");
        graphicManager = new GraphicManager(game, layout);
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

    public void MoveUnit(int unitID, Hex Target, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeers(CommandParser.ConstructMoveUnitCommand(unitID, Target));
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

        try
        {
            unit.TryMoveToGameHex(target, game.teamManager);
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

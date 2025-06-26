using Godot;
using NetworkMessages;
using System;
using System.Linq;

public partial class Mainmenu : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void onPlayButtonPressed()
    {
		Global.menuManager.LoadLobby();
    }

	public void onDebugStart1Pressed()
	{
        Global.gameManager.startGame(1);
    }

    public void onDebugStart2Pressed()
    {
        MapGenerator mapGenerator = new MapGenerator();

        mapGenerator.mapSize = MapGenerator.MapSize.Tiny;

        mapGenerator.numberOfPlayers = 1;
        mapGenerator.numberOfHumanPlayers = 1;
        mapGenerator.generateRivers = false;
        mapGenerator.resourceAmount = MapGenerator.ResourceAmount.Medium;
        mapGenerator.mapType = MapGenerator.MapType.DebugSquare;

        string mapData = mapGenerator.GenerateMap();

        Global.gameManager.game = new Game(1);
        Global.gameManager.game.mainGameBoard.InitGameBoardFromData(mapData, mapGenerator.right, mapGenerator.bottom);
        Global.gameManager.game.AddPlayer(10, 0, 0, Global.menuManager.lobby.PlayerColors[(int)Global.menuManager.lobby.PlayerStatuses[Global.clientID].ColorIndex], true);
        Global.gameManager.game.AddPlayer(10, 1, Global.clientID, Global.menuManager.lobby.PlayerColors[(int)Global.menuManager.lobby.PlayerStatuses[Global.clientID].ColorIndex], false);
        Global.gameManager.startGame(1);
    }

    public void onDebugStart3Pressed()
    {
        MapGenerator mapGenerator = new MapGenerator();

        mapGenerator.mapSize = MapGenerator.MapSize.Medium;

        mapGenerator.numberOfPlayers = 1;
        mapGenerator.numberOfHumanPlayers = 1;
        mapGenerator.generateRivers = false;
        mapGenerator.resourceAmount = MapGenerator.ResourceAmount.Medium;
        mapGenerator.mapType = MapGenerator.MapType.Continents;

        string mapData = mapGenerator.GenerateMap();

        Global.gameManager.game = new Game(1);
        Global.gameManager.game.mainGameBoard.InitGameBoardFromData(mapData, mapGenerator.right, mapGenerator.bottom);
        Global.gameManager.game.AddPlayer(10, 0, 0, Global.menuManager.lobby.PlayerColors[(int)Global.menuManager.lobby.PlayerStatuses[Global.clientID].ColorIndex], true);
        Global.gameManager.game.AddPlayer(10, 1, Global.clientID, Global.menuManager.lobby.PlayerColors[(int)Global.menuManager.lobby.PlayerStatuses[Global.clientID].ColorIndex],false);
        Global.gameManager.startGame(1);
    }

    public void onOptionsButtonPressed()
    {

    }
	public void onExitButtonPressed()
    {
        GetTree().Quit();
    }
}

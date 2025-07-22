using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data;
using Godot;
using System.Diagnostics.Metrics;
using System.Threading;
using NetworkMessages;
using System.IO;

[Serializable]
public class Game
{

    public Dictionary<int, ulong> teamNumToPlayerID { get; set; } = new Dictionary<int, ulong>();
    public GameBoard mainGameBoard { get; set; }
    public Dictionary<int, Player> playerDictionary { get; set; } = new();
    public Dictionary<int, City> cityDictionary { get; set; } = new();
    public Dictionary<int, Unit> unitDictionary { get; set; } = new();
    public HashSet<String> builtWonders { get; set; } = new();
    public TeamManager teamManager { get; set; }
    public TurnManager turnManager { get; set; }
    private int currentID = 0;

    public Player localPlayerRef;

    public int CurrentID
    {
        get => currentID;
        set => currentID = value;
    }
    public int localPlayerTeamNum { get; set; }

    //blank used for loading a game
    public Game()
    {
    }

    public Game(int localPlayerTeamNum)
    {
        Global.gameManager.game = this;
        this.localPlayerTeamNum = localPlayerTeamNum;
        this.playerDictionary = new();
        this.cityDictionary = new();
        this.unitDictionary = new();
        this.turnManager = new TurnManager();
        this.teamManager = new TeamManager();
        mainGameBoard = new();
        builtWonders = new();
        this.localPlayerTeamNum = localPlayerTeamNum;
    }



    public void AssignGameBoard(GameBoard mainGameBoard)
    {
        this.mainGameBoard = mainGameBoard;
    }

    public void AssignTurnManager(TurnManager turnManager)
    {
        this.turnManager = turnManager;
    }

    public void AddPlayer(float startGold, int teamNum, FactionType faction, ulong playerID, Godot.Color teamColor, bool isAI, bool isEncampment)
    {
        Global.Log("Checking player to game with color:" + teamColor.ToString());
        Player newPlayer = new Player(startGold, teamNum, faction, teamColor, isAI, isEncampment);
        Global.gameManager.game.teamNumToPlayerID.Add(teamNum, playerID);
    }

    public int GetUniqueID(int teamID)
    {
        return Global.gameManager.game.playerDictionary[teamID].GetNextUniqueID();
    }

}

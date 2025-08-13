using Godot;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static AIUtils;

[Serializable]
public class TurnManager
{
    public int currentTurn { get; set; } = 0;


    public void StartNewTurn()
    {
        currentTurn++;
        Global.Log("NEWTURN: " + currentTurn + "///////////////////////////////////////////NEWTURN" + currentTurn + "///////////////////////////////////////////////////////NEWTURN " + currentTurn + " ///////////////////////////////////////////NEWTURN" + currentTurn + "");
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
        {
            manager.CallDeferred("Update2DUI", (int)UIElement.turnNumber);
            manager.CallDeferred("Update2DUI", (int)UIElement.unitDisplay);
        }
        foreach (Player player in Global.gameManager.game.playerDictionary.Values)
        {
            player.OnTurnStarted(currentTurn);
        }
        if(Global.gameManager.game.mainGameBoard != null)
        {
            Global.gameManager.game.mainGameBoard.OnTurnStarted(currentTurn);
        }

        Global.gameManager.graphicManager.uiManager.NewTurnStarted();
    }
    public void EndCurrentTurn(int teamNum)
    {
        Global.Log("Attempting to end turn of " +teamNum);
        if (!Global.gameManager.game.playerDictionary[teamNum].turnFinished)
        {
            Global.gameManager.game.playerDictionary[teamNum].OnTurnEnded(currentTurn);
            Global.Log("Ended end turn of " +teamNum);
        }
        Global.Log($"Still wating for {CheckTurnStatus().Count} players to end turn! (numAI: {Global.gameManager.game.numAI}");
        if (CheckTurnStatus().Count == Global.gameManager.game.numAI)
        {
            //run gameboard hex based logic (volcanos and stuff?)
            //Global.gameManager.game.mainGameBoard.OnTurnEnded(currentTurn);

            if (Global.gameManager.isHost)
            {
                Global.Log("All non-AI players have gone, running AI turns");
                //foreach (AI ai in Global.gameManager.AIManager.aiList)
                //{
                //    Task AIThread = Task.Run(() => Global.gameManager.AIManager.RunAITurn(ai));
                //}
                //Global.gameManager.AIManager.RunAllAITurns();
                Task AIThread = Task.Run(() => Global.gameManager.AIManager.RunAllAITurns())
                .ContinueWith(t =>
                {
                    if (t.Exception != null)
                    {
                        foreach (var ex in t.Exception.Flatten().InnerExceptions)
                        {
                            Console.WriteLine($"AI error: {ex.Message}");
                        }
                        foreach(Player player in Global.gameManager.game.playerDictionary.Values)
                        {
                            if(!player.turnFinished)
                            {
                                Global.gameManager.EndTurn(player.teamNum);
                            }
                        }
                        //Global.gameManager.SaveGame(OS.GetUserDataDir() + "/saves/testsave.txt");
                        throw t.Exception;
                    }
                }, TaskContinuationOptions.OnlyOnFaulted);

            }
        }
    }

    public List<int> CheckTurnStatus()
    {
        List<int> waitingForPlayers = new List<int>();
        foreach (Player player in Global.gameManager.game.playerDictionary.Values)
        {
            if(!player.turnFinished)
            {
                waitingForPlayers.Add(player.teamNum);
            }
        }
        return waitingForPlayers;
    }
}

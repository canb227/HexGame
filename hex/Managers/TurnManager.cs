using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data;
using System.IO;
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

        //Run AI on a new thread
        //if(Global.gameManager.isHost)
        //{
        //    Task AIThread = Task.Run(() => Global.gameManager.AIManager.OnTurnStart());
        //}
        /*try
        {
            AIThread.Wait();
        }
        catch (Exception ex) { throw; }*/


        //Run AI on main thread
        if (Global.gameManager.isHost)
        {
            Global.Log("Starting AI turn in debug mode (single threaded)");
            Global.gameManager.AIManager.OnTurnStart();
        }



    }
    public void EndCurrentTurn(int teamNum)
    {
        //Global.debugLog("ending turn for team: " + teamNum );
        if (!Global.gameManager.game.playerDictionary[teamNum].turnFinished)
        {
            Global.gameManager.game.playerDictionary[teamNum].OnTurnEnded(currentTurn);
            if (Global.gameManager.game.mainGameBoard != null && teamNum == 0)
            {
                Global.gameManager.game.mainGameBoard.OnTurnEnded(currentTurn);
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

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

        Global.gameManager.graphicManager.uiManager.NewTurnStarted();
    }
    public void EndCurrentTurn(int teamNum)
    {
        if (!Global.gameManager.game.playerDictionary[teamNum].turnFinished)
        {
            Global.gameManager.game.playerDictionary[teamNum].OnTurnEnded(currentTurn);
            if (Global.gameManager.game.mainGameBoard != null && teamNum == 0)
            {
                Global.gameManager.game.mainGameBoard.OnTurnEnded(currentTurn);
            }
        }
        if (CheckTurnStatus().Count == Global.gameManager.game.numAI)
        {
            if (Global.gameManager.isHost)
            {
                Task AIThread = Task.Run(() => Global.gameManager.AIManager.RunAllAITurns());
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

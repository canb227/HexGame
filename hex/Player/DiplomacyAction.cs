using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class DiplomacyAction
{
    public int teamNum;
    public string actionName;
    public bool hasDuration;
    public int duration;
    public int targetTeamNum;
    public bool hasQuantity;
    public int quantity = 0;
    public DiplomacyAction(int teamNum, string actionName, bool hasQuantity, bool hasDuration)
    {
        this.teamNum = teamNum;
        this.actionName = actionName;
        this.hasQuantity = hasQuantity;
        this.hasDuration = hasDuration;
    }

    public DiplomacyAction() { }

    public void ActivateAction()
    {
        if (actionName == "Give Gold")
        {
            GiveGold(targetTeamNum, quantity);
        }
        if (actionName == "Give Gold Per Turn")
        {
            StartGoldPerTurn(targetTeamNum, quantity, duration);
        }
        if (actionName == "Make Peace")
        {
            MakePeace(targetTeamNum);
        }
        if(actionName == "Make Alliance")
        {
            MakeAlliance(targetTeamNum);
        }
        if(actionName == "Share Map")
        {
            ShareMap(targetTeamNum);
        }
    }

    public bool ActionValid(int targetTeamNum)
    {
        if(actionName == "Give Gold")
        {
            if (Global.gameManager.game.playerDictionary[teamNum].goldTotal <= 0)
            {
                return false;
            }
        }

        if(actionName == "Give Gold Per Turn")
        {
            if (Global.gameManager.game.playerDictionary[teamNum].GetGoldPerTurn() <= 0)
            {
                return false;
            }
            if (Global.gameManager.game.teamManager.goldTradeDealDict.ContainsKey(new TradeDealKey { sender = teamNum, reciever = targetTeamNum}))
            {
                return false;
            }    
        }

        if(actionName == "Make Peace")
        {
            if(!Global.gameManager.game.teamManager.GetEnemies(teamNum).Contains(targetTeamNum))
            {
                return false;
            }
        }

        if(actionName == "Make Alliance")
        {
            if (Global.gameManager.game.teamManager.GetEnemies(teamNum).Contains(targetTeamNum))
            {
                return false;
            }
            if(Global.gameManager.game.teamManager.GetAllies(teamNum).Contains(targetTeamNum))
            {
                return false;
            }
        }
        if (actionName == "Share Map")
        {
            //no limit
        }

        return true;
    }


    private void GiveGold(int targetTeamNum, int goldAmount)
    {
        Global.gameManager.game.playerDictionary[teamNum].goldTotal -= goldAmount;
        Global.gameManager.game.playerDictionary[targetTeamNum].goldTotal += goldAmount;
    }

    private void StartGoldPerTurn(int targetTeamNum, int goldAmount, int turnsLeft)
    {
        Global.gameManager.game.teamManager.goldTradeDealDict[new TradeDealKey { sender = teamNum, reciever = targetTeamNum }] = new GoldPerTurnDeal { amount = goldAmount, turnsLeft = turnsLeft };
    }
    private void MakePeace(int targetTeamNum)
    {
        Global.gameManager.game.teamManager.SetDiplomaticState(teamNum, targetTeamNum, DiplomaticState.ForcedPeace);
        Global.gameManager.game.playerDictionary[teamNum].turnsUntilForcedPeaceEnds[targetTeamNum] = 30;
        Global.gameManager.game.playerDictionary[targetTeamNum].turnsUntilForcedPeaceEnds[teamNum] = 30;
    }

    private void MakeAlliance(int targetTeamNum)
    {
        Global.gameManager.game.teamManager.SetDiplomaticState(teamNum, targetTeamNum, DiplomaticState.Ally);
        //store copies of targetTeamNums dictionaries for later
        Dictionary<Hex, bool> tempSeen = Global.gameManager.game.playerDictionary[targetTeamNum].seenGameHexDict;

        //add seen hexes to target's seen set
        foreach (var hexCountPair in Global.gameManager.game.playerDictionary[teamNum].seenGameHexDict)
        {
            if (Global.gameManager.game.playerDictionary[targetTeamNum].seenGameHexDict.Keys.Contains(hexCountPair.Key))
            {
                if(hexCountPair.Value)
                {
                    Global.gameManager.game.playerDictionary[targetTeamNum].seenGameHexDict[hexCountPair.Key] = hexCountPair.Value;
                }
            }
            else
            {
                Global.gameManager.game.playerDictionary[targetTeamNum].seenGameHexDict.Add(hexCountPair.Key, hexCountPair.Value);
            }
        }

        //add visible hexes to target's visible set
        foreach (var hexCountPair in Global.gameManager.game.playerDictionary[teamNum].personalVisibleGameHexDict)
        {
            if (Global.gameManager.game.playerDictionary[targetTeamNum].visibleGameHexDict.Keys.Contains(hexCountPair.Key))
            {
                Global.gameManager.game.playerDictionary[targetTeamNum].visibleGameHexDict[hexCountPair.Key] += hexCountPair.Value;
            }
            else
            {
                Global.gameManager.game.playerDictionary[targetTeamNum].visibleGameHexDict.Add(hexCountPair.Key, hexCountPair.Value);
            }
        }

        //inverse for player 2 back to player 1 now using tempSeen and tempVisible from before
        //add seen hexes to target's seen set
        foreach (var hexCountPair in tempSeen)
        {
            if (Global.gameManager.game.playerDictionary[teamNum].seenGameHexDict.Keys.Contains(hexCountPair.Key))
            {
                if (hexCountPair.Value)
                {
                    Global.gameManager.game.playerDictionary[teamNum].seenGameHexDict[hexCountPair.Key] = hexCountPair.Value;
                }
            }
            else
            {
                Global.gameManager.game.playerDictionary[teamNum].seenGameHexDict.Add(hexCountPair.Key, hexCountPair.Value);
            }
        }

        //add visible hexes to target's visible set
        foreach (var hexCountPair in Global.gameManager.game.playerDictionary[targetTeamNum].personalVisibleGameHexDict)
        {
            if (Global.gameManager.game.playerDictionary[teamNum].visibleGameHexDict.Keys.Contains(hexCountPair.Key))
            {
                Global.gameManager.game.playerDictionary[teamNum].visibleGameHexDict[hexCountPair.Key] += hexCountPair.Value;
            }
            else
            {
                Global.gameManager.game.playerDictionary[teamNum].visibleGameHexDict.Add(hexCountPair.Key, hexCountPair.Value);
            }
        }

        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager)) manager.CallDeferred("UpdateGraphic", Global.gameManager.game.mainGameBoard.id, (int)GraphicUpdateType.Update);
    }

    private void BreakAlliance(int targetTeamNum)
    {
        Global.gameManager.game.teamManager.SetDiplomaticState(teamNum, targetTeamNum, DiplomaticState.Ally);
        //remove visible hexes from target's visible set
        foreach (var hexCountPair in Global.gameManager.game.playerDictionary[teamNum].personalVisibleGameHexDict)
        {
            if (Global.gameManager.game.playerDictionary[targetTeamNum].visibleGameHexDict.Keys.Contains(hexCountPair.Key))
            {
                Global.gameManager.game.playerDictionary[targetTeamNum].visibleGameHexDict[hexCountPair.Key] -= hexCountPair.Value;
            }
        }

        foreach (var hexCountPair in Global.gameManager.game.playerDictionary[targetTeamNum].personalVisibleGameHexDict)
        {
            if (Global.gameManager.game.playerDictionary[teamNum].visibleGameHexDict.Keys.Contains(hexCountPair.Key))
            {
                Global.gameManager.game.playerDictionary[teamNum].visibleGameHexDict[hexCountPair.Key] -= hexCountPair.Value;
            }
        }
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager)) manager.CallDeferred("UpdateGraphic", Global.gameManager.game.mainGameBoard.id, (int)GraphicUpdateType.Update);
    }

    private void ShareMap(int targetTeamNum)
    {
        foreach (var hexCountPair in Global.gameManager.game.playerDictionary[teamNum].seenGameHexDict)
        {
            if (Global.gameManager.game.playerDictionary[targetTeamNum].seenGameHexDict.Keys.Contains(hexCountPair.Key))
            {
                if (hexCountPair.Value)
                {
                    Global.gameManager.game.playerDictionary[targetTeamNum].seenGameHexDict[hexCountPair.Key] = hexCountPair.Value;
                }
            }
            else
            {
                Global.gameManager.game.playerDictionary[targetTeamNum].seenGameHexDict.Add(hexCountPair.Key, hexCountPair.Value);
            }
        }
    }

}

public struct TradeDealKey
{
    public int sender;
    public int reciever;
}

public struct GoldPerTurnDeal
{
    public int amount;
    public int turnsLeft;
}
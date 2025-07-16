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
    }

    private void ShareMap(int targetTeamNum)
    {
        Global.gameManager.game.playerDictionary[targetTeamNum].seenGameHexDict.Concat(Global.gameManager.game.playerDictionary[teamNum].seenGameHexDict);
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
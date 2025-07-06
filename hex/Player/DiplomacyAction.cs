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
        if (actionName == "Make Peace")
        {
            MakePeace(targetTeamNum);
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
        }

        if(actionName == "Make Peace")
        {
            if(!Global.gameManager.game.teamManager.GetEnemies(teamNum).Contains(targetTeamNum))
            {
                return false;
            }
        }

        return true;
    }


    private void GiveGold(int targetTeamNum, int goldAmount)
    {
        Global.gameManager.game.playerDictionary[teamNum].goldTotal -= goldAmount;
        Global.gameManager.game.playerDictionary[targetTeamNum].goldTotal += goldAmount;
    }
    private void MakePeace(int targeTeamNum)
    {
        //MAKE PEACE
    }

}

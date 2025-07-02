using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class DiplomacyAction
{
    int teamNum;
    public string actionName;
    public DiplomacyAction(int teamNum, string actionName)
    {
        this.teamNum = teamNum;
        this.actionName = actionName;
    }

    public void ActivateAction(object actionParameter1, object actionParameter2)
    {
        if (actionName == "Give Gold")
        {
            GiveGold((int)actionParameter1, (int)actionParameter2);
        }
        if (actionName == "Make Peace")
        {
            MakePeace((int)actionParameter1);
        }
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

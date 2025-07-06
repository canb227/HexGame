using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public partial class DiplomacyDeal : GodotObject
{
    public int id;
    public int fromTeamNum;
    public int toTeamNum;

    public List<DiplomacyAction> offersList;
    public List<DiplomacyAction> requestsList;
    public DiplomacyDeal(int fromTeamNum, int toTeamNum, List<DiplomacyAction> offersList, List<DiplomacyAction> requestsList)
    {
        this.id = Global.gameManager.game.playerDictionary[fromTeamNum].GetNextUniqueID();
        this.fromTeamNum = fromTeamNum;
        this.toTeamNum = toTeamNum;
        this.offersList = offersList;
        this.requestsList = requestsList;
    }

}

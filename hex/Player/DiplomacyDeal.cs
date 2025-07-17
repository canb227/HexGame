using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public partial class DiplomacyDeal
{
    public int id;
    public int fromTeamNum; //sendingTeamNum;
    public int toTeamNum; //receivingTeamNum;

    public List<DiplomacyAction> offersList; //offersList;
    public List<DiplomacyAction> requestsList; //requestsList;
    public DiplomacyDeal(int id, int fromTeamNum, int toTeamNum, List<DiplomacyAction> offersList, List<DiplomacyAction> requestsList)
    {
        this.id = id;
        this.fromTeamNum = fromTeamNum;
        this.toTeamNum = toTeamNum;
        this.offersList = offersList;
        this.requestsList = requestsList;
    }

}

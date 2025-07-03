using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class DiplomacyDeal
{
    public int sendingTeamNum;
    public int receivingTeamNum;
    public List<DiplomacyAction> senderOfferingList;
    public List<DiplomacyAction> receivingOfferingList;
    public DiplomacyDeal(int sendingTeamNum, int receivingTeamNum, List<DiplomacyAction> senderOfferingList, List<DiplomacyAction> receivingOfferingList)
    {
        this.sendingTeamNum = sendingTeamNum;
        this.receivingTeamNum = receivingTeamNum;
        this.senderOfferingList = senderOfferingList;
        this.receivingOfferingList = receivingOfferingList;
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class TradeRoute
{
    public int homeCityID;
    public int targetCityID;
    public TradeRoute(int homeCityID, int targetCityID)
    {
        this.homeCityID = homeCityID;
        this.targetCityID = targetCityID;
    }

    public override bool Equals(object obj)
    {
        if (obj is TradeRoute)
        {
            return homeCityID == ((TradeRoute)obj).homeCityID && targetCityID == ((TradeRoute)obj).targetCityID;
        }
        return false;
    }
    public override int GetHashCode()
    {
        return (homeCityID, targetCityID).GetHashCode();
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class ExportRoute
{
    public int sourceCityID;
    public int targetCityID;
    public YieldType exportType;
    public ExportRoute(int sourceCityID, int targetCityID, YieldType exportType)
    {
        this.sourceCityID = sourceCityID;
        this.targetCityID = targetCityID;
        this.exportType = exportType;
    }

    public override bool Equals(object obj)
    {
        if (obj is ExportRoute)
        {
            return sourceCityID == ((ExportRoute)obj).sourceCityID && targetCityID == ((ExportRoute)obj).targetCityID && exportType == ((ExportRoute)obj).exportType;
        }
        return false;
    }
    public override int GetHashCode()
    {
        return (sourceCityID, targetCityID, exportType).GetHashCode();
    }
}
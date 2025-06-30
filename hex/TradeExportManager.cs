using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class TradeExportManager
{
    public HashSet<ExportRoute> exportRouteHashSet { get; set; } = new();

    public TradeExportManager()
    {
    }
    public void RecalculateExportsFromCity(int sourceCity)
    {
        foreach(ExportRoute export in exportRouteHashSet)
        {
            if(export.sourceCityID == sourceCity)
            {
                Global.gameManager.game.cityDictionary[export.targetCityID].RecalculateYields();
            }
        }
    }
    public void NewExportRoute(int city, int targetCity, YieldType exportType)
    {
        Global.gameManager.game.cityDictionary[city].NewExport(exportType);
        exportRouteHashSet.Add(new ExportRoute(city, targetCity, exportType));
        Global.gameManager.game.cityDictionary[city].RecalculateYields();
        Global.gameManager.game.cityDictionary[targetCity].RecalculateYields();
    }

    public void RemoveExportRoute(int city, int targetCity, YieldType exportType)
    {
        Global.gameManager.game.cityDictionary[city].RemoveExport(exportType);
        exportRouteHashSet.Remove(new ExportRoute(city, targetCity, exportType));
        Global.gameManager.game.cityDictionary[city].RecalculateYields();
        Global.gameManager.game.cityDictionary[targetCity].RecalculateYields();
    }

}

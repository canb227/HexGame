using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class TradeExportManager
{
    public List<ExportRoute> exportRouteList { get; set; } = new();

    public TradeExportManager()
    {
    }
    public void RecalculateExportsFromCity(int sourceCity)
    {
        foreach(ExportRoute export in exportRouteList)
        {
            if(export.sourceCityID == sourceCity)
            {
                Global.gameManager.game.cityDictionary[export.targetCityID].RecalculateYields();
            }
        }
    }
    public void NewExportRoute(int city, int targetCity, YieldType exportType)
    {
        Global.gameManager.game.playerDictionary[Global.gameManager.game.cityDictionary[city].teamNum].exportCount++;
        Global.gameManager.game.cityDictionary[city].NewExport(exportType);
        exportRouteList.Add(new ExportRoute(city, targetCity, exportType));
        Global.gameManager.game.cityDictionary[city].RecalculateYields();
        Global.gameManager.game.cityDictionary[targetCity].RecalculateYields();
    }

    public void RemoveExportRoute(int city, int targetCity, YieldType exportType)
    {
        Global.gameManager.game.playerDictionary[Global.gameManager.game.cityDictionary[city].teamNum].exportCount--;
        Global.gameManager.game.cityDictionary[city].RemoveExport(exportType);
        exportRouteList.Remove(new ExportRoute(city, targetCity, exportType));
        Global.gameManager.game.cityDictionary[city].RecalculateYields();
        Global.gameManager.game.cityDictionary[targetCity].RecalculateYields();
    }

}

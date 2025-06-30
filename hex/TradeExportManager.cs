using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class TradeExportManager
{
    public HashSet<ExportRoute> exportRouteDictionary { get; set; } = new();

    public TradeExportManager()
    {
    }
    public void NewExportRoute(int city, int targetCity, YieldType exportType)
    {
        Global.gameManager.game.cityDictionary[city].NewExport(exportType);
        exportRouteDictionary.Add(new ExportRoute(city, targetCity, exportType));
        Global.gameManager.game.cityDictionary[city].RecalculateYields();
        Global.gameManager.game.cityDictionary[targetCity].RecalculateYields();
    }

    public void RemoveExportRoute(int city, int targetCity, YieldType exportType)
    {
        Global.gameManager.game.cityDictionary[city].RemoveExport(exportType);
        exportRouteDictionary.Remove(new ExportRoute(city, targetCity, exportType));
        Global.gameManager.game.cityDictionary[city].RecalculateYields();
        Global.gameManager.game.cityDictionary[targetCity].RecalculateYields();
    }

}

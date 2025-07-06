using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public enum FactionType
{
    All,
    Human,
    Goblins
}

public static class FactionLoader
{
    public static Dictionary<FactionType, HashSet<TerrainType>> factionPlacementDict = new();
    public static Dictionary<FactionType, String> factionCapitalBuildingDict = new();
    static FactionLoader()
    {
        //Humans
        HashSet<TerrainType> validPlacement = new();
        validPlacement.Add(TerrainType.Flat);
        validPlacement.Add(TerrainType.Rough);
        factionPlacementDict.Add(FactionType.Human, validPlacement);
        factionCapitalBuildingDict.Add(FactionType.Human, "Palace");

        //Goblins
        validPlacement = new();
        validPlacement.Add(TerrainType.Flat);
        validPlacement.Add(TerrainType.Rough);
        factionPlacementDict.Add(FactionType.Goblins, validPlacement);
        factionCapitalBuildingDict.Add(FactionType.Goblins, "GoblinGen");
    }

    public static string GetFactionCapitalBuilding(FactionType faction)
    {
        if(faction == FactionType.Goblins)
        {
            return "GoblinDen";
        }
        else
        {
            return "CityCenter";
        }
    }

    public static bool IsFactionMinor(FactionType faction)
    {
        if(faction == FactionType.Human)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
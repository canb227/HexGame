using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public enum FactionType
{
    Human,
    Goblins
}

public static class FactionLoader
{
    public static Dictionary<FactionType, HashSet<TerrainType>> factionPlacementDict = new();
    static FactionLoader()
    {
        //Humans
        HashSet<TerrainType> validPlacement = new();
        validPlacement.Add(TerrainType.Flat);
        validPlacement.Add(TerrainType.Rough);
        factionPlacementDict.Add(FactionType.Goblins, validPlacement);

        //Goblins
        validPlacement = new();
        validPlacement.Add(TerrainType.Flat);
        validPlacement.Add(TerrainType.Rough);
        factionPlacementDict.Add(FactionType.Goblins, validPlacement);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public enum NeutralType
{
    Goblins
}

public static class NeutralLoader
{
    public static Dictionary<NeutralType, HashSet<TerrainType>> neutralPlacementDict = new();
    static NeutralLoader()
    {
        HashSet<TerrainType> validPlacement = new();
        validPlacement.Add(TerrainType.Flat);
        validPlacement.Add(TerrainType.Rough);
        neutralPlacementDict.Add(NeutralType.Goblins, validPlacement);
    }
}
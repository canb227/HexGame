using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public enum FactionType
{
    All,
    Humans,
    Orcs,
    Elves,
    Beastfolk,
    Hobbits,
    Goblins
}

public static class FactionLoader
{
    public static Dictionary<FactionType, HashSet<TerrainType>> factionPlacementDict = new();
    public static Dictionary<FactionType, String> factionCapitalBuildingDict = new();
    static FactionLoader()
    {
        //Humans
        HashSet<TerrainType> validPlacementHuman = new();
        validPlacementHuman.Add(TerrainType.Flat);
        validPlacementHuman.Add(TerrainType.Rough);
        factionPlacementDict.Add(FactionType.Humans, validPlacementHuman);
        factionCapitalBuildingDict.Add(FactionType.Humans, "Palace");

        //Orcs
        HashSet<TerrainType> validPlacementOrc = new();
        validPlacementOrc.Add(TerrainType.Flat);
        validPlacementOrc.Add(TerrainType.Rough);
        factionPlacementDict.Add(FactionType.Orcs, validPlacementOrc);
        factionCapitalBuildingDict.Add(FactionType.Orcs, "Palace");

        //Elves
        HashSet<TerrainType> validPlacementElf = new();
        validPlacementElf.Add(TerrainType.Flat);
        validPlacementElf.Add(TerrainType.Rough);
        factionPlacementDict.Add(FactionType.Elves, validPlacementElf);
        factionCapitalBuildingDict.Add(FactionType.Elves, "Palace");

        //Beastfolk
        HashSet<TerrainType> validPlacementBeast = new();
        validPlacementBeast.Add(TerrainType.Flat);
        validPlacementBeast.Add(TerrainType.Rough);
        factionPlacementDict.Add(FactionType.Beastfolk, validPlacementBeast);
        factionCapitalBuildingDict.Add(FactionType.Beastfolk, "Palace");

        //Hobbits
        HashSet<TerrainType> validPlacementHobbit = new();
        validPlacementHobbit.Add(TerrainType.Flat);
        validPlacementHobbit.Add(TerrainType.Rough);
        factionPlacementDict.Add(FactionType.Hobbits, validPlacementHobbit);
        factionCapitalBuildingDict.Add(FactionType.Hobbits, "Palace");

        //Goblins
        HashSet<TerrainType> validPlacementGoblin = new();
        validPlacementGoblin.Add(TerrainType.Flat);
        validPlacementGoblin.Add(TerrainType.Rough);
        factionPlacementDict.Add(FactionType.Goblins, validPlacementGoblin);
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
        if(faction == FactionType.Humans || faction == FactionType.Orcs || faction == FactionType.Elves || faction == FactionType.Beastfolk || faction == FactionType.Hobbits)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
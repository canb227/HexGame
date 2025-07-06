using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

public enum DistrictType
{
    citycenter,
    rural,
    production,
    gold,
    science,
    culture,
    happiness,
    influence,
    dock,
    military,
    refinement,
    wonder
}

public struct BuildingInfo
{
    public DistrictType DistrictType;
    public FactionType FactionType;
    public int ProductionCost;
    public int GoldCost;
    public Yields yields;
    public float MaintenanceCost;
    public int PerCity;
    public int PerPlayer;
    public bool Wonder;
    public bool isFactionUnique;
    public String IconPath;
    public String ModelPath;
    public List<String> Effects;
    public List<TerrainType> TerrainTypes;
}

public static class BuildingLoader
{
    public static Dictionary<String, BuildingInfo> buildingsDict;
    public static Dictionary<DistrictType, BuildingInfo> districtDict;


    static BuildingLoader()
    {
        string xmlPath = "hex/Buildings.xml";
        buildingsDict = LoadBuildingData(xmlPath);
        districtDict = PrepDistrictData(buildingsDict);
    }
    
    public static Dictionary<String, BuildingInfo> LoadBuildingData(string xmlPath)
    {
        XDocument xmlDoc = XDocument.Load(xmlPath);
        var BuildingData = xmlDoc.Descendants("Building")
            .ToDictionary(
                r => r.Attribute("Name").Value,
                r => new BuildingInfo
                {
                    DistrictType = (DistrictType)Enum.Parse(typeof(DistrictType), r.Attribute("DistrictType").Value),
                    FactionType = Enum.TryParse<FactionType>(r.Attribute("Class")?.Value, out var factionType) ? factionType : FactionType.All,
                    ProductionCost = int.Parse(r.Attribute("ProductionCost").Value),
                    GoldCost = int.Parse(r.Attribute("GoldCost").Value),
                    yields = new Yields
                    {
                        food = float.Parse(r.Attribute("FoodYield").Value),
                        production = float.Parse(r.Attribute("ProductionYield").Value),
                        gold = float.Parse(r.Attribute("GoldYield").Value),
                        science = float.Parse(r.Attribute("ScienceYield").Value),
                        culture = float.Parse(r.Attribute("CultureYield").Value),
                        happiness = float.Parse(r.Attribute("HappinessYield").Value),
                        influence = float.Parse(r.Attribute("InfluenceYield").Value)
                    },
                    MaintenanceCost = float.Parse(r.Attribute("MaintenanceCost").Value),
                    PerCity = int.Parse(r.Attribute("PerCity").Value),
                    PerPlayer = int.Parse(r.Attribute("PerPlayer").Value),
                    Wonder = bool.Parse(r.Attribute("Wonder").Value),
                    IconPath = r.Attribute("IconPath")?.Value ?? "",
                    ModelPath = r.Attribute("ModelPath")?.Value ?? "",
                    Effects = r.Element("Effects").Elements("Effect").Select(e => e.Attribute("Name").Value).ToList(),
                    TerrainTypes = r.Element("TerrainTypes").Elements("TerrainType").Select(t => Enum.Parse<TerrainType>(t.Value)).ToList(),
                }
            );
        return BuildingData;
    }

    private static Dictionary<DistrictType, BuildingInfo> PrepDistrictData(Dictionary<String, BuildingInfo> buildingDict)
    {
        Dictionary<DistrictType, BuildingInfo> temp = new();
        temp.Add(DistrictType.citycenter, buildingDict["CityCenter"]);
        temp.Add(DistrictType.rural, buildingDict["Farm"]);
        temp.Add(DistrictType.refinement, buildingDict["Refinery"]);
        temp.Add(DistrictType.production, buildingDict["Industry"]);
        temp.Add(DistrictType.gold, buildingDict["Commerce"]);
        temp.Add(DistrictType.science, buildingDict["Campus"]);
        temp.Add(DistrictType.culture, buildingDict["Cultural"]);
        temp.Add(DistrictType.happiness, buildingDict["Entertainment"]);
        temp.Add(DistrictType.influence, buildingDict["Administrative"]);
        temp.Add(DistrictType.dock, buildingDict["Harbor"]);
        temp.Add(DistrictType.military, buildingDict["Militaristic"]);
        return temp;
    }
}

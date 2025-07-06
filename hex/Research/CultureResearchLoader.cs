using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

public static class CultureResearchLoader
{
    public static Dictionary<String, ResearchInfo> researchesDict;
    public static Dictionary<int, int> tierCostDict;


    static CultureResearchLoader()
    {
        string xmlPath = "hex/CultureResearches.xml";
        researchesDict = LoadResearchData(xmlPath);
        tierCostDict = new Dictionary<int, int>();
        tierCostDict.Add(0, 0);
        tierCostDict.Add(1, 25);
        tierCostDict.Add(2, 50);
        tierCostDict.Add(3, 80);
        tierCostDict.Add(4, 120);
        tierCostDict.Add(5, 200);
        tierCostDict.Add(6, 300);
        tierCostDict.Add(7, 390);
        tierCostDict.Add(8, 600);
        tierCostDict.Add(9, 730);
        tierCostDict.Add(10, 930);
        tierCostDict.Add(11, 1070);
        tierCostDict.Add(12, 1250);
        tierCostDict.Add(13, 1370);
        tierCostDict.Add(14, 1480);
        tierCostDict.Add(15, 1660);
        tierCostDict.Add(16, 1850);
        tierCostDict.Add(17, 2155);
        tierCostDict.Add(18, 2500);

    }
        
    public static Dictionary<String, ResearchInfo> LoadResearchData(string xmlPath)
    {
        // Load the XML file
        XDocument xmlDoc = XDocument.Load(xmlPath);

        // Parse the research data into a dictionary, allowing for nulls
        var ResearchData = xmlDoc.Descendants("Research")
            .ToDictionary(
                r => r.Attribute("Name")?.Value ?? throw new InvalidOperationException("Missing 'Name' attribute"),
                r => new ResearchInfo
                {
                    Tier = int.Parse(r.Attribute("Tier")?.Value ?? "0"),
                    FactionType = Enum.TryParse<FactionType>(r.Attribute("Class")?.Value, out var factionType) ? factionType : FactionType.All,
                    IconPath = r.Attribute("IconPath")?.Value ?? throw new InvalidOperationException("Missing 'IconPath' attribute"),
                    Requirements = r.Element("Requirements")?.Elements("ResearchType")
                        .Select(e => e.Value ?? throw new Exception("Invalid Stringy"))
                        .ToList() ?? new List<String>(),
                    BuildingUnlocks = r.Element("BuildingUnlocks")?.Elements("BuildingType")
                        .Select(e => e.Attribute("Name")?.Value ?? throw new Exception("Invalid BuildingUnlock"))
                        .ToList() ?? new List<string>(),
                    UnitUnlocks = r.Element("UnitUnlocks")?.Elements("UnitType")
                        .Select(e => e.Attribute("Name")?.Value ?? throw new Exception("Invalid UnitUnlock"))
                        .ToList() ?? new List<string>(),

                    Effects = r.Element("Effects")?.Elements("Effect")
                        .Select(e => e.Value)
                        .Where(e => !string.IsNullOrWhiteSpace(e))
                        .ToList() ?? new List<string>(),
                }
            );

        return ResearchData;
    }
    
    public static void ProcessFunctionString(String functionString, Player player)
    {
        Dictionary<String, Action<Player>> effectFunctions = new Dictionary<string, Action<Player>>
        {
            { "TribalDominionEffect", TribalDominionEffect },
            { "ForeignTradeEffect", ForeignTradeEffect },
            { "SongwritingEffect", SongwritingEffect },
            { "FutureTechEffect", FutureTechEffect }
        };
        
        if (effectFunctions.TryGetValue(functionString, out Action<Player> effectFunction))
        {
            effectFunction(player);
        }
        else
        {
            throw new ArgumentException($"Function '{functionString}' not recognized in ResearchEffects from Researches file.");
        }
    }
    static void TribalDominionEffect(Player player)
    {

    }

    static void ForeignTradeEffect(Player player)
    {

    }

    static void SongwritingEffect(Player player)
    {
    }
    static void FutureTechEffect(Player player)
    {

    }
}

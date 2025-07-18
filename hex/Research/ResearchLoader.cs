using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

public struct ResearchInfo
{
    public int Tier;
    public FactionType FactionType;
    public int VisualSlot;
    public string IconPath;
    public List<String> Requirements;
    public List<String> BuildingUnlocks;
    public List<String> UnitUnlocks;
    public List<ResourceType> ResourceUnlocks;
    public List<String> PolicyCardUnlocks;
    public List<GovernmentType> GovernmentUnlocks;
    public List<String> Effects;
}

public static class ResearchLoader
{
    public static Dictionary<String, ResearchInfo> researchesDict;
    public static Dictionary<int, int> tierCostDict;


    static ResearchLoader()
    {
        string xmlPath = "hex/Researches.xml";
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
                    VisualSlot = int.Parse(r.Attribute("VisualSlot")?.Value ?? "0"),
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
                    ResourceUnlocks = r.Element("ResourceUnlocks")?.Elements("ResourceType")
                        .Select(e => (ResourceType)Enum.Parse(typeof(ResourceType), e.Attribute("Name")?.Value
                            ?? throw new Exception("Invalid ResourceType")))
                        .ToList() ?? new List<ResourceType>(),
                    PolicyCardUnlocks = r.Element("PolicyCardUnlocks")?.Elements("PolicyCard")
                        .Select(e => e.Attribute("Name")?.Value ?? throw new Exception("Invalid UnitUnlock"))
                        .ToList() ?? new List<string>(),
                    GovernmentUnlocks = r.Element("GovernmentUnlocks")?.Elements("GovernmentType")
                        .Select(e => (GovernmentType)Enum.Parse(typeof(GovernmentType), e.Attribute("Name")?.Value
                            ?? throw new Exception("Invalid GovernmentType")))
                        .ToList() ?? new List<GovernmentType>(),
                    Effects = r.Element("Effects")?.Elements("Effect")
                        .Select(e => e.Attribute("Name")?.Value ?? throw new Exception("Missing Effect name"))
                        .Where(e => !string.IsNullOrWhiteSpace(e))
                        .ToList() ?? new List<string>(),
                }
            );

        return ResearchData;
    }

    static Dictionary<String, Func<Player, bool, string>> effectFunctions = new Dictionary<string, Func<Player, bool, string>>
        {
            { "AgricultureEffect", AgricultureEffect },
            { "SailingEffect", SailingEffect },
            { "PotteryEffect", PotteryEffect },
            { "AnimalHusbandryEffect", AnimalHusbandryEffect },
            { "IrrigationEffect", IrrigationEffect },
            { "WritingEffect", WritingEffect },
            { "MasonryEffect", MasonryEffect },
            { "MiningEffect", MiningEffect },
            { "AstrologyEffect", AstrologyEffect },
            { "ArcheryEffect", ArcheryEffect },
            { "BronzeWorkingEffect", BronzeWorkingEffect },
            { "WheelEffect", WheelEffect },
            { "CelestialNavigationEffect", CelestialNavigationEffect },
            { "CurrencyEffect", CurrencyEffect },
            { "HorsebackRidingEffect", HorsebackRidingEffect },
            { "IronWorkingEffect", IronWorkingEffect },
            { "ShipbuildingEffect", ShipbuildingEffect },
            { "MathematicsEffect", MathematicsEffect },
            { "EngineeringEffect", EngineeringEffect },
            { "TimbercraftEffect", TimbercraftEffect },
            { "ApprenticeshipEffect", ApprenticeshipEffect },
            { "MachineryEffect", MachineryEffect },
            { "AquaticQuarriesEffect", AquaticQuarriesEffect },
            { "EducationEffect", EducationEffect },
            { "StirrupsEffect", StirrupsEffect },
            { "MilitaryEngineeringEffect", MilitaryEngineeringEffect },
            { "CastlesEffect", CastlesEffect },
            { "CartographyEffect", CartographyEffect },
            { "CartographyEndEraEffect", CartographyEndEraEffect },
        };

    public static string ProcessFunctionString(String functionString, Player player)
    {
        if (effectFunctions.TryGetValue(functionString, out Func<Player, bool, string> effectFunction))
        {
            return effectFunction(player, true);
        }
        else
        {
            return $"Function '{functionString}' not recognized in ResearchEffects from Researches file.";
        }
    }

    public static string ProcessFunctionStringGetDescriptionOnly(String functionString, Player player)
    {
        if (effectFunctions.TryGetValue(functionString, out Func<Player, bool, string> effectFunction))
        {
            return effectFunction(player, false);
        }
        else
        {
            return $"Function '{functionString}' not recognized in ResearchEffects from Researches file.";
        }
    }
    static string AgricultureEffect(Player player, bool executeLogic)
    {
        return "";
    }
    static string SailingEffect(Player player, bool executeLogic)
    {
        if(executeLogic)
        {
            player.unitPlayerEffects.Add(new UnitPlayerEffect("Sailing", new UnitEffect("EnableEmbarkDisembark"), UnitClass.Civilian & UnitClass.Land));
        }
        return "Enable Emarking and Disembarking for all Civilian Units";
    }
    static string PotteryEffect(Player player, bool executeLogic)
    {
        return "";
    }
    static string AnimalHusbandryEffect(Player player, bool executeLogic)
    {
        //player.unitPlayerEffects.Add(("AnimalHusbandry", new UnitEffect(UnitEffectType.MovementSpeed, EffectOperation.Add, 1.0f, 5), UnitClass.Recon));
        return "";
    }
    static string IrrigationEffect(Player player, bool executeLogic)
    {
        return "";
    }
    static string WritingEffect(Player player, bool executeLogic)
    {
        return "";
    }
    static string MasonryEffect(Player player, bool executeLogic)
    {
        return "";
    }
    static string MiningEffect(Player player, bool executeLogic)
    {
        return "";
    }

    static string AstrologyEffect(Player player, bool executeLogic)
    {
        return "";
    }

    static string ArcheryEffect(Player player, bool executeLogic)
    {
        return "";
    }

    static string BronzeWorkingEffect(Player player, bool executeLogic)
    {
        return "";
    }

    static string WheelEffect(Player player, bool executeLogic)
    {
        return "";
    }

    static string CelestialNavigationEffect(Player player, bool executeLogic)
    {
        return "";
    }

    static string CurrencyEffect(Player player, bool executeLogic)
    {
        return "";
    }

    static string HorsebackRidingEffect(Player player, bool executeLogic)
    {
        return "";
    }

    static string IronWorkingEffect(Player player, bool executeLogic)
    {
        return "";
    }

    static string ShipbuildingEffect(Player player, bool executeLogic)
    {
        if (executeLogic)
        {
            player.unitPlayerEffects.Add(new UnitPlayerEffect("Sailing", new UnitEffect("EnableEmbarkDisembark"), UnitClass.Land));
        }
        return "Enable Emarking and Disembarking for all Land Units";
    }

    static string MathematicsEffect(Player player, bool executeLogic)
    {
        if (executeLogic)
        {
            player.unitPlayerEffects.Add(new UnitPlayerEffect("Mathematics", new UnitEffect(UnitEffectType.MovementSpeed, EffectOperation.Add, 1.0f, 5), UnitClass.Naval));
        }
        return "+1 Movement speed to all Naval units";
    }

    static string EngineeringEffect(Player player, bool executeLogic)
    {
        return "";
    }

    static string TimbercraftEffect(Player player, bool executeLogic)
    {
        return "";
    }

    static string ApprenticeshipEffect(Player player, bool executeLogic)
    {
        if (executeLogic)
        {
            player.buildingPlayerEffects.Add(new BuildingPlayerEffect("Apprenticeship", new BuildingEffect(BuildingEffectType.ProductionYield, EffectOperation.Add, 1.0f, 5), "Mine"));
        }
        return "+1 Production Yield to all Mines";
    }

    static string MachineryEffect(Player player, bool executeLogic)
    {
        return "";
    }

    static string EducationEffect(Player player, bool executeLogic)
    {
        return "";
    }

    
    static string AquaticQuarriesEffect(Player player, bool executeLogic)
    {
        if(executeLogic)
        {
            player.coralYields.production += 1;
        }
        return "";
    }

    static string StirrupsEffect(Player player, bool executeLogic)
    {
        if (executeLogic)
        {
            player.buildingPlayerEffects.Add(new BuildingPlayerEffect("Stirrups", new BuildingEffect(BuildingEffectType.FoodYield, EffectOperation.Add, 1.0f, 5), "Pasture"));
        }
        return "+1 Food Yield to all Pastures";
    }

    static string MilitaryEngineeringEffect(Player player, bool executeLogic)
    {
        return "";
    }

    static string CastlesEffect(Player player, bool executeLogic)
    {
        return "";
    }

    static string CartographyEffect(Player player, bool executeLogic)
    {
        if (executeLogic && player.industrialInsightCulturalResearchCount == 0)
        {
            player.unitPlayerEffects.Add(new UnitPlayerEffect("CartographyCivilian", new UnitEffect("EnableOceanMovement"), UnitClass.Civilian));
            player.unitPlayerEffects.Add(new UnitPlayerEffect("CartographyCombat", new UnitEffect("EnableOceanMovement"), UnitClass.Combat));
        }
        return "Grants the ability to move on Ocean tiles.";
    }
    static string CartographyEndEraEffect(Player player, bool executeLogic)
    {
        if(executeLogic)
        {
            player.industrialInsightCulturalResearchCount++;
            player.completedResearches.Remove("Cartography");
        }
        return "Upon completing this research if it is still the Classical Era you may research it again, each time you complete this research you will recieve a bonus.";
    }
}

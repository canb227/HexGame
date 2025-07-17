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
        researchesDict = ResearchLoader.LoadResearchData(xmlPath);
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
    static Dictionary<String, Func<Player, bool, string>> effectFunctions = new Dictionary<string, Func<Player, bool, string>>
    {
        { "TribalDominionEffect", TribalDominionEffect },
        { "CraftsmanshipEffect", CraftsmanshipEffect },
        { "PenmanshipExportEffect", PenmanshipExportEffect },
        { "PenmanshipShareMapEffect", PenmanshipShareMapEffect },
        { "MilitaryTraditionEffect", MilitaryTraditionEffect },
        { "StateWorkforceEffect", StateWorkforceEffect },
        { "EarlyEmpireEffect", EarlyEmpireEffect },
        { "MysticismEffect", MysticismEffect },
        { "ForeignTradeEffect", ForeignTradeEffect },
        { "RecreationEffect", RecreationEffect },
        { "PoliticalPhilosophyEffect", PoliticalPhilosophyEffect },
        { "DramaAndPoetryEffect", DramaAndPoetryEffect },
        { "MilitaryTrainingEffect", MilitaryTrainingEffect },
        { "DefensiveTacticsEffect", DefensiveTacticsEffect },
        { "RecordedHistoryEffect", RecordedHistoryEffect },
        { "MythologyEffect", MythologyEffect },
        { "NavalTraditionEffect", NavalTraditionEffect },
        { "FeudalismEffect", FeudalismEffect },
        { "CivilServiceEffect", CivilServiceEffect },
        { "MercenariesEffect", MercenariesEffect },
        { "MedievalFairesEffect", MedievalFairesEffect },
        { "GuildsEffect", GuildsEffect },
        { "DivineRightEffect", DivineRightEffect },
        { "IndustrialInsightEffect", IndustrialInsightEffect },
        { "IndustrialInsightEndEraEffect", IndustrialInsightEndEraEffect },
        { "FutureTechEffect", FutureTechEffect },

    };
    public static string ProcessFunctionString(String functionString, Player player)
    {        
        if (effectFunctions.TryGetValue(functionString, out Func<Player, bool, string> effectFunction))
        {
            return effectFunction(player, true);
        }
        else
        {
            return ($"Function '{functionString}' not recognized in CultureResearchEffects from CultureResearches file.");
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
            return $"Function '{functionString}' not recognized in CultureResearchEffects from CultureResearches file.";
        }
    }
    static string TribalDominionEffect(Player player, bool executeLogic)
    {
        return "";
    }
    static string CraftsmanshipEffect(Player player, bool executeLogic)
    {
        return "";
    }

    static string PenmanshipExportEffect(Player player, bool executeLogic)
    {
        if (executeLogic)
        {
            player.exportCap += 1;
        }
        return "+1 to Total Export Route Capacity.";
    }

    static string PenmanshipShareMapEffect(Player player, bool executeLogic)
    {
        if (executeLogic)
        {
            player.diplomaticActionHashSet.Add(new DiplomacyAction(player.teamNum, "Share Map", false, false));
        }
        return "Unlock ability to share your maps with others.";
    }

    static string MilitaryTraditionEffect(Player player, bool executeLogic)
    {
        return "";
    }

    static string StateWorkforceEffect(Player player, bool executeLogic)
    {
        return "";
    }

    static string EarlyEmpireEffect(Player player, bool executeLogic)
    {
        return "";
    }
    static string ForeignTradeEffect(Player player, bool executeLogic)
    {
        if(executeLogic)
        {
            player.tradeRouteCount += 2;
        }
        return "+2 Maximum Total Trade Routes";
    }
    static string MysticismEffect(Player player, bool executeLogic)
    {
        return "";
    }

    static string RecreationEffect(Player player, bool executeLogic)
    {
        return "";
    }

    static string PoliticalPhilosophyEffect(Player player, bool executeLogic)
    {
        return "";
    }

    static string DramaAndPoetryEffect(Player player, bool executeLogic)
    {
        return "";
    }

    static string MilitaryTrainingEffect(Player player, bool executeLogic)
    {
        return "";
    }

    static string DefensiveTacticsEffect(Player player, bool executeLogic)
    {
        return "";
    }

    static string RecordedHistoryEffect(Player player, bool executeLogic)
    {
        return "";
    }

    static string MythologyEffect(Player player, bool executeLogic)
    {
        return "";
    }

    static string NavalTraditionEffect(Player player, bool executeLogic)
    {
        return "";
    }

    static string FeudalismEffect(Player player, bool executeLogic)
    {
        if (executeLogic)
        {
            //TODO
        }
        return "Rural Districts Gain +1 Food Yield if they are Adjacent to Atleast 1 Other Rural District.";
    }

    static string CivilServiceEffect(Player player, bool executeLogic)
    {
        if(executeLogic)
        {
            player.diplomaticActionHashSet.Add(new DiplomacyAction(player.teamNum, "Make Alliance", false, false));
        }
        return "Unlocks making Alliances with Other Players.";
    }

    static string MercenariesEffect(Player player, bool executeLogic)
    {
        return "";
    }

    static string MedievalFairesEffect(Player player, bool executeLogic)
    {
        return "";
    }

    static string GuildsEffect(Player player, bool executeLogic)
    {
        return "";
    }

    static string DivineRightEffect(Player player, bool executeLogic)
    {
        return "";
    }

    static string IndustrialInsightEffect(Player player, bool executeLogic)
    {
        if(executeLogic)
        {

        }
        return "Reveals Buried Runes to be Explored by your Hero for Greater Rewards.";
    }
    static string IndustrialInsightEndEraEffect(Player player, bool executeLogic)
    {
        if(executeLogic)
        {
            player.industrialInsightCulturalResearchCount++;
            player.completedCultureResearches.Remove("IndustrialInsight");
        }
        return "Upon completing this research if it is still the Classical Era you may research it again, each time you complete this research you will recieve a bonus.";
    }
    static string FutureTechEffect(Player player, bool executeLogic)
    {
        return "";
    }
}

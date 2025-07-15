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
    public static void ProcessFunctionString(String functionString, Player player)
    {
        Dictionary<String, Action<Player>> effectFunctions = new Dictionary<string, Action<Player>>
        {
            { "TribalDominionEffect", TribalDominionEffect },
            { "CraftsmanshipEffect", CraftsmanshipEffect },
            { "InternalTradeEffect", InternalTradeEffect },
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
            { "FutureTechEffect", FutureTechEffect },

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
    static void CraftsmanshipEffect(Player player)
    {

    }

    static void InternalTradeEffect(Player player)
    {

    }

    static void MilitaryTraditionEffect(Player player)
    {

    }

    static void StateWorkforceEffect(Player player)
    {

    }

    static void EarlyEmpireEffect(Player player)
    {

    }
    static void ForeignTradeEffect(Player player)
    {

    }
    static void MysticismEffect(Player player)
    {

    }

    static void RecreationEffect(Player player)
    {

    }

    static void PoliticalPhilosophyEffect(Player player)
    {

    }

    static void DramaAndPoetryEffect(Player player)
    {

    }

    static void MilitaryTrainingEffect(Player player)
    {

    }

    static void DefensiveTacticsEffect(Player player)
    {

    }

    static void RecordedHistoryEffect(Player player)
    {

    }

    static void MythologyEffect(Player player)
    {

    }

    static void NavalTraditionEffect(Player player)
    {

    }

    static void FeudalismEffect(Player player)
    {

    }

    static void CivilServiceEffect(Player player)
    {

    }

    static void MercenariesEffect(Player player)
    {

    }

    static void MedievalFairesEffect(Player player)
    {

    }

    static void GuildsEffect(Player player)
    {

    }

    static void DivineRightEffect(Player player)
    {

    }

    static void FutureTechEffect(Player player)
    {

    }
}

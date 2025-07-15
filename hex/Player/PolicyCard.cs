using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class PolicyCardLoader
{
    public static Dictionary<int, PolicyCard> policyCardDictionary = new();
    public static Dictionary<string, int> policyCardXMLDictionary = new();

    private static Dictionary<string, Action<Player, bool, bool>> policyEffectActions;
    static int index = 0;
    static PolicyCardLoader()
    {
        policyEffectActions = new Dictionary<string, Action<Player, bool, bool>>
        {
            { "Discipline", DisciplineEffect },
            { "UrbanPlanning", UrbanPlanningEffect },
            { "Workership", WorkershipEffect },
            { "RallingDrums", RallingDrumsEffect },
            { "InternalRouting", InternalRoutingEffect },
            { "Maneuver", ManeuverEffect },
            { "WarStories", WarStoriesEffect },
            { "Servitude", ServitudeEffect },
            { "Conscription", ConscriptionEffect },
            { "Colonization", ColonizationEffect },
            { "Caravanners", CaravannersEffect },
            { "MaritimeIndustry", MaritimeIndustryEffect },
            { "Inspiration", InspirationEffect },
            { "Revelation", RevelationEffect },
            { "CharismaticLeader", CharismaticLeaderEffect },
            { "DiplomaticLeague", DiplomaticLeagueEffect },
            { "LiteraryTradition", LiteraryTraditionEffect },
            { "Veterancy", VeterancyEffect },
            { "EquestrianOrder", EquestrianOrderEffect },
            { "Bastions", BastionsEffect },
            { "Limestone", LimestoneEffect },
            { "NaturalPhilosophy", NaturalPhilosophyEffect },
            { "Praetorium", PraetoriumEffect },
            { "Myths", MythsEffect },
            { "NavalInfrastructure", NavalInfrastructureEffect },
            { "FeudalContract", FeudalContractEffect },
            { "TownCharters", TownChartersEffect },
            { "Chivalry", ChivalryEffect },
            { "GothicArchitecture", GothicArchitectureEffect },
        };


        policyCardXMLDictionary.Add("Sample", 0);
        List<(String, UnitEffect, UnitClass)> unitEffectList = new();
        List<(String, BuildingEffect, String)> buildingEffectList = new();

        policyCardDictionary.Add(0, new PolicyCard("Sample", "This is a sample policy card.", buildingEffectList, unitEffectList, true, false, false, false));

        //Code of Laws
        policyCardXMLDictionary.Add("Discipline", 1);
        unitEffectList.Clear();
        buildingEffectList.Clear();
        policyCardDictionary.Add(1, new PolicyCard("Discipline", "+5 Combat Power against Encampments and their Units.", buildingEffectList, unitEffectList, true, false, false, false));

        policyCardXMLDictionary.Add("UrbanPlanning", 2);
        unitEffectList.Clear();
        buildingEffectList.Clear();
        policyCardDictionary.Add(2, new PolicyCard("UrbanPlanning", "+1 Production in all cities.", buildingEffectList, unitEffectList, false, true, false, false));

        //Craftsmanship
        policyCardXMLDictionary.Add("Workership", 3);
        unitEffectList.Clear();
        buildingEffectList.Clear();
        policyCardDictionary.Add(3, new PolicyCard("Workership", "+1 Food and +1 Gold for each Refinement District", buildingEffectList, unitEffectList, false, true, false, false));

        policyCardXMLDictionary.Add("RallingDrums", 4);
        unitEffectList.Clear();
        buildingEffectList.Clear();
        policyCardDictionary.Add(4, new PolicyCard("RallingDrums", "+50% Production towards Classical Era Infantry and Ranged Land Units.", buildingEffectList, unitEffectList, true, false, false, false));

        //InternalTrade
        policyCardXMLDictionary.Add("InternalRouting", 38);
        unitEffectList.Clear();
        buildingEffectList.Clear();
        policyCardDictionary.Add(38, new PolicyCard("InternalRouting", "+1 Max Export Routes", buildingEffectList, unitEffectList, false, true, false, false));

        //Military Tradition
        policyCardXMLDictionary.Add("Maneuver", 7);
        unitEffectList.Clear();
        buildingEffectList.Clear();
        policyCardDictionary.Add(7, new PolicyCard("Maneuver", "+50% Production towards Classical Era Cavalry Units.", buildingEffectList, unitEffectList, true, false, false, false));

        policyCardXMLDictionary.Add("WarStories", 8);
        unitEffectList.Clear();
        buildingEffectList.Clear();
        policyCardDictionary.Add(8, new PolicyCard("WarStories", "+2 Military Experience Per Turn for Your Hero.", buildingEffectList, unitEffectList, false, false, false, true));

        //State Workforce
        policyCardXMLDictionary.Add("Servitude", 9);
        unitEffectList.Clear();
        buildingEffectList.Clear();
        policyCardDictionary.Add(9, new PolicyCard("Servitude", "+15% Production towards Classical Era World Wonders.", buildingEffectList, unitEffectList, false, true, false, false));

        policyCardXMLDictionary.Add("Conscription", 10);
        unitEffectList.Clear();
        buildingEffectList.Clear();
        policyCardDictionary.Add(10, new PolicyCard("Conscription", "Unit Maintenance Reduced by 1 Gold per Unit.", buildingEffectList, unitEffectList, true, false, false, false));

        //Early Empire
        policyCardXMLDictionary.Add("Colonization", 11);
        unitEffectList.Clear();
        buildingEffectList.Clear();
        policyCardDictionary.Add(11, new PolicyCard("Colonization", "+50% Production towards Settlers.", buildingEffectList, unitEffectList, false, true, false, false));
/*
        policyCardXMLDictionary.Add("", 12);
        policyCardDictionary.Add(12, new PolicyCard("", "", false, false, false, false));*/

        //Foreign Trade
        policyCardXMLDictionary.Add("Caravanners", 5);
        unitEffectList.Clear();
        buildingEffectList.Clear();

        policyCardDictionary.Add(5, new PolicyCard("Caravanners", "+2 Gold from all Trade Routes.", buildingEffectList, unitEffectList, false, true, false, false));

        policyCardXMLDictionary.Add("MaritimeIndustry", 6);
        unitEffectList.Clear();
        buildingEffectList.Clear();

        policyCardDictionary.Add(6, new PolicyCard("MaritimeIndustry", "+100% Production towards Classical Era Naval Units.", buildingEffectList, unitEffectList, true, false, false, false));

        //Mysticism
        policyCardXMLDictionary.Add("Inspiration", 13);
        unitEffectList.Clear();
        buildingEffectList.Clear();

        policyCardDictionary.Add(13, new PolicyCard("Inspiration", "+2 Scientific Experience Per Turn for Your Hero.", buildingEffectList, unitEffectList, false, false, false, true));

        policyCardXMLDictionary.Add("Revelation", 14);
        unitEffectList.Clear();
        buildingEffectList.Clear();

        policyCardDictionary.Add(14, new PolicyCard("Revelation", "+2 Economic Experience Per Turn for Your Hero.", buildingEffectList, unitEffectList, false, false, false, true));

        //Recreation

        //Political Philosophy
        policyCardXMLDictionary.Add("CharismaticLeader", 15);
        unitEffectList.Clear();
        buildingEffectList.Clear();

        policyCardDictionary.Add(15, new PolicyCard("CharismaticLeader", "+2 Diplomatic Experience Per Turn for Your Hero.", buildingEffectList, unitEffectList, false, false, true, false));

        policyCardXMLDictionary.Add("DiplomaticLeague", 16);
        unitEffectList.Clear();
        buildingEffectList.Clear();

        policyCardDictionary.Add(16, new PolicyCard("DiplomaticLeague", "+50% Cost for other Leaders to Incite Rebellion against you.", buildingEffectList, unitEffectList, false, false, true, false));

        //Drama and Poetry
        policyCardXMLDictionary.Add("LiteraryTradition", 17);
        unitEffectList.Clear();
        buildingEffectList.Clear();

        policyCardDictionary.Add(17, new PolicyCard("LiteraryTradition", "+2 Cultural Experience Per Turn for Your Hero.", buildingEffectList, unitEffectList, false, false, false, true));

        //Military Training
        policyCardXMLDictionary.Add("Veterancy", 18);
        unitEffectList.Clear();
        buildingEffectList.Clear();

        policyCardDictionary.Add(18, new PolicyCard("Veterancy", "+50% Production Towards Buildings in Military Districts", buildingEffectList, unitEffectList, true, false, false, false));

        policyCardXMLDictionary.Add("EquestrianOrder", 19);
        unitEffectList.Clear();
        buildingEffectList.Clear();

        policyCardDictionary.Add(19, new PolicyCard("EquestrianOrder", "+1 Production in all Urban Districts", buildingEffectList, unitEffectList, true, false, false, false));

        //Defensive Tactics
        policyCardXMLDictionary.Add("Bastions", 20);
        unitEffectList.Clear();
        buildingEffectList.Clear();

        policyCardDictionary.Add(20, new PolicyCard("Bastions", "+7 City Combat Strength and Walls Provide an Additional +10 Health.", buildingEffectList, unitEffectList, true, false, false, false));

        policyCardXMLDictionary.Add("Limestone", 21);
        unitEffectList.Clear();
        buildingEffectList.Clear();

        policyCardDictionary.Add(21, new PolicyCard("Limestone", "+100% Production Towards Defensive Structures.", buildingEffectList, unitEffectList, true, false, false, false));

        //Recorded History
        policyCardXMLDictionary.Add("NaturalPhilosophy", 22);
        unitEffectList.Clear();
        buildingEffectList.Clear();

        policyCardDictionary.Add(22, new PolicyCard("NaturalPhilosophy", "+25% Production Towards Buildings in Scientific Districts.", buildingEffectList, unitEffectList, false, true, false, false));

        policyCardXMLDictionary.Add("Praetorium", 23);
        unitEffectList.Clear();
        buildingEffectList.Clear();

        policyCardDictionary.Add(23, new PolicyCard("Praetorium", "+1 Influence per Turn for each Encampment you Occupy or are Allied With.", buildingEffectList, unitEffectList, false, false, true, false));

        //Mythology
        policyCardXMLDictionary.Add("Myths", 24);
        unitEffectList.Clear();
        buildingEffectList.Clear();
        policyCardDictionary.Add(24, new PolicyCard("Myths", "+25% Production Towards Buildings in Heroic Districts.", buildingEffectList, unitEffectList, false, true, false, false));

        //Naval Tradition
        policyCardXMLDictionary.Add("NavalInfrastructure", 25);
        unitEffectList.Clear();
        buildingEffectList.Clear();

        policyCardDictionary.Add(25, new PolicyCard("NavalInfrastructure", "+50% Production Towards Buildings in Harbor Districts.", buildingEffectList, unitEffectList, false, true, false, false));

        //Feudalism
        policyCardXMLDictionary.Add("FeudalContract", 26);
        unitEffectList.Clear();
        buildingEffectList.Clear();

        policyCardDictionary.Add(26, new PolicyCard("FeudalContract", "+0.5 Production per Population in Encampments you Occupy (Rounded Down)", buildingEffectList, unitEffectList, true, false, false, false));

        policyCardXMLDictionary.Add("Serfdom", 27);
        unitEffectList.Clear();
        buildingEffectList.Clear();

        policyCardDictionary.Add(27, new PolicyCard("Serfdom", "+1 Food and +1 Gold in All Rural Districts.", buildingEffectList, unitEffectList, false, true, false, false));

        //Civil Service
        policyCardXMLDictionary.Add("Meritocracy", 28);
        unitEffectList.Clear();
        buildingEffectList.Clear();

        policyCardDictionary.Add(28, new PolicyCard("Meritocracy", "+1 Culture for each Urban District a City Control.", buildingEffectList, unitEffectList, false, true, false, false));

        policyCardXMLDictionary.Add("CivilPrestige", 29);
        unitEffectList.Clear();
        buildingEffectList.Clear();

        policyCardDictionary.Add(29, new PolicyCard("CivilPrestige", ".", buildingEffectList, unitEffectList, false, true, false, false));

        //Mercenaries
        policyCardXMLDictionary.Add("Sack", 30);
        unitEffectList.Clear();
        buildingEffectList.Clear();

        policyCardDictionary.Add(30, new PolicyCard("Sack", "Yields from Pillaging are Doubled.", buildingEffectList, unitEffectList, true, false, false, false));

        policyCardXMLDictionary.Add("TradeConfederation", 31);
        unitEffectList.Clear();
        buildingEffectList.Clear();

        policyCardDictionary.Add(31, new PolicyCard("TradeConfederation", "+1 Culture and +1 Science for each Trade Route you send or receive.", buildingEffectList, unitEffectList, false, true, false, false));

        //Medieval Faires
        policyCardXMLDictionary.Add("Aesthetics", 32);
        unitEffectList.Clear();
        buildingEffectList.Clear();

        policyCardDictionary.Add(32, new PolicyCard("Aesthetics", "+50% Production Towards Buildings in Cultural Districts.", buildingEffectList, unitEffectList, false, true, false, false));

        policyCardXMLDictionary.Add("MerchantConfederation", 33);
        unitEffectList.Clear();
        buildingEffectList.Clear();

        policyCardDictionary.Add(33, new PolicyCard("MerchantConfederation", "+1 Culture and +1 Science for each Encampment you are Allied with.", buildingEffectList, unitEffectList, false, false, true, false));

        //Guilds
        policyCardXMLDictionary.Add("Craftsmen", 34);
        unitEffectList.Clear();
        buildingEffectList.Clear();

        policyCardDictionary.Add(34, new PolicyCard("Craftsmen", "+25% Production Towards Buildings in Production Districts.", buildingEffectList, unitEffectList, true, false, false, false));

        policyCardXMLDictionary.Add("TownCharters", 35);
        unitEffectList.Clear();
        buildingEffectList.Clear();

        policyCardDictionary.Add(35, new PolicyCard("TownCharters", "+50% Production Towards Buildings in Economic Districts.", buildingEffectList, unitEffectList, false, true, false, false));

        //Divine Right
        policyCardXMLDictionary.Add("Chivalry", 36);
        unitEffectList.Clear();
        buildingEffectList.Clear();

        policyCardDictionary.Add(36, new PolicyCard("Chivalry", "+50% Production Towards Industrial and Earlier Cavalry Units.", buildingEffectList, unitEffectList, true, false, false, false));

        policyCardXMLDictionary.Add("GothicArchitecture", 37);
        unitEffectList.Clear();
        buildingEffectList.Clear();

        policyCardDictionary.Add(37, new PolicyCard("GothicArchitecture", "+20% Production Towards Industrial and Earlier World Wonders.", buildingEffectList, unitEffectList, false, true, false, false));

    }

    private static void DisciplineEffect(Player player, bool add, bool remove) { }
    private static void UrbanPlanningEffect(Player player, bool add, bool remove) { }
    private static void WorkershipEffect(Player player, bool add, bool remove) { }
    private static void RallingDrumsEffect(Player player, bool add, bool remove) { }
    private static void InternalRoutingEffect(Player player, bool add, bool remove) { }
    private static void ManeuverEffect(Player player, bool add, bool remove) { }
    private static void WarStoriesEffect(Player player, bool add, bool remove) { }
    private static void ServitudeEffect(Player player, bool add, bool remove) { }
    private static void ConscriptionEffect(Player player, bool add, bool remove) { }
    private static void ColonizationEffect(Player player, bool add, bool remove) { }
    private static void CaravannersEffect(Player player, bool add, bool remove) { }
    private static void MaritimeIndustryEffect(Player player, bool add, bool remove) { }
    private static void InspirationEffect(Player player, bool add, bool remove) { }
    private static void RevelationEffect(Player player, bool add, bool remove) { }
    private static void CharismaticLeaderEffect(Player player, bool add, bool remove) { }
    private static void DiplomaticLeagueEffect(Player player, bool add, bool remove) { }
    private static void LiteraryTraditionEffect(Player player, bool add, bool remove) { }
    private static void VeterancyEffect(Player player, bool add, bool remove) { }
    private static void EquestrianOrderEffect(Player player, bool add, bool remove) { }
    private static void BastionsEffect(Player player, bool add, bool remove) { }
    private static void LimestoneEffect(Player player, bool add, bool remove) { }
    private static void NaturalPhilosophyEffect(Player player, bool add, bool remove) { }
    private static void PraetoriumEffect(Player player, bool add, bool remove) { }
    private static void MythsEffect(Player player, bool add, bool remove) { }
    private static void NavalInfrastructureEffect(Player player, bool add, bool remove) { }
    private static void FeudalContractEffect(Player player, bool add, bool remove) { }
    private static void TownChartersEffect(Player player, bool add, bool remove) { }
    private static void ChivalryEffect(Player player, bool add, bool remove) { }
    private static void GothicArchitectureEffect(Player player, bool add, bool remove) { }





    public static PolicyCard GetPolicyCard(string name)
    {
        return policyCardDictionary[policyCardXMLDictionary[name]];
    }

    public static PolicyCard GetPolicyCard(int id)
    {
        return policyCardDictionary[id];
    }

    static int NextPolicyCardIndex()
    {
        return index++;
    }
}

public class PolicyCard
{
    public bool isMilitary { get; set; }
    public bool isEconomic { get; set; }
    public bool isDiplomatic { get; set; }
    public bool isHeroic { get; set; }
    public string title { get; set; }
    public string description { get; set; }
    public List<(String, BuildingEffect, String)> buildingEffects { get; set; }
    public List<(String, UnitEffect, UnitClass)> unitEffects { get; set; }


    public PolicyCard(string title, string description, List<(String, BuildingEffect, String)> buildingEffects, List<(String, UnitEffect, UnitClass)> unitEffects, bool isMilitary = false, bool isEconomic = false, bool isDiplomatic=false, bool isHeroic = false)
    {
        this.title = title;
        this.description = description;
        this.isMilitary = isMilitary;
        this.isEconomic = isEconomic;
        this.isDiplomatic = isDiplomatic;
        this.isHeroic = isHeroic;

        this.buildingEffects = new();
        this.buildingEffects.Concat(buildingEffects).ToList();

        this.unitEffects = new();
        this.unitEffects.Concat(unitEffects).ToList();
    }
    public PolicyCard()
    {

    }

    public override bool Equals(object obj)
    {
        if (obj is PolicyCard)
        {
            if (((PolicyCard)obj).title == title)
            {
                return true;
            }
        }
        return false;
    }

    public void AddEffect(int teamNum)
    {
        Player player = Global.gameManager.game.playerDictionary[teamNum];
        foreach(UnitEffect unitEffect in unitEffects)
        {
            player.unitPlayerEffects.Add((title, unitEffect, UnitClass.None));
        }
        foreach (BuildingEffect buildingEffect in buildingEffects)
        {
            player.buildingPlayerEffects.Add(("", buildingEffect, ""));
        }
        

        foreach (int unitID in player.unitList)
        {
            Global.gameManager.game.unitDictionary[unitID].RecalculateEffects();
        }
        foreach (int cityID in player.cityList)
        {
            Global.gameManager.game.cityDictionary[cityID].RecalculateYields();
        }
    }
    public void CalculateEffects()
    {

    }

    public bool SameType(PolicyCard other)
    {
        if(isMilitary && other.isMilitary)
        {
            return true;
        }
        if(isEconomic && other.isEconomic)
        {
            return true;
        }
        if(isDiplomatic && other.isDiplomatic)
        {
            return true;
        }
        if(isHeroic && other.isHeroic)
        {
            return true;
        }
        return false;
    }
}
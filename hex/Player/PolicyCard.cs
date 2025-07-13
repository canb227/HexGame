using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class PolicyCardLoader
{
    private static Dictionary<int, PolicyCard> policyCardDictionary = new();
    private static Dictionary<string, int> policyCardXMLDictionary = new();
    static int index = 0;
    static PolicyCardLoader()
    {
        policyCardXMLDictionary.Add("Sample", 0);
        policyCardDictionary.Add(0, new PolicyCard("Sample", "This is a sample policy card.", true, false, false, false));

        //Code of Laws
        policyCardXMLDictionary.Add("Discipline", 1);
        policyCardDictionary.Add(1, new PolicyCard("Discipline", "+5 Combat Power against Encampments and their Units.", true, false, false, false));

        policyCardXMLDictionary.Add("UrbanPlanning", 2);
        policyCardDictionary.Add(2, new PolicyCard("UrbanPlanning", "+1 Production in all cities.", false, true, false, false));

        //Craftsmanship
        policyCardXMLDictionary.Add("Workership", 3);
        policyCardDictionary.Add(3, new PolicyCard("Workership", "", false, true, false, false));

        policyCardXMLDictionary.Add("RallingDrums", 4);
        policyCardDictionary.Add(4, new PolicyCard("RallingDrums", "+50% Production towards Classical Era Infantry and Ranged Land Units.", true, false, false, false));

        //InternalTrade
        policyCardXMLDictionary.Add("InternalRouting", 38);
        policyCardDictionary.Add(38, new PolicyCard("InternalRouting", "+1 Max Export Routes", false, true, false, false));

        //Military Tradition
        policyCardXMLDictionary.Add("Maneuver", 7);
        policyCardDictionary.Add(7, new PolicyCard("Maneuver", "+50% Production towards Classical Era Cavalry Units.", true, false, false, false));

        policyCardXMLDictionary.Add("WarStories", 8);
        policyCardDictionary.Add(8, new PolicyCard("WarStories", "+2 Military Experience Per Turn for Your Hero.", false, false, false, true));

        //State Workforce
        policyCardXMLDictionary.Add("Servitude", 9);
        policyCardDictionary.Add(9, new PolicyCard("Servitude", "+15% Production towards Classical Era World Wonders.", false, true, false, false));

        policyCardXMLDictionary.Add("Conscription", 10);
        policyCardDictionary.Add(10, new PolicyCard("Conscription", "Unit Maintenance Reduced by 1 Gold per Unit.", true, false, false, false));

        //Early Empire
        policyCardXMLDictionary.Add("Colonization", 11);
        policyCardDictionary.Add(11, new PolicyCard("Colonization", "+50% Production towards Settlers.", false, true, false, false));

        policyCardXMLDictionary.Add("", 12);
        policyCardDictionary.Add(12, new PolicyCard("", "", false, false, false, false));

        //Foreign Trade
        policyCardXMLDictionary.Add("Caravanners", 5);
        policyCardDictionary.Add(5, new PolicyCard("Caravanners", "+2 Gold from all Trade Routes.", false, true, false, false));

        policyCardXMLDictionary.Add("MaritimeIndustry", 6);
        policyCardDictionary.Add(6, new PolicyCard("MaritimeIndustry", "+100% Production towards Classical Era Naval Units.", true, false, false, false));

        //Mysticism
        policyCardXMLDictionary.Add("Inspiration", 13);
        policyCardDictionary.Add(13, new PolicyCard("Inspiration", "+2 Scientific Experience Per Turn for Your Hero.", false, false, false, true));

        policyCardXMLDictionary.Add("Revelation", 14);
        policyCardDictionary.Add(14, new PolicyCard("Revelation", "+2 Economic Experience Per Turn for Your Hero.", false, false, false, true));

        //Recreation

        //Political Philosophy
        policyCardXMLDictionary.Add("CharismaticLeader", 15);
        policyCardDictionary.Add(15, new PolicyCard("CharismaticLeader", "+2 Diplomatic Experience Per Turn for Your Hero.", false, false, true, false));

        policyCardXMLDictionary.Add("DiplomaticLeague", 16);
        policyCardDictionary.Add(16, new PolicyCard("DiplomaticLeague", "+50% Cost for other Leaders to Incite Rebellion against you.", false, false, true, false));

        //Drama and Poetry
        policyCardXMLDictionary.Add("LiteraryTradition", 17);
        policyCardDictionary.Add(17, new PolicyCard("LiteraryTradition", "+2 Cultural Experience Per Turn for Your Hero.", false, false, false, true));

        //Military Training
        policyCardXMLDictionary.Add("Veterancy", 18);
        policyCardDictionary.Add(18, new PolicyCard("Veterancy", "+50% Production Towards Buildings in Military Districts", true, false, false, false));

        policyCardXMLDictionary.Add("EquestrianOrder", 19);
        policyCardDictionary.Add(19, new PolicyCard("EquestrianOrder", "", true, false, false, false));

        //Defensive Tactics
        policyCardXMLDictionary.Add("Bastions", 20);
        policyCardDictionary.Add(20, new PolicyCard("Bastions", "+7 City Combat Strength and Walls Provide an Additional +10 Health.", true, false, false, false));

        policyCardXMLDictionary.Add("Limestone", 21);
        policyCardDictionary.Add(21, new PolicyCard("Limestone", "+100% Production Towards Defensive Structures.", true, false, false, false));

        //Recorded History
        policyCardXMLDictionary.Add("NaturalPhilosophy", 22);
        policyCardDictionary.Add(22, new PolicyCard("NaturalPhilosophy", "+25% Production Towards Buildings in Scientific Districts.", false, true, false, false));

        policyCardXMLDictionary.Add("Praetorium", 23);
        policyCardDictionary.Add(23, new PolicyCard("Praetorium", "+1 Influence per Turn for each Encampment you Occupy or are Allied With.", false, false, true, false));

        //Mythology
        policyCardXMLDictionary.Add("Myths", 24);
        policyCardDictionary.Add(24, new PolicyCard("Myths", "+25% Production Towards Buildings in Heroic Districts.", false, true, false, false));

        //Naval Tradition
        policyCardXMLDictionary.Add("NavalInfrastructure", 25);
        policyCardDictionary.Add(25, new PolicyCard("NavalInfrastructure", "+50% Production Towards Buildings in Harbor Districts.", false, true, false, false));

        //Feudalism
        policyCardXMLDictionary.Add("FeudalContract", 26);
        policyCardDictionary.Add(26, new PolicyCard("FeudalContract", "+0.5 Production per Population in Encampments you Occupy (Rounded Down)", true, false, false, false));

        policyCardXMLDictionary.Add("Serfdom", 27);
        policyCardDictionary.Add(27, new PolicyCard("Serfdom", "+1 Food and +1 Gold in All Rural Districts.", false, true, false, false));

        //Civil Service
        policyCardXMLDictionary.Add("Meritocracy", 28);
        policyCardDictionary.Add(28, new PolicyCard("Meritocracy", "+1 Culture for each Urban District a City Control.", false, true, false, false));

        policyCardXMLDictionary.Add("CivilPrestige", 29);
        policyCardDictionary.Add(29, new PolicyCard("CivilPrestige", ".", false, true, false, false));

        //Mercenaries
        policyCardXMLDictionary.Add("Sack", 30);
        policyCardDictionary.Add(30, new PolicyCard("Sack", "Yields from Pillaging are Doubled.", true, false, false, false));

        policyCardXMLDictionary.Add("TradeConfederation", 31);
        policyCardDictionary.Add(31, new PolicyCard("TradeConfederation", "+1 Culture and +1 Science for each Trade Route you send or receive.", false, true, false, false));

        //Medieval Faires
        policyCardXMLDictionary.Add("Aesthetics", 32);
        policyCardDictionary.Add(32, new PolicyCard("Aesthetics", "+50% Production Towards Buildings in Cultural Districts.", false, true, false, false));

        policyCardXMLDictionary.Add("MerchantConfederation", 33);
        policyCardDictionary.Add(33, new PolicyCard("MerchantConfederation", "+1 Culture and +1 Science for each Encampment you are Allied with.", false, false, true, false));

        //Guilds
        policyCardXMLDictionary.Add("Craftsmen", 34);
        policyCardDictionary.Add(34, new PolicyCard("Craftsmen", "+25% Production Towards Buildings in Production Districts.", true, false, false, false));

        policyCardXMLDictionary.Add("TownCharters", 35);
        policyCardDictionary.Add(35, new PolicyCard("TownCharters", "+50% Production Towards Buildings in Economic Districts.", false, true, false, false));

        //Divine Right
        policyCardXMLDictionary.Add("Chivalry", 36);
        policyCardDictionary.Add(36, new PolicyCard("Chivalry", "+50% Production Towards Industrial and Earlier Cavalry Units.", true, false, false, false));

        policyCardXMLDictionary.Add("GothicArchitecture", 37);
        policyCardDictionary.Add(37, new PolicyCard("GothicArchitecture", "+20% Production Towards Industrial and Earlier World Wonders.", false, true, false, false));

    }

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
    public int staticID { get; set; }
    public bool isMilitary { get; set; }
    public bool isEconomic { get; set; }
    public bool isDiplomatic { get; set; }
    public bool isHeroic { get; set; }
    public string title { get; set; }
    public string description { get; set; }

    public PolicyCard(string title = "", string description = "", bool isMilitary = false, bool isEconomic = false, bool isDiplomatic=false, bool isHeroic = false)
    {
        this.title = title;
        this.description = description;
        this.isMilitary = isMilitary;
        this.isEconomic = isEconomic;
        this.isDiplomatic = isDiplomatic;
        this.isHeroic = isHeroic;
    }
    public PolicyCard()
    {

    }

    public override bool Equals(object obj)
    {
        if (obj is PolicyCard)
        {
            if (((PolicyCard)obj).staticID == staticID)
            {
                return true;
            }
        }
        return false;
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
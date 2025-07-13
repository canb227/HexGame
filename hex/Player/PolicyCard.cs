using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class PolicyCardLoader
{
    public static Dictionary<int, PolicyCard> policyCardDictionary = new();
    public static Dictionary<string, int> policyCardXMLDictionary = new();
    static int index = 0;
    static PolicyCardLoader()
    {
        policyCardXMLDictionary.Add("Sample", 0);
        policyCardDictionary.Add(0, new PolicyCard("Sample", "This is a sample policy card", true, false, false, false));
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
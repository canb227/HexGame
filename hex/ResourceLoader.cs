using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

public enum ResourceType
{
    None = '0',
    Iron = 'I',
    Horses = 'H',
    Niter = 'N',
    Coal = 'C',
    Oil = 'O',
    Uranium = 'U',
    Lithium = 'L',
    SciFi = 'S',
    Jade = 'j',
    Wheat = 'w',
    Sheep = 's',
    Marble = 'm',
    Dates = 'd',
    Silk = 'u',
    Salt = 'n',
    Rubber = 'R',
    Ivory = 'i',
    Gold = 'g',
    Silver = 'k',
    Camels = 'c',
    Coffee = 'e',
    Cotton = 'q',
    Tobacco = 't',
    Stone = 'z',
}

public struct ResourceInfo
{
    public string Name { get; set; }
    public string IconPath { get; set; }
    public string ModelPath { get; set; }
    public string ImprovementType { get; set; }
    public bool IsGlobal { get; set; }
    public int Food { get; set; }
    public int Production { get; set; }
    public int Gold { get; set; }
    public int Science { get; set; }
    public int Culture { get; set; }
    public int Happiness { get; set; }
}

public static class ResourceLoader
{
    public static Dictionary<ResourceType, ResourceInfo> resources;
    public static Dictionary<ResourceType, string> resourceEffects;
    public static Dictionary<String, ResourceType> resourceNames = new Dictionary<String, ResourceType>
    {
        { "0", ResourceType.None},
        { "I", ResourceType.Iron },
        { "H", ResourceType.Horses },
        { "N", ResourceType.Niter },
        { "C", ResourceType.Coal },
        { "O", ResourceType.Oil },
        { "U", ResourceType.Uranium },
        { "L", ResourceType.Lithium },
        { "S", ResourceType.SciFi },
        { "j", ResourceType.Jade },
        { "w", ResourceType.Wheat },
        { "s", ResourceType.Sheep },
        { "m", ResourceType.Marble },
        { "d", ResourceType.Dates },
        { "u", ResourceType.Silk },
        { "n", ResourceType.Salt },
        { "R", ResourceType.Rubber },
        { "i", ResourceType.Ivory },
        { "g", ResourceType.Gold },
        { "k", ResourceType.Silver },
        { "c", ResourceType.Camels },
        { "e", ResourceType.Coffee },
        { "q", ResourceType.Cotton },
        { "t", ResourceType.Tobacco },
        { "z", ResourceType.Stone }
    };
    
    static ResourceLoader()
    { //Iron, Horses, Niter, Coal, Oil, Uranium, Lithium, Jade, Silk, Tobacco, Silver, Gold, Camels
        resourceEffects = new Dictionary<ResourceType, string>
        {
            { ResourceType.Iron, "ApplyIronEffect" },
            { ResourceType.Horses, "ApplyHorsesEffect" },
            { ResourceType.Niter, "ApplyNiterEffect" },
            { ResourceType.Coal, "ApplyCoalEffect" },
            { ResourceType.Oil, "ApplyOilEffect" },
            { ResourceType.Uranium, "ApplyUraniumEffect" },
            { ResourceType.Lithium, "ApplyLithiumEffect" },
            { ResourceType.SciFi, "ApplySciFiEffect" },
            { ResourceType.Jade, "ApplyJadeEffect" },
            { ResourceType.Wheat, "ApplyWheatEffect" },
            { ResourceType.Sheep, "ApplySheepEffect" },
            { ResourceType.Marble, "ApplyMarbleEffect" },
            { ResourceType.Dates, "ApplyDatesEffect" },
            { ResourceType.Silk, "ApplySilkEffect" },
            { ResourceType.Salt, "ApplySaltEffect" },
            { ResourceType.Rubber, "ApplyRubberEffect" },
            { ResourceType.Ivory, "ApplyIvoryEffect" },
            { ResourceType.Gold, "ApplyGoldEffect" },
            { ResourceType.Silver, "ApplySilverEffect" },
            { ResourceType.Camels, "ApplyCamelsEffect" },
            { ResourceType.Coffee, "ApplyCoffeeEffect" },
            { ResourceType.Cotton, "ApplyCottonEffect" },
            { ResourceType.Tobacco, "ApplyTobaccoEffect" },
            { ResourceType.Stone, "ApplyStoneEffect" }
        };


        string xmlPath = "hex/Resources.xml";
        resources = LoadResourceData(xmlPath);
        //if (resources.TryGetValue(resource, out ResourceInfo info))
        //ExecuteResourceEffect(resource);
    }
    public static Dictionary<ResourceType, ResourceInfo> LoadResourceData(string xmlPath)
    {
        // Load the XML file
        XDocument xmlDoc = XDocument.Load(xmlPath);

        // Parse the resource information into a dictionary
        var resourceData = xmlDoc.Descendants("Resource")
            .ToDictionary(
                r => (ResourceType)Enum.Parse(typeof(ResourceType), r.Attribute("Name").Value),
                r => new ResourceInfo
                {
                    Name = r.Attribute("Name").Value,
                    IconPath = r.Attribute("IconPath").Value,
                    ModelPath = r.Attribute("ModelPath").Value,
                    ImprovementType = r.Attribute("ImprovementType").Value,
                    IsGlobal = bool.Parse(r.Attribute("IsGlobal").Value),
                    Food = int.Parse(r.Attribute("Food").Value),
                    Production = int.Parse(r.Attribute("Production").Value),
                    Gold = int.Parse(r.Attribute("Gold").Value),
                    Science = int.Parse(r.Attribute("Science").Value),
                    Culture = int.Parse(r.Attribute("Culture").Value),
                    Happiness = int.Parse(r.Attribute("Happiness").Value)
                }
            );

        return resourceData;
    }
    public static bool ProcessFunctionString(String functionString, int cityID)
    {
        if (functionString.Equals("ApplyIronEffect"))
        {
            ApplyIronEffect(Global.gameManager.game.cityDictionary[cityID]);
        }
        else if (functionString.Equals("ApplyHorsesEffect"))
        {
            ApplyHorsesEffect(Global.gameManager.game.cityDictionary[cityID]);
        }
        else if (functionString.Equals("ApplyNiterEffect"))
        {
            ApplyNiterEffect(Global.gameManager.game.cityDictionary[cityID]);
        }
        else if (functionString.Equals("ApplyCoalEffect"))
        {
            ApplyCoalEffect(Global.gameManager.game.cityDictionary[cityID]);
        }
        else if (functionString.Equals("ApplyOilEffect"))
        {
            ApplyOilEffect(Global.gameManager.game.cityDictionary[cityID]);
        }
        else if (functionString.Equals("ApplyUraniumEffect"))
        {
            ApplyUraniumEffect(Global.gameManager.game.cityDictionary[cityID]);
        }
        else if (functionString.Equals("ApplyLithiumEffect"))
        {
            ApplyLithiumEffect(Global.gameManager.game.cityDictionary[cityID]);
        }
        else if (functionString.Equals("ApplySciFiEffect"))
        {
            ApplySciFiEffect(Global.gameManager.game.cityDictionary[cityID]);
        }
        else if (functionString.Equals("ApplyJadeEffect"))
        {
            ApplyJadeEffect(Global.gameManager.game.cityDictionary[cityID]);
        }
        else if (functionString.Equals("ApplyWheatEffect"))
        {
            ApplyWheatEffect(Global.gameManager.game.cityDictionary[cityID]);
        }
        else if (functionString.Equals("ApplySheepEffect"))
        {
            ApplySheepEffect(Global.gameManager.game.cityDictionary[cityID]);
        }
        else if (functionString.Equals("ApplyMarbleEffect"))
        {
            ApplyMarbleEffect(Global.gameManager.game.cityDictionary[cityID]);
        }
        else if (functionString.Equals("ApplyDatesEffect"))
        {
            ApplyDatesEffect(Global.gameManager.game.cityDictionary[cityID]);
        }
        else if (functionString.Equals("ApplySilkEffect"))
        {
            ApplySilkEffect(Global.gameManager.game.cityDictionary[cityID]);
        }
        else if (functionString.Equals("ApplySaltEffect"))
        {
            ApplySaltEffect(Global.gameManager.game.cityDictionary[cityID]);
        }
        else if (functionString.Equals("ApplyRubberEffect"))
        {
            ApplyRubberEffect(Global.gameManager.game.cityDictionary[cityID]);
        }
        else if (functionString.Equals("ApplyIvoryEffect"))
        {
            ApplyIvoryEffect(Global.gameManager.game.cityDictionary[cityID]);
        }
        else if (functionString.Equals("ApplyGoldEffect"))
        {
            ApplyGoldEffect(Global.gameManager.game.cityDictionary[cityID]);
        }
        else if (functionString.Equals("ApplySilverEffect"))
        {
            ApplySilverEffect(Global.gameManager.game.cityDictionary[cityID]);
        }
        else if (functionString.Equals("ApplyCamelsEffect"))
        {
            ApplyCamelsEffect(Global.gameManager.game.cityDictionary[cityID]);
        }
        else if (functionString.Equals("ApplyCoffeeEffect"))
        {
            ApplyCoffeeEffect(Global.gameManager.game.cityDictionary[cityID]);
        }
        else if (functionString.Equals("ApplyCottonEffect"))
        {
            ApplyCottonEffect(Global.gameManager.game.cityDictionary[cityID]);
        }
        else if (functionString.Equals("ApplyTobaccoEffect"))
        {
            ApplyTobaccoEffect(Global.gameManager.game.cityDictionary[cityID]);
        }
        else if (functionString.Equals("ApplyStoneEffect"))
        {
            ApplyStoneEffect(Global.gameManager.game.cityDictionary[cityID]);
        }
        else
        {
            throw new NotImplementedException("The Effect Function: " + functionString + " does not exist, implement it in ResourceLoader");
        }
        return true;
    }

    // No effect
    static void ApplyIronEffect(City city)
    {
    }

    // All mounted units (horse, siege) +1 combat strength
    static void ApplyHorsesEffect(City city)
    {
    }

    // Effect TBD
    static void ApplyNiterEffect(City city)
    {
    }

    // Effect TBD
    static void ApplyCoalEffect(City city)
    {
    }

    // Effect TBD
    static void ApplyOilEffect(City city)
    {
    }

    // Effect TBD
    static void ApplyUraniumEffect(City city)
    {
    }

    // Effect TBD
    static void ApplyLithiumEffect(City city)
    {
    }

    // 15% more gold in city
    static void ApplyJadeEffect(City city)
    {
    }

    // +2 food
    static void ApplyWheatEffect(City city)
    {
        city.yields.food += 2;
    }

    // 10% more culture in city
    static void ApplySilkEffect(City city)
    {
    }

    // 10% more science in city
    static void ApplyCoffeeEffect(City city)
    {
    }

    // 15% off units purchased with gold
    static void ApplySilverEffect(City city)
    {
    }

    // 15% off buildings purchased with gold
    static void ApplyGoldEffect(City city)
    {
    }

    // Allow 3 more resources to be assigned to this city
    static void ApplyCamelsEffect(City city)
    {
    }

    // Effect TBD
    static void ApplySciFiEffect(City city)
    {
    }

    // Effect TBD
    static void ApplySheepEffect(City city)
    {
    }

    // Effect TBD
    static void ApplyMarbleEffect(City city)
    {
    }

    // Effect TBD
    static void ApplyDatesEffect(City city)
    {
    }

    // Effect TBD
    static void ApplySaltEffect(City city)
    {
    }

    // Effect TBD
    static void ApplyRubberEffect(City city)
    {
    }

    // Effect TBD
    static void ApplyIvoryEffect(City city)
    {
    }

    // Effect TBD
    static void ApplyCottonEffect(City city)
    {
    }

    // Effect TBD
    static void ApplyTobaccoEffect(City city)
    {
    }

    // Effect TBD
    static void ApplyStoneEffect(City city)
    {
    }

}

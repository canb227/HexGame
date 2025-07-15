using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Godot.GodotThread;

public static class PlayerEffect
{
    public static void ProcessFunctionString(String functionString, BasePlayer player)
    {
        Dictionary<String, Action<BasePlayer>> effectFunctions = new Dictionary<string, Action<BasePlayer>>
        {
            { "AddTribalGovernmentEffect", AddTribalGovernmentEffect },
        };

        if (effectFunctions.TryGetValue(functionString, out Action<BasePlayer> effectFunction))
        {
            effectFunction(player);
        }
        else
        {
            throw new ArgumentException($"Function '{functionString}' not recognized in PlayerEffect");
        }
    }

    static void RemovePlayerBuildingEffects(string name, BasePlayer player)
    {
        List<(string, BuildingEffect, string)> toRemove = new();
        foreach ((string, BuildingEffect, string) effect in player.buildingPlayerEffects)
        {
            if (effect.Item1 == "TribalGovernment")
            {
                toRemove.Add(effect);
            }
        }
        foreach (var effect in toRemove)
        {
            player.buildingPlayerEffects.Remove(effect);
        }
    }
    static void RemovePlayerUnitEffects(string name, BasePlayer player)
    {
        List<(string, UnitEffect, UnitClass)> toRemoveUnit = new();
        foreach ((string, UnitEffect, UnitClass) effect in player.unitPlayerEffects)
        {
            if (effect.Item1 == "TribalGovernment")
            {
                toRemoveUnit.Add(effect);
            }
        }
        foreach (var effect in toRemoveUnit)
        {
            player.unitPlayerEffects.Remove(effect);
        }
    }

    public static void SetGovernment(BasePlayer player, GovernmentType governmentType)
    {
        //if they had a government remove its effects
        if (player.government != null)
        {
            if (player.government == GovernmentType.Tribal)
            {
                PlayerEffect.RemoveTribalGovernmentEffect(player);
            }
            else if (player.government == GovernmentType.Autocracy)
            {
                PlayerEffect.RemoveAutocracyGovernmentEffect(player);
            }
            else if (player.government == GovernmentType.ClassicalRepublic)
            {
                PlayerEffect.RemoveClassicalRepublicGovernmentEffect(player);
            }
            else if (player.government == GovernmentType.Oligarchy)
            {
                PlayerEffect.RemoveOligarchyGovernmentEffect(player);
            }
            else if (player.government == GovernmentType.Monarchy)
            {
                PlayerEffect.RemoveMonarchyGovernmentEffect(player);
            }
        }

        //set the new government
        player.government = governmentType;
        
        //add effects of the new government
        if (player.government == GovernmentType.Tribal)
        {
            PlayerEffect.AddTribalGovernmentEffect(player);
        }
        else if(player.government == GovernmentType.Autocracy)
        {
            PlayerEffect.AddAutocracyGovernmentEffect(player);
        }
        else if (player.government == GovernmentType.ClassicalRepublic)
        {
            PlayerEffect.AddClassicalRepublicGovernmentEffect(player);
        }
        else if (player.government == GovernmentType.Oligarchy)
        {
            PlayerEffect.AddOligarchyGovernmentEffect(player);
        }
        else if (player.government == GovernmentType.Monarchy)
        {
            PlayerEffect.AddMonarchyGovernmentEffect(player);
        }
    }


    //tribal government
    static void AddTribalGovernmentEffect(BasePlayer player)
    {
        player.militaryPolicySlots += 1;
        player.economicPolicySlots += 1;
        player.buildingPlayerEffects.Add(("TribalGovernment", new BuildingEffect(BuildingEffectType.ProductionCost, EffectOperation.Multiply, 0.90f, 0), "")); //buildings are 10% cheaper (DOESNT WORK CURRENTLY
        player.unitPlayerEffects.Add(("TribalGovernment", new UnitEffect(UnitEffectType.CombatStrength, EffectOperation.Add, 2, 0), UnitClass.Land)); //increase land strength by +2
        foreach (int unitID in player.unitList)
        {
            Global.gameManager.game.unitDictionary[unitID].RecalculateEffects();
        }
        foreach (int cityID in player.cityList)
        {
            Global.gameManager.game.cityDictionary[cityID].RecalculateYields();
        }
    }
    static void RemoveTribalGovernmentEffect(BasePlayer player)
    {
        player.militaryPolicySlots -= 1;
        player.economicPolicySlots -= 1;
        RemovePlayerBuildingEffects("TribalGovernment", player);
        RemovePlayerUnitEffects("TribalGovernment", player);
        foreach (int unitID in player.unitList)
        {
            Global.gameManager.game.unitDictionary[unitID].RecalculateEffects();
        }
        foreach (int cityID in player.cityList)
        {
            Global.gameManager.game.cityDictionary[cityID].RecalculateYields();
        }
    }

    //Autocracy
    static void AddAutocracyGovernmentEffect(BasePlayer player)
    {
        player.militaryPolicySlots += 2;
        player.economicPolicySlots += 1;
        player.heroicPolicySlots += 1;
        player.buildingPlayerEffects.Add(("Autocracy", new BuildingEffect("AutocracyEffect"), "")); //+10% of all yields in Capital
        foreach (int unitID in player.unitList)
        {
            Global.gameManager.game.unitDictionary[unitID].RecalculateEffects();
        }
        foreach(int cityID in player.cityList)
        {
            Global.gameManager.game.cityDictionary[cityID].RecalculateYields();
        }
    }
    static void RemoveAutocracyGovernmentEffect(BasePlayer player)
    {
        player.militaryPolicySlots -= 2;
        player.economicPolicySlots -= 1;
        player.heroicPolicySlots -= 1;
        RemovePlayerBuildingEffects("Autocracy", player);
        RemovePlayerUnitEffects("Autocracy", player);
        foreach (int unitID in player.unitList)
        {
            Global.gameManager.game.unitDictionary[unitID].RecalculateEffects();
        }
        foreach (int cityID in player.cityList)
        {
            Global.gameManager.game.cityDictionary[cityID].RecalculateYields();
        }
    }
    //Classical Republic
    static void AddClassicalRepublicGovernmentEffect(BasePlayer player)
    {
        player.economicPolicySlots += 2;
        player.diplomaticPolicySlots += 1;
        player.heroicPolicySlots += 1;
        player.buildingPlayerEffects.Add(("ClassicalRepublic", new BuildingEffect("ClassicalRepublicEffect"), "")); //+ 1 Happiness for each urban district
        //player.?.Add(("ClassicalRepublic"), "+10% hero xp gain");
        foreach (int unitID in player.unitList)
        {
            Global.gameManager.game.unitDictionary[unitID].RecalculateEffects();
        }
        foreach (int cityID in player.cityList)
        {
            Global.gameManager.game.cityDictionary[cityID].RecalculateYields();
        }
    }
    static void RemoveClassicalRepublicGovernmentEffect(BasePlayer player)
    {
        player.economicPolicySlots -= 2;
        player.diplomaticPolicySlots -= 1;
        player.heroicPolicySlots -= 1;
        RemovePlayerBuildingEffects("ClassicalRepublic", player);
        RemovePlayerUnitEffects("ClassicalRepublic", player);
        foreach (int unitID in player.unitList)
        {
            Global.gameManager.game.unitDictionary[unitID].RecalculateEffects();
        }
        foreach (int cityID in player.cityList)
        {
            Global.gameManager.game.cityDictionary[cityID].RecalculateYields();
        }
    }

    //Oligarchy
    static void AddOligarchyGovernmentEffect(BasePlayer player)
    {
        player.militaryPolicySlots += 1;
        player.economicPolicySlots += 1;
        player.diplomaticPolicySlots += 1;
        player.heroicPolicySlots += 1;
        player.unitPlayerEffects.Add(("Oligarchy", new UnitEffect(UnitEffectType.CombatStrength, EffectOperation.Add, 4, 0), UnitClass.Infantry | UnitClass.Naval)); //+4 combat strength to all melee units (land and naval)
        //player.?.Add(("ClassicalRepublic"), "+10% hero xp gain");
        foreach (int unitID in player.unitList)
        {
            Global.gameManager.game.unitDictionary[unitID].RecalculateEffects();
        }
        foreach (int cityID in player.cityList)
        {
            Global.gameManager.game.cityDictionary[cityID].RecalculateYields();
        }
    }
    static void RemoveOligarchyGovernmentEffect(BasePlayer player)
    {
        player.militaryPolicySlots -= 1;
        player.economicPolicySlots -= 1;
        player.diplomaticPolicySlots -= 1;
        player.heroicPolicySlots -= 1;
        RemovePlayerBuildingEffects("Oligarchy", player);
        RemovePlayerUnitEffects("Oligarchy", player);
        foreach (int unitID in player.unitList)
        {
            Global.gameManager.game.unitDictionary[unitID].RecalculateEffects();
        }
        foreach (int cityID in player.cityList)
        {
            Global.gameManager.game.cityDictionary[cityID].RecalculateYields();
        }
    }

    //Monarchy
    static void AddMonarchyGovernmentEffect(BasePlayer player)
    {
        player.militaryPolicySlots += 2;
        player.economicPolicySlots += 1;
        player.diplomaticPolicySlots += 1;
        player.heroicPolicySlots += 2;
        player.unitPlayerEffects.Add(("Monarch", new UnitEffect(UnitEffectType.CombatStrength, EffectOperation.Add, 4, 0), UnitClass.Combat));
        foreach (int unitID in player.unitList)
        {
            Global.gameManager.game.unitDictionary[unitID].RecalculateEffects();
        }
        foreach (int cityID in player.cityList)
        {
            Global.gameManager.game.cityDictionary[cityID].RecalculateYields();
        }
    }
    static void RemoveMonarchyGovernmentEffect(BasePlayer player)
    {
        player.militaryPolicySlots -= 2;
        player.economicPolicySlots -= 1;
        player.diplomaticPolicySlots -= 1;
        player.heroicPolicySlots -= 2;
        RemovePlayerBuildingEffects("Oligarchy", player);
        RemovePlayerUnitEffects("Oligarchy", player);
        foreach (int unitID in player.unitList)
        {
            Global.gameManager.game.unitDictionary[unitID].RecalculateEffects();
        }
        foreach (int cityID in player.cityList)
        {
            Global.gameManager.game.cityDictionary[cityID].RecalculateYields();
        }
    }

    public static string GetGovernmentTypeTitle(GovernmentType governmentType)
    {
        switch (governmentType)
        {
            case GovernmentType.Tribal:
                return "Tribal";
            case GovernmentType.Autocracy:
                return "Autocracy";
            case GovernmentType.ClassicalRepublic:
                return "ClassicalRepublic";
            case GovernmentType.Oligarchy:
                return "Oligarchy";
            case GovernmentType.Monarchy:
                return "Monarchy";
            default:
                return "NOT PROVIDED";
        }
    }
    public static string GetGovernmentTypeDescription(GovernmentType governmentType)
    {
        switch (governmentType)
        {
            case GovernmentType.Tribal:
                return "+1 Military Policy Slot\n+1 Economic Policy Slot\n+1 Diplomatic Policy Slot\n+2 Combat Strength to All Land Units";
            case GovernmentType.Autocracy:
                return "+2 Military Policy Slots\n+1 Economic Policy Slot\n+1 Heroic Policy Slot\n+10% to all yields in your Capital";
            case GovernmentType.ClassicalRepublic:
                return "+2 Economic Policy Slots\n+1 Diplomatic Policy Slot\n+1 Heroic Policy Slot\nCities recieve +1 Happiness in every Urban District\n+10% all Heroic Skill Point Gains";
            case GovernmentType.Oligarchy:
                return "+1 Military Policy Slot\n+1 Economic Policy Slot\n+1 Diplomatic Policy Slot\n+1 Heroic Policy Slot\n+4 Combat Strength for all Melee Units (Land and Naval)";
            case GovernmentType.Monarchy:
                return "+2 Military Policy Slots\n+1 Economic Policy Slot\n+1 Diplomatic Policy Slots\n+2 Heroic Policy Slot\nPLACEHOLDER (+4 Combat Strength for all Combat Units)";
            default:
                return "NOT PROVIDED";
        }
    }
    public static Texture2D GetGovernmentTypeIcon(GovernmentType governmentType)
    {
        switch (governmentType)
        {
            case GovernmentType.Tribal:
                return GD.Load<CompressedTexture2D>("res://graphics/ui/icons/government.png");
            case GovernmentType.Autocracy:
                return GD.Load<CompressedTexture2D>("res://graphics/ui/icons/government.png");
            case GovernmentType.ClassicalRepublic:
                return GD.Load<CompressedTexture2D>("res://graphics/ui/icons/government.png");
            case GovernmentType.Oligarchy:
                return GD.Load<CompressedTexture2D>("res://graphics/ui/icons/government.png");
            case GovernmentType.Monarchy:
                return GD.Load<CompressedTexture2D>("res://graphics/ui/icons/government.png");
            default:
                return GD.Load<CompressedTexture2D>("res://graphics/ui/icons/government.png");
        }
    }

    //policy card specific effects

}

public enum GovernmentType
{
    Tribal,

    Autocracy,
    ClassicalRepublic,
    Oligarchy,

    Monarchy,

    //MerchantRepublic,
    //Theocracy,

    //Communism,
    //Democracy,
    //Facism
}

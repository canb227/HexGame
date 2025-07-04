using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Godot.GodotThread;

public static class PlayerEffect
{
    public static void ProcessFunctionString(String functionString, Player player)
    {
        Dictionary<String, Action<Player>> effectFunctions = new Dictionary<string, Action<Player>>
        {
            { "AddTribalGovernmentEffect", AddTribalGovernmentEffect },
        };

        if (effectFunctions.TryGetValue(functionString, out Action<Player> effectFunction))
        {
            effectFunction(player);
        }
        else
        {
            throw new ArgumentException($"Function '{functionString}' not recognized in PlayerEffect");
        }
    }
    public static void AddTribalGovernmentEffect(Player player)
    {
        player.flatYields.food += 1; //all flat land makes +1 food
        player.buildingPlayerEffects.Add(("TribalGovernment", new BuildingEffect(BuildingEffectType.ProductionCost, EffectOperation.Multiply, 0.90f, 0), "")); //buildings are 10% cheaper (DOESNT WORK CURRENTLY
        player.unitPlayerEffects.Add(("TribalGovernment", new UnitEffect(UnitEffectType.CombatStrength, EffectOperation.Add, 2, 0), UnitClass.Land)); //increase land strength by +2
    }
    public static void RemoveTribalGovernmentEffect(Player player)
    {
        player.flatYields.food -= 1; //remove all flat land makes +1 food
        RemovePlayerBuildingEffects("TribalGovernment", player);
        RemovePlayerUnitEffects("TribalGovernment", player);
    }

    static void RemovePlayerBuildingEffects(string name, Player player)
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
    static void RemovePlayerUnitEffects(string name, Player player)
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
}

public enum GovernmentType
{
    Tribal,

    Autocracy,
    ClassicalRepublic,
    Oligarchy,

    MerchantRepublic,
    Monarchy,
    Theocracy,

    Communism,
    Democracy,
    Facism
}

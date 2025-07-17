using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data;
using Godot;
using System.IO;

public enum BuildingEffectType
{
    ProductionCost,
    GoldCost,
    MaintenanceCost,
    FoodYield,
    ProductionYield,
    GoldYield,
    ScienceYield,
    CultureYield,
    HappinessYield    
}

public class BuildingEffect
{
    public BuildingEffect(BuildingEffectType effectType, EffectOperation effectOperation, float effectMagnitude, int priority)
    {
        this.effectType = effectType;
        this.effectOperation = effectOperation;
        this.effectMagnitude = effectMagnitude;
        this.priority = priority;
    }

/*    public BuildingEffect(Action<Building> applyFunction, int priority)
    {
        //default values
        this.effectType = BuildingEffectType.ProductionCost;
        this.effectOperation = EffectOperation.Add;
        this.effectMagnitude = 0.0f;
        //real values
        this.priority = priority;
        this.applyFunction = applyFunction;
    }*/

    public BuildingEffect(String functionName)
    {
        this.functionName = functionName;
    }

    public BuildingEffect()
    {
    }
    
    public BuildingEffectType effectType { get; set; }
    public EffectOperation effectOperation { get; set; }
    public float effectMagnitude { get; set; }
    public int priority { get; set; }
    public Action<Building>? applyFunction { get; set; }
    public String functionName { get; set; } = "";

    public void ApplyEffect(Building building)
    {
        if (applyFunction != null)
        {
            applyFunction(building);
        }
        else if (functionName != "")
        {
            ProcessFunctionString(functionName, building);
        }
        else
        {
            if (effectType == BuildingEffectType.ProductionCost)
            {
                building.productionCost = ApplyOperation(building.productionCost);
            }
            else if (effectType == BuildingEffectType.GoldCost)
            {
                building.goldCost = ApplyOperation(building.goldCost);
            }
            else if (effectType == BuildingEffectType.MaintenanceCost)
            {
                building.maintenanceCost = ApplyOperation(building.maintenanceCost);
            }
            else if (effectType == BuildingEffectType.FoodYield)
            {
                building.yields.food = ApplyOperation(building.yields.food);
            }
            else if (effectType == BuildingEffectType.ProductionYield)
            {
                building.yields.production = ApplyOperation(building.yields.production);
            }
            else if (effectType == BuildingEffectType.GoldYield)
            {
                building.yields.gold = ApplyOperation(building.yields.gold);
            }
            else if (effectType == BuildingEffectType.ScienceYield)
            {
                building.yields.science = ApplyOperation(building.yields.science);
            }
            else if (effectType == BuildingEffectType.CultureYield)
            {
                building.yields.culture = ApplyOperation(building.yields.culture);
            }
            else if (effectType == BuildingEffectType.HappinessYield)
            {
                building.yields.happiness = ApplyOperation(building.yields.happiness);
            }

        }
    }
    float ApplyOperation(float property)
    {
        switch (effectOperation)
        {
            case EffectOperation.Multiply:
                property *= effectMagnitude;
                break;
            case EffectOperation.Divide:
                property /= effectMagnitude;
                break;
            case EffectOperation.Add:
                property += effectMagnitude;
                break;
            case EffectOperation.Subtract:
                property -= effectMagnitude;
                break;
        }
        return property;
    }
    void ProcessFunctionString(String functionString, Building building)
    {
        Dictionary<String, Func<Building, Yields>> effectFunctions = new Dictionary<string, Func<Building, Yields>>
        {
            { "WaterSupplyEffect", WaterSupplyEffect },
            { "FarmEffect", FarmEffect },
            { "PastureEffect", PastureEffect },
            { "MineEffect", MineEffect },
            { "LumbermillEffect", LumbermillEffect },
            { "FishingBoatEffect", FishingBoatEffect },
            { "RefineryEffect", RefineryEffect },
            { "IndustryEffect", IndustryEffect },
            { "CommerceEffect", CommerceEffect },
            { "CampusEffect", CampusEffect },
            { "CulturalEffect", CulturalEffect },
            { "EntertainmentEffect", EntertainmentEffect },
            { "HeroicDistrictEffect", HeroicDistrictEffect },
            { "HarborEffect", HarborEffect },
            { "MilitaristicEffect", MilitaristicEffect },
            { "GranaryEffect", GranaryEffect },
            { "DockWarehouseEffect", DockWarehouseEffect },
            { "StoneCutterWarehouseEffect", StoneCutterWarehouseEffect },
            { "ArenaEffect", ArenaEffect },
            { "TempleEffect", TempleEffect },
            { "GardenEffect", GardenEffect },
            { "LibraryEffect", LibraryEffect },
            { "AncientWallEffect", AncientWallEffect },
            { "ReservoirEffect", ReservoirEffect },
            { "StonecutterEffect", StoneCutterEffect },
            { "LumberyardEffect", LumberyardEffect },
            { "AmpitheaterEffect", AmpitheaterEffect },
            { "CityCenterWallEffect", CityCenterWallEffect },
            { "MonumentEffect", MonumentEffect },
            { "ShrineEffect", ShrineEffect }, 
            { "BarracksEffect", BarracksEffect },
            { "WatermillEffect", WatermillEffect },
            { "LighthouseEffect", LighthouseEffect }, 
            { "MarketEffect", MarketEffect }, 
            { "StablesEffect", StablesEffect }, 
            { "AqueductEffect", AqueductEffect }, 
            { "WorkshopEffect", WorkshopEffect }, 
            { "UniversityEffect", UniversityEffect }, 
            { "ArmoryEffect", ArmoryEffect }, 
            { "MedievalWallsEffect", MedievalWallsEffect },
            { "StonehengeEffect", StonehengeEffect },
            { "ColossusEffect", ColossusEffect },
            { "PetraEffect", PetraEffect },
            { "TerracottaArmyEffect", TerracottaArmyEffect },
            { "MachuPicchuEffect", MachuPicchuEffect },
            { "OracleEffect", OracleEffect },
            { "ColosseumEffect", ColosseumEffect },

            { "FeudalismEffect", FeudalismEffect },

            { "AutocracyEffect", AutocracyEffect },
            { "ClassicalRepublicEffect", ClassicalRepublicEffect },
        };
        
        if (effectFunctions.TryGetValue(functionString, out Func<Building, Yields> effectFunction))
        {
            effectFunction(building);
        }
        else
        {
            throw new ArgumentException($"Function '{functionString}' not recognized in BuildingEffect from Buildings file.");
        }
    }
    Yields WaterSupplyEffect(Building building)
    {
        float waterHappinessYield = 0.0f;
        
        if(Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].terrainType == TerrainType.Coast ||Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].featureSet.Contains(FeatureType.River) 
                || Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].featureSet.Contains(FeatureType.Wetland))
        {
            waterHappinessYield = 5.0f;
        }
        else
        {
            foreach(Hex hex in building.districtHex.WrappingNeighbors(Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.bottom))
            {
                if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].terrainType == TerrainType.Coast || Global.gameManager.game.mainGameBoard.gameHexDict[hex].featureSet.Contains(FeatureType.River) 
                    || Global.gameManager.game.mainGameBoard.gameHexDict[hex].featureSet.Contains(FeatureType.Wetland))
                {
                    waterHappinessYield = 5.0f;
                    break;
                }
            }
        }
        building.yields.happiness += waterHappinessYield;
        Yields yields = new Yields();
        yields.happiness += 5;
        return yields;
    }
    Yields FarmEffect(Building building)
    {
        return new Yields();
    }
    Yields PastureEffect(Building building)
    {
        return new Yields();
    }
    Yields MineEffect(Building building)
    {
        return new Yields();
    }
    Yields LumbermillEffect(Building building)
    {
        return new Yields(); 
    }
    Yields FishingBoatEffect(Building building)
    {
        return new Yields(); 
    }

    //district base buildings
    Yields RefineryEffect(Building building)
    {
        Yields yields = new Yields();
        foreach (Hex hex in building.districtHex.WrappingNeighbors(Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.bottom))
        {
            if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].district != null && !Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.isUrban)
            {
                yields.food += 1;
            }
        }
        building.yields += yields;
        return yields; 
    }

    Yields IndustryEffect(Building building)
    {
        Yields yields = new Yields();
        foreach (Hex hex in building.districtHex.WrappingNeighbors(Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.bottom))
        {
            if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].district != null && (Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.districtType == DistrictType.gold 
                || Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.districtType == DistrictType.dock 
                || Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.districtType == DistrictType.military))
            {
                yields.production += 1;
            }
        }
        building.yields += yields;
        return yields;
    }

    Yields CommerceEffect(Building building)
    {
        Yields yields = new Yields();
        foreach (Hex hex in building.districtHex.WrappingNeighbors(Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.bottom))
        {
            if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].district != null && (Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.districtType == DistrictType.production
                || Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.districtType == DistrictType.dock
                || Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.districtType == DistrictType.science))
            {
                yields.gold += 1;
            }
        }
        building.yields += yields;
        return yields;
    }

    Yields CampusEffect(Building building)
    {
        Yields yields = new Yields();
        foreach (Hex hex in building.districtHex.WrappingNeighbors(Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.bottom))
        {
            if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].district != null && (Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.districtType == DistrictType.culture
                || Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.districtType == DistrictType.happiness
                || Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.districtType == DistrictType.citycenter))
            {
                yields.science += 1;
            }
        }
        building.yields += yields;
        return yields;
    }

    Yields CulturalEffect(Building building)
    {
        Yields yields = new Yields();
        foreach (Hex hex in building.districtHex.WrappingNeighbors(Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.bottom))
        {
            if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].district != null && (Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.districtType == DistrictType.heroic
                || Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.districtType == DistrictType.gold
                || Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.districtType == DistrictType.citycenter))
            {
                yields.culture += 1;
            }
        }
        building.yields += yields;
        return yields;
    }

    Yields EntertainmentEffect(Building building)
    {
        Yields yields = new Yields();
        foreach (Hex hex in building.districtHex.WrappingNeighbors(Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.bottom))
        {
            if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].district != null && (Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.districtType == DistrictType.rural
                || Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.districtType == DistrictType.culture))
            {
                yields.happiness += 1;
            }
        }
        building.yields += yields;
        return yields;
    }

    Yields HeroicDistrictEffect(Building building)
    {
        Yields yields = new Yields();
        foreach (Hex hex in building.districtHex.WrappingNeighbors(Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.bottom))
        {
            if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].district != null && (Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.districtType == DistrictType.science
                || Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.districtType == DistrictType.military))
            {
                yields.influence += 1;
            }
        }
        building.yields += yields;
        return yields;
    }


    Yields HarborEffect(Building building)
    {
        Yields yields = new Yields();
        foreach (Hex hex in building.districtHex.WrappingNeighbors(Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.bottom))
        {
            if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].district != null && (Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.districtType == DistrictType.dock
                || Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.districtType == DistrictType.gold
                || Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.districtType == DistrictType.citycenter))
            {
                yields.gold += 1;
            }
        }
        building.yields += yields;
        return yields;
    }

    Yields MilitaristicEffect(Building building)
    {
        Yields yields = new Yields();
        foreach (Hex hex in building.districtHex.WrappingNeighbors(Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.bottom))
        {
            if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].district != null && (Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.districtType == DistrictType.heroic 
                    || Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.districtType == DistrictType.citycenter 
                    || Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.districtType == DistrictType.science))
            {
                yields.production += 1;
            }
        }
        building.yields += yields;
        return yields;
    }

    Yields GranaryEffect(Building building)
    {
        return new Yields(); 
    }
    Yields DockWarehouseEffect(Building building)
    {
        return new Yields(); 
    }



    Yields StoneCutterWarehouseEffect(Building building)
    {
        Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].district.cityID].roughYields.production += 1;
        return new Yields(); 
    }
    Yields ArenaEffect(Building building)
    {
        return new Yields(); 
    }
    Yields TempleEffect(Building building)
    {
        return new Yields(); 
    }
    Yields GardenEffect(Building building)
    {
        //garden effect TODO
        return new Yields(); 
    }
    Yields LibraryEffect(Building building)
    {
        //library effect TODO
        return new Yields();
    }
    Yields AncientWallEffect(Building building)
    {
        if(!Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].district.hasWalls)
        {
            Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].district.AddWalls(100.0f);
        }
        return new Yields();
    }

    Yields ReservoirEffect(Building building)
    {
        Yields yields = new Yields();
        foreach (District district in Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].district.cityID].districts)
        {
            foreach(Building districtBuilding in district.buildings)
            {
                if(districtBuilding.name == "Farm")
                {
                    yields.food += 1;
                    districtBuilding.yields.food += 1;
                }
            }
        }
        return yields;
    }

    Yields StoneCutterEffect(Building building)
    {
        Yields yields = new Yields();
        foreach (District district in Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].district.cityID].districts)
        {
            foreach (Building districtBuilding in district.buildings)
            {
                if (districtBuilding.name == "Mine")
                {
                    yields.production += 1;
                    districtBuilding.yields.production += 1;
                }
            }
        }
        return yields;
    }

    Yields LumberyardEffect(Building building)
    {
        Yields yields = new Yields();
        foreach (District district in Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].district.cityID].districts)
        {
            foreach (Building districtBuilding in district.buildings)
            {
                if (districtBuilding.name == "Lumbermill")
                {
                    yields.production += 1;
                    districtBuilding.yields.production += 1;
                }
            }
        }
        return yields;
    }

    Yields MonumentEffect(Building building)
    {
        return new Yields();
    }
    Yields AmpitheaterEffect(Building building)
    {
        return new Yields();
    }
    Yields CityCenterWallEffect(Building building)
    {
        if(!Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].district.hasWalls)
        {
            Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].district.maxHealth = 50.0f;
            Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].district.health = 50.0f;
            Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].district.hasWalls = true;
        }
        return new Yields();
    }

    Yields ShrineEffect(Building building)
    {
        //something with the hero I think
        return new Yields();
    }
    Yields BarracksEffect(Building building)
    {
        Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].district.cityID].infantryProductionCombatModifier += 1;
        return new Yields();
    }

    Yields WatermillEffect(Building building)
    {
        return new Yields();
    }
    Yields LighthouseEffect(Building building)
    {
        Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].district.cityID].coastalYields.food += 1;
        Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].district.cityID].navalProductionCombatModifier += 1;
        return new Yields();
    }

    Yields MarketEffect(Building building)
    {
        Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].district.cityID].maxResourcesHeld += 1;
        return new Yields();
    }

    Yields StablesEffect(Building building)
    {
        Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].district.cityID].cavalryProductionCombatModifier += 1;
        return new Yields();
    }

    Yields AqueductEffect(Building building)
    {
        return new Yields();
    }

    Yields WorkshopEffect(Building building)
    {
        return new Yields();
    }

    Yields UniversityEffect(Building building)
    {
        return new Yields();
    }

    Yields ArmoryEffect(Building building)
    {
        return new Yields();
    }

    Yields MedievalWallsEffect(Building building)
    {
        return new Yields();
    }

    //world wonders

    Yields StonehengeEffect(Building building)
    {
        return new Yields();
    }

    Yields ColossusEffect(Building building)
    {
        City city = Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].district.cityID];
        city.additionalTradeRoutes += 1;
        return new Yields();
    }

    Yields PetraEffect(Building building)
    {
        Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].district.cityID].desertYields.food += 2;
        Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].district.cityID].desertYields.gold += 2;
        Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].district.cityID].desertYields.production += 1;
        return new Yields();
    }

    Yields TerracottaArmyEffect(Building building)
    {
        //provide some bonus to all units
        return new Yields();
    }

    Yields MachuPicchuEffect(Building building)
    {
        //mountains provide a adjacency bonus to districts in all cities
        return new Yields();
    }

    Yields OracleEffect(Building building)
    {
        //something hero related
        return new Yields();
    }

    Yields ColosseumEffect(Building building)
    {
        return new Yields();
    }

    Yields FeudalismEffect(Building building)
    {
        Yields yields = new Yields();
        foreach (Hex hex in building.districtHex.WrappingNeighbors(Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.bottom))
        {
            foreach(Building adjacentBuilding in Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.buildings)
            {
                if (building.name == "Farm")
                {
                    yields.food += 1;
                }
            }

        }
        return yields;
    }

    //government effects
    Yields AutocracyEffect(Building building)
    {
        if(Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].district.cityID].isCapital)
        {
            building.yields.food *= 1.1f;
            building.yields.production *= 1.1f;
            building.yields.gold *= 1.1f;
            building.yields.science *= 1.1f;
            building.yields.culture *= 1.1f;
            building.yields.happiness *= 1.1f;
            building.yields.influence *= 1.1f;
        }
        return new Yields();
    }

    Yields ClassicalRepublicEffect(Building building)
    {
        if(Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].district.isUrban)
        {
            if(building.name.Contains("District"))
            {
                building.yields.happiness += 1;
            }
        }
        return new Yields();
    }

}

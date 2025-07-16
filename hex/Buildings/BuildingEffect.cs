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
        Dictionary<String, Action<Building>> effectFunctions = new Dictionary<string, Action<Building>>
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

            { "AutocracyEffect", AutocracyEffect },
            { "ClassicalRepublicEffect", ClassicalRepublicEffect },
        };
        
        if (effectFunctions.TryGetValue(functionString, out Action<Building> effectFunction))
        {
            effectFunction(building);
        }
        else
        {
            throw new ArgumentException($"Function '{functionString}' not recognized in BuildingEffect from Buildings file.");
        }
    }
    void WaterSupplyEffect(Building building)
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
    }
    void FarmEffect(Building building)
    {
    }
    void PastureEffect(Building building)
    {

    }
    void MineEffect(Building building)
    {

    }
    void LumbermillEffect(Building building)
    {

    }
    void FishingBoatEffect(Building building)
    {

    }
    void RefineryEffect(Building building)
    {

    }

    void IndustryEffect(Building building)
    {

    }

    void CommerceEffect(Building building)
    {

    }

    void CampusEffect(Building building)
    {

    }

    void CulturalEffect(Building building)
    {

    }

    void EntertainmentEffect(Building building)
    {

    }

    void HeroicDistrictEffect(Building building)
    {

    }

    void HarborEffect(Building building)
    {

    }

    void MilitaristicEffect(Building building)
    {

    }

    void GranaryEffect(Building building)
    {
        
    }
    void DockWarehouseEffect(Building building)
    {
    }



    void StoneCutterWarehouseEffect(Building building)
    {
        Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].district.cityID].roughYields.production += 1;
    }
    void ArenaEffect(Building building)
    {
        
    }
    void TempleEffect(Building building)
    {
        
    }
    void GardenEffect(Building building)
    {
        //garden effect TODO
    }
    void LibraryEffect(Building building)
    {
        //library effect TODO
    }
    void AncientWallEffect(Building building)
    {
        if(!Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].district.hasWalls)
        {
            Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].district.AddWalls(100.0f);
        }
    }

    void MonumentEffect(Building building)
    {
    }
    void AmpitheaterEffect(Building building)
    {

    }
    void CityCenterWallEffect(Building building)
    {
        if(!Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].district.hasWalls)
        {
            Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].district.maxHealth = 50.0f;
            Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].district.health = 50.0f;
            Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].district.hasWalls = true;
        }
    }

    void ShrineEffect(Building building)
    {
        //something with the hero I think
    }
    void BarracksEffect(Building building)
    {
        Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].district.cityID].infantryProductionCombatModifier += 1;
    }

    void WatermillEffect(Building building)
    {

    }
    void LighthouseEffect(Building building)
    {
        Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].district.cityID].coastalYields.food += 1;
        Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].district.cityID].navalProductionCombatModifier += 1;
    }

    void MarketEffect(Building building)
    {
        Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].district.cityID].maxResourcesHeld += 1;
    }

    void StablesEffect(Building building)
    {
        Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].district.cityID].cavalryProductionCombatModifier += 1;
    }

    void AqueductEffect(Building building)
    {
    }

    void WorkshopEffect(Building building)
    {
    }

    void UniversityEffect(Building building)
    {
    }

    void ArmoryEffect(Building building)
    {
    }

    void MedievalWallsEffect(Building building)
    {
    }

    //world wonders

    void StonehengeEffect(Building building)
    {
    }

    void ColossusEffect(Building building)
    {
        City city = Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].district.cityID];
        city.additionalTradeRoutes += 1;
    }

    void PetraEffect(Building building)
    {
        Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].district.cityID].desertYields.food += 2;
        Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].district.cityID].desertYields.gold += 2;
        Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].district.cityID].desertYields.production += 1;
    }

    void TerracottaArmyEffect(Building building)
    {
        //provide some bonus to all units
    }

    void MachuPicchuEffect(Building building)
    {
        //mountains provide a adjacency bonus to districts in all cities
    }

    void OracleEffect(Building building)
    {
        //something hero related
    }

    void ColosseumEffect(Building building)
    {

    }

    //government effects
    void AutocracyEffect(Building building)
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
    }

    void ClassicalRepublicEffect(Building building)
    {
        if(Global.gameManager.game.mainGameBoard.gameHexDict[building.districtHex].district.isUrban)
        {
            if(building.name.Contains("District"))
            {
                building.yields.happiness += 1;
            }
        }
    }

}

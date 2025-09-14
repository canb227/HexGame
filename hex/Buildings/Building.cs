using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data;
using Godot;
using System.IO;
using NetworkMessages;

[Serializable]
public partial class Building
{
    public String name { get; set; }
    public int id { get; set; }
    public String buildingType { get; set; }
    public Hex districtHex { get; set; }
    private List<BuildingEffect> buildingEffects { get; set; } 
    public float baseProductionCost { get; set; }
    public float productionCost { get; set; }
    public float baseGoldCost { get; set; } 
    public float goldCost { get; set; } 
    public float baseMaintenanceCost { get; set; }
    public float maintenanceCost { get; set; }
    public Yields baseYields { get; set; }
    public Yields yields { get; set; }
    public bool isDistrictCenterBuilding { get; set; }
    public bool CurrentlyPowered { get; set; }
    public int PoweredCost { get; set; }

    public Building(String buildingType, Hex districtHex, bool isResource)
    {
        this.districtHex = districtHex;
        this.buildingType = buildingType;
        this.name = buildingType;
        if(buildingType.Contains("District"))
        {
            isDistrictCenterBuilding = true;
        }
        else
        {
            isDistrictCenterBuilding = false;
        }
        // 'City Center' 'Farm' 'Mine' 'Hunting Camp' 'Fishing Boat' 'Whaling Ship'
        if (BuildingLoader.buildingsDict.TryGetValue(buildingType, out BuildingInfo buildingInfo))
        {
            this.productionCost = buildingInfo.ProductionCost;
            this.baseProductionCost = buildingInfo.ProductionCost;
            
            this.goldCost = buildingInfo.GoldCost;
            this.baseGoldCost = buildingInfo.GoldCost;
            
            this.maintenanceCost = buildingInfo.MaintenanceCost;
            this.baseMaintenanceCost = buildingInfo.MaintenanceCost;
            
            this.yields = buildingInfo.yields;
            this.baseYields = buildingInfo.yields;
            
            this.buildingEffects = new();
            foreach (String effectName in buildingInfo.Effects)
            {
                buildingEffects.Add(new BuildingEffect(effectName));
            }
        }
        if(BuildingLoader.buildingsDict[buildingType].Wonder)
        {
            Global.gameManager.game.builtWonders.Add(buildingType);
        }
        id = Global.gameManager.game.GetUniqueID(Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[districtHex].district.cityID].teamNum);
        if(!isResource)
        {
            var data = new Godot.Collections.Dictionary
            {
                { "q", districtHex.q },
                { "r", districtHex.r },
                { "s", districtHex.s }
            };

            if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager)) manager.CallDeferred("NewBuilding", buildingType, data, id, isDistrictCenterBuilding);
        }
    }

    public Building()
    {

    }
    public void SwitchTeams()
    {
        RecalculateYields();
    }

    public void DestroyBuilding()
    {
        if(BuildingLoader.buildingsDict[buildingType].Wonder)
        {
            Global.gameManager.game.builtWonders.Remove(buildingType);
        }
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
        {
            manager.CallDeferred("UpdateGraphic", id, (int)GraphicUpdateType.Remove);
            var data = new Godot.Collections.Dictionary
                {
                    { "q", districtHex.q },
                    { "r", districtHex.r },
                    { "s", districtHex.s }
                };
            manager.CallDeferred("UpdateHex", data);
        }
        districtHex = new Hex(0, 0, 0);
    }

    public void Raze()
    {
        DestroyBuilding();
    }

    public void AddEffect(BuildingEffect effect)
    {
        buildingEffects.Add(effect);
        Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[districtHex].district.cityID].RecalculateYields();
    }

    public void RemoveEffect(BuildingEffect effect)
    {
        buildingEffects.Remove(effect);
        Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[districtHex].district.cityID].RecalculateYields();
    }

    public void OnTurnStartedPowerDemand()
    {
        if (PoweredCost > 0)
        {
            CurrentlyPowered = false;
            Global.gameManager.game.mainGameBoard.gameHexDict[districtHex].district.powerDemand += PoweredCost;
        }
    }

    public void OnTurnStartedPowerSupply()
    {
        if (name == "CoalPowerPlant")
        {
            Player player = Global.gameManager.game.playerDictionary[Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[districtHex].district.cityID].teamNum];
            int availableCoal = player.resourceStockpiles[ResourceType.Coal];
            int maxPower = availableCoal * 5;
            int currentPower = 0;
            foreach (Hex hex in districtHex.WrappingRange(6, Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.top, Global.gameManager.game.mainGameBoard.bottom))
            {
                if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].district != null)
                {
                    District foundDistrict = Global.gameManager.game.mainGameBoard.gameHexDict[hex].district;
                    if (foundDistrict.isCityCenter)
                    {
                        City foundCity = Global.gameManager.game.cityDictionary[foundDistrict.cityID];
                        if (foundCity.teamNum == Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[districtHex].district.cityID].teamNum)
                        {
                            foreach(District district in foundCity.districts)
                            {

                                if (district.powerDemand > 0 && availableCoal > 0)
                                {
                                    // How much power we actually want to give
                                    int powerToGive = Math.Min(district.powerDemand, maxPower);

                                    // How many coal units that requires (round up so partial power still costs 1 coal)
                                    int coalNeeded = (int)Math.Ceiling(powerToGive / 5.0);

                                    // Don’t use more coal than we have
                                    coalNeeded = Math.Min(coalNeeded, availableCoal);

                                    // Final power we can actually give
                                    int actualPower = coalNeeded * 5;

                                    // Send power to the district
                                    district.ReceivePower(actualPower);

                                    // Subtract coal used
                                    player.resourceStockpiles[ResourceType.Coal] -= coalNeeded;
                                }
                            }
                        }
                    }
                }
            }
        }
        else if (name == "OilPowerPlant")
        {
            Player player = Global.gameManager.game.playerDictionary[Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[districtHex].district.cityID].teamNum];
            int avaliableOil = player.resourceStockpiles[ResourceType.Oil];
            int maxPower = avaliableOil * 7;
            int currentPower = 0;
            foreach (Hex hex in districtHex.WrappingRange(6, Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.top, Global.gameManager.game.mainGameBoard.bottom))
            {
                if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].district != null)
                {
                    District foundDistrict = Global.gameManager.game.mainGameBoard.gameHexDict[hex].district;
                    if (foundDistrict.isCityCenter)
                    {
                        City foundCity = Global.gameManager.game.cityDictionary[foundDistrict.cityID];
                        if (foundCity.teamNum == Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[districtHex].district.cityID].teamNum)
                        {
                            foreach (District district in foundCity.districts)
                            {

                                if (district.powerDemand > 0 && avaliableOil > 0)
                                {
                                    // How much power we actually want to give
                                    int powerToGive = Math.Min(district.powerDemand, maxPower);

                                    // How many coal units that requires (round up so partial power still costs 1 coal)
                                    int oilNeeded = (int)Math.Ceiling(powerToGive / 7.0);

                                    // Don’t use more coal than we have
                                    oilNeeded = Math.Min(oilNeeded, avaliableOil);

                                    // Final power we can actually give
                                    int actualPower = oilNeeded * 7;

                                    // Send power to the district
                                    district.ReceivePower(actualPower);

                                    // Subtract coal used
                                    player.resourceStockpiles[ResourceType.Oil] -= oilNeeded;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public void PrepareYieldRecalculate()
    {
        yields = new Yields(baseYields);
        PriorityQueue<BuildingEffect, int> orderedEffects = new();
        if (buildingEffects == null)
        {
            if (BuildingLoader.buildingsDict.TryGetValue(buildingType, out BuildingInfo buildingInfo))
            {
                buildingEffects = new();
                foreach (String effectName in buildingInfo.Effects)
                {
                    AddEffect(new BuildingEffect(effectName));
                }
            }
        }
        foreach (BuildingPlayerEffect effectName in Global.gameManager.game.playerDictionary[Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[districtHex].district.cityID].teamNum].buildingPlayerEffects)
        {
            if(effectName.effectedName == "" || effectName.effectedName == name)
            {
                orderedEffects.Enqueue(effectName.effect, effectName.effect.priority);
            }
        }
        foreach (BuildingEffect effect1 in buildingEffects)
        {
            orderedEffects.Enqueue(effect1, effect1.priority);
        }
        BuildingEffect effect;
        int priority;
        while(orderedEffects.TryDequeue(out effect, out priority))
        {
            effect.ApplyEffect(this);
        }
    }

    public void RecalculateYields()
    {
        Global.gameManager.game.mainGameBoard.gameHexDict[districtHex].yields += yields;
    }
}

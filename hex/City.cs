using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data;
using Godot;
using System.Reflection;
using NetworkMessages;
using System.IO;

public enum ProductionType
{
    Building,
    Unit,
}

[Serializable]
public class ProductionQueueType
{
    public String itemName { get; set; }
    public Hex targetHex { get; set; } //TODO save file issue
    public float productionLeft { get; set; }
    public float productionCost { get; set; }
    public String productionIconPath { get; set; }
    public ProductionQueueType(String itemName, Hex targetHex, float productionCost, float productionLeft)
    {
        this.itemName = itemName;
        this.targetHex = targetHex;
        this.productionCost = productionCost;
        this.productionLeft = productionLeft;
        if(UnitLoader.unitsDict.ContainsKey(itemName))
        {
            this.productionIconPath = UnitLoader.unitsDict[itemName].IconPath;
        }
        else if (BuildingLoader.buildingsDict.ContainsKey(itemName))
        {
            this.productionIconPath = BuildingLoader.buildingsDict[itemName].IconPath;
        }

    }

    public override bool Equals(object obj)
    {
        if (obj is ProductionQueueType)
        {
            if(((ProductionQueueType)obj).itemName == itemName && ((ProductionQueueType)obj).productionLeft == productionLeft && ((ProductionQueueType)obj).targetHex.Equals(targetHex))
            {
                return true;
            }
        }
        return false;
    }
}


[Serializable]
public class City
{
    public City(int id, int teamNum, String name, bool isCapital, GameHex gameHex)
    {
        Global.gameManager.game.cityDictionary.Add(id, this);
        this.id = id;
        originalCapitalTeamID = id;
        this.teamNum = teamNum;
        this.name = name;
        this.hex = gameHex.hex;
        productionQueue = new();
        partialProductionDictionary = new();
        heldResources = new();
        heldHexes = new();
        Global.gameManager.game.playerDictionary[teamNum].cityList.Add(this.id);
        districts = new();
        citySize = 2;
        naturalPopulation = 2;
        readyToExpand = 1; //we ready to expand for 
        maxDistrictSize = 2;
        baseMaxResourcesHeld = 1;
        foodToGrow = GetFoodToGrowCost();
        yields = new();
        myYields = new();

        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
        {
            manager.NewCity(this);
        }
        AddCityCenter(isCapital);
        this.isCapital = isCapital;
        this.wasCapital = isCapital;

        RecalculateYields();
        SetBaseHexYields();


    }

    public City()
    {

    }
    public int id { get; set; }
    public int teamNum { get; set; }
    public String name { get; set; }
    public bool isCapital { get; set; }
    public bool wasCapital { get; set; }
    public int originalCapitalTeamID { get; set; }
    public int maxDistrictSize { get; set; }
    public List<District> districts { get; set; } = new();
    public Hex hex { get; set; }
    public Yields yields { get; set; }
    public Yields myYields { get; set; }
    public Dictionary<YieldType, int> exportCount { get; set; } = new();
    public int citySize { get; set; }
    public int naturalPopulation { get; set; }
    public float foodToGrow{ get; set; }
    public float foodStockpile{ get; set; }
    public float productionOverflow{ get; set; }
    public Yields flatYields{ get; set; }
    public Yields roughYields{ get; set; }
    public Yields mountainYields{ get; set; }
    public Yields coastalYields{ get; set; }
    public Yields oceanYields{ get; set; }
    public Yields desertYields{ get; set; }
    public Yields plainsYields{ get; set; }
    public Yields grasslandYields{ get; set; }
    public Yields tundraYields{ get; set; }
    public Yields arcticYields{ get; set; }
    public List<ProductionQueueType> productionQueue{ get; set; } = new();
    public Dictionary<string, ProductionQueueType> partialProductionDictionary{ get; set; } = new();
    public Dictionary<Hex, ResourceType> heldResources{ get; set; } = new();
    public HashSet<Hex> heldHexes{ get; set; } = new();
    public int baseMaxResourcesHeld{ get; set; }
    public int maxResourcesHeld{ get; set; }
    public int readyToExpand{ get; set; }

    private void AddCityCenter(bool isCapital)
    {
        District district;
        if (isCapital)
        {
            district = new District(Global.gameManager.game.mainGameBoard.gameHexDict[hex], "Palace", true, true, id);
        }
        else
        {
            district = new District(Global.gameManager.game.mainGameBoard.gameHexDict[hex], "CityCenter", true, true, id);
        }
        districts.Add(district);
    }

    private void SetBaseHexYields()
    {
        flatYields = new();
        roughYields = new();
        mountainYields = new();
        coastalYields = new();
        oceanYields = new();
        desertYields = new();
        plainsYields = new();
        grasslandYields = new();
        tundraYields = new();
        arcticYields = new();
        
        flatYields.food += 1;
        roughYields.production += 1;
        //mountainYields.production += 0;
        coastalYields.food += 1;
        oceanYields.gold += 1;
        
        desertYields.gold += 1;
        plainsYields.production += 1;
        grasslandYields.food += 1;
        tundraYields.happiness += 1;
        //arcticYields
    }

    public (List<String>, List<String>) GetProducables()
    {
        List<String> buildings = new();
        List<String> units = new();
        foreach(String buildingType in Global.gameManager.game.playerDictionary[teamNum].allowedBuildings)
        {
            int count = 0;
            if(buildingType != "" & BuildingLoader.buildingsDict[buildingType].PerCity != 0 )
            {
                count = CountString(buildingType);
            }
            foreach(ProductionQueueType queueItem in productionQueue)
            {
                if (queueItem.itemName == buildingType)
                {
                    count += 1;
                }
            }
            if(!Global.gameManager.game.builtWonders.Contains(buildingType) & !BuildingLoader.buildingsDict[buildingType].Wonder)
            {
                if(count < BuildingLoader.buildingsDict[buildingType].PerCity)
                {
                    buildings.Add(buildingType);
                }
            }
        }

        foreach(String unitType in Global.gameManager.game.playerDictionary[teamNum].allowedUnits)
        {
            units.Add(unitType);
        }
        return (buildings, units);
    }

    public bool RemoveFromQueue(ProductionQueueType productionQueueItem)
    {
        return RemoveFromQueue(productionQueue.IndexOf(productionQueueItem));
    }

    public bool RemoveFromQueue(int index)
    {
        if(productionQueue.Count > index)
        {
            float prodCost = productionQueue[index].productionCost;

            if (productionQueue[index].productionLeft < prodCost)
            {
                bool foundNewHome = false;
                for(int i = 0; i < productionQueue.Count; i++)
                {
                    if (productionQueue[i].itemName == productionQueue[index].itemName & productionQueue[i].productionLeft > productionQueue[index].productionLeft)
                    {
                        foundNewHome = true;
                        productionQueue[i] = productionQueue[index];
                        break;
                    }
                }
                if(!foundNewHome)
                {
                    partialProductionDictionary.Add(productionQueue[index].itemName, productionQueue[index]);
                }
            }
            productionQueue.RemoveAt(index);

            if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
            {
                manager.uiManager.cityInfoPanel.UpdateCityPanelInfo();
                manager.UpdateGraphic(id, GraphicUpdateType.Update);
                manager.Update2DUI(UIElement.endTurnButton);
            }
            return true;
        }
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager2)) manager2.Update2DUI(UIElement.endTurnButton);
        return false;
    }

    public bool MoveToFrontOfProductionQueue(int indexToMove)
    {
        if (indexToMove < 0 || indexToMove >= productionQueue.Count)
        {
            GD.PushWarning("Index out of bounds");
            return false;
        }
        ProductionQueueType item = productionQueue[indexToMove];
        int frontalItemIndex = indexToMove;
        for (int i = 0; i < productionQueue.Count; i++)
        {
            if (productionQueue[i].itemName == productionQueue[indexToMove].itemName & productionQueue[i].productionLeft < productionQueue[indexToMove].productionLeft)
            {
                frontalItemIndex = i;
            }
        }

        //now we have frontalItemIndex, the index of the most finished item of our type and the index of the item we want to move, so we move our target item to the most finished item slot and move the most finished to the front
        //store bestItem, replace it with target, remove target, insert bestItem
        ProductionQueueType bestItem = productionQueue[frontalItemIndex];
        productionQueue[frontalItemIndex] = item;
        productionQueue.RemoveAt(indexToMove);
        productionQueue.Insert(0, bestItem);

        //RemoveFromQueue(indexToMove);
        //AddToFrontOfQueue(item.itemName, item.buildingType, item.unitType, item.targetGameHex, item.productionCost);
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
        {
            manager.uiManager.cityInfoPanel.UpdateCityPanelInfo();
            manager.UpdateGraphic(id, GraphicUpdateType.Update);
        }
        return true;
    }

    public int CountString(String buildingType)
    {
        int count = 0;
        foreach(District district in districts)
        {
            count += district.CountString(buildingType);
        }
        return count;
    }

    public bool AddToQueue(String itemName, Hex targetHex)
    {
        int count = 0;
        if(BuildingLoader.buildingsDict.ContainsKey(itemName))
        {
            if (BuildingLoader.buildingsDict[itemName].PerCity != 0)
            {
                count = CountString(itemName);
                foreach (ProductionQueueType queueItem in productionQueue)
                {
                    if (queueItem.itemName == itemName)
                    {
                        count += 1;
                    }
                }
                if (itemName != "" && (count >= BuildingLoader.buildingsDict[itemName].PerCity))
                {
                    return false;
                }
            }
            if (Global.gameManager.game.builtWonders.Contains(itemName))
            {
                return false;
            }
        }
        
        ProductionQueueType queueItem1;
        if(partialProductionDictionary.TryGetValue(itemName, out queueItem1))
        {
            partialProductionDictionary.Remove(itemName);
            productionQueue.Add(new ProductionQueueType(itemName, targetHex, queueItem1.productionCost, queueItem1.productionLeft));
        }
        else
        {
            if(BuildingLoader.buildingsDict.ContainsKey(itemName))
            {
                productionQueue.Add(new ProductionQueueType(itemName, targetHex, BuildingLoader.buildingsDict[itemName].ProductionCost, BuildingLoader.buildingsDict[itemName].ProductionCost));
            }
            else if(UnitLoader.unitsDict.ContainsKey(itemName))
            {
                float prodCost = UnitLoader.unitsDict[itemName].ProductionCost;
                if (itemName == "Settler")
                {
                    prodCost = UnitLoader.unitsDict[itemName].ProductionCost + 30 * Global.gameManager.game.playerDictionary[teamNum].settlerCount;
                }
                productionQueue.Add(new ProductionQueueType(itemName, targetHex, prodCost, prodCost));
            }
            else
            {
                throw new InvalidOperationException("Production Queue Item Name doesnt exist in Unit or Building Dictionary");
            }
        }
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
        {
            manager.uiManager.cityInfoPanel.UpdateCityPanelInfo();
            manager.UpdateGraphic(id, GraphicUpdateType.Update);
            manager.Update2DUI(UIElement.endTurnButton);

        }
        return true;
    }

    public bool AddToFrontOfQueue(String itemName, Hex targetHex)
    {
        int count = 0;
        if (BuildingLoader.buildingsDict.ContainsKey(itemName))
        {
            if (BuildingLoader.buildingsDict[itemName].PerCity != 0)
            {
                count = CountString(itemName);
            }
            foreach (ProductionQueueType queueItem in productionQueue)
            {
                if (queueItem.itemName == itemName)
                {
                    count += 1;
                }
            }
            if (itemName != "" && count >= BuildingLoader.buildingsDict[itemName].PerCity)
            {
                return false;
            }
            if (Global.gameManager.game.builtWonders.Contains(itemName))
            {
                return false;
            }
        }
        ProductionQueueType queueItem1;
        if (partialProductionDictionary.TryGetValue(itemName, out queueItem1))
        {
            partialProductionDictionary.Remove(itemName);
            productionQueue.Insert(0, new ProductionQueueType(itemName, targetHex, queueItem1.productionCost, queueItem1.productionLeft));
        }
        else
        {
            if (BuildingLoader.buildingsDict.ContainsKey(itemName))
            {
                productionQueue.Insert(0, new ProductionQueueType(itemName, targetHex, BuildingLoader.buildingsDict[itemName].ProductionCost, BuildingLoader.buildingsDict[itemName].ProductionCost));
            }
            else if(UnitLoader.unitsDict.ContainsKey(itemName))
            {
                float prodCost = UnitLoader.unitsDict[itemName].ProductionCost;
                if (itemName == "Settler")
                {
                    prodCost = UnitLoader.unitsDict[itemName].ProductionCost + 30 * Global.gameManager.game.playerDictionary[teamNum].settlerCount;
                }
                productionQueue.Insert(0, new ProductionQueueType(itemName, targetHex, prodCost, prodCost));
            }
            else
            {
                throw new InvalidOperationException("Production Queue Item Name doesnt exist in Unit or Building Dictionary");
            }
        }
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
        {
            manager.uiManager.cityInfoPanel.UpdateCityPanelInfo();
            manager.UpdateGraphic(id, GraphicUpdateType.Update);
        }
        return true;
    }


    public bool ChangeTeam(int newTeamNum)
    {
        foreach (District district in districts)
        {
            district.BeforeSwitchTeam();
        }
        heldResources.Clear();
        teamNum = newTeamNum;
        foreach (District district in districts)
        {
            district.AfterSwitchTeam();
        }
        RecalculateYields();
        productionQueue = new();
        partialProductionDictionary = new();
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
        {
            if(manager.selectedObjectID == id)
            {
                manager.UnselectObject();
            }
            manager.UpdateGraphic(id, GraphicUpdateType.Update);
        }
        return true;
    }

    public void ChangeName(String name)
    {
        this.name = name;
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
        {
            manager.uiManager.cityInfoPanel.UpdateCityPanelInfo();
            manager.UpdateGraphic(id, GraphicUpdateType.Update);
        }
    }

    public void OnTurnStarted(int turnNumber)
    {
        foreach(District district in districts)
        {
            district.HealForTurn(10.0f);
        }
        RecalculateYields();
        productionOverflow += yields.production;
        Global.gameManager.game.playerDictionary[teamNum].AddGold(yields.gold);
        Global.gameManager.game.playerDictionary[teamNum].AddScience(yields.science);
        Global.gameManager.game.playerDictionary[teamNum].AddCulture(yields.culture);
        Global.gameManager.game.playerDictionary[teamNum].AddHappiness(yields.happiness);
        Global.gameManager.game.playerDictionary[teamNum].AddInfluence(yields.influence);
        List<ProductionQueueType> toRemove = new List<ProductionQueueType>();
        for (int i = 0; i < productionQueue.Count; i++)
        {
            ProductionQueueType queueItem = productionQueue[i];
            if(BuildingLoader.buildingsDict.ContainsKey(queueItem.itemName))
            {
                if (Global.gameManager.game.builtWonders.Contains(queueItem.itemName))
                {
                    productionQueue.Remove(queueItem);
                    productionOverflow += queueItem.productionCost - queueItem.productionLeft;
                }
                int count = 0;
                if (queueItem.itemName != "" && BuildingLoader.buildingsDict[queueItem.itemName].PerCity != 0)
                {
                    count = CountString(queueItem.itemName);
                    foreach (ProductionQueueType queueItem2 in productionQueue)
                    {
                        if (!toRemove.Contains(queueItem2))
                        {
                            if (queueItem2.itemName == queueItem.itemName)
                            {
                                count += 1;
                            }
                        }
                    }
                    if (count > BuildingLoader.buildingsDict[queueItem.itemName].PerCity)
                    {
                        toRemove.Add(queueItem);
                        continue;
                    }
                }
                //hex doesnt have a enemy unit
                bool enemyUnitPresent = false;
                //check for enemy
                foreach (int unitID in Global.gameManager.game.mainGameBoard.gameHexDict[queueItem.targetHex].units)
                {
                    Unit unit = Global.gameManager.game.unitDictionary[unitID];
                    if (Global.gameManager.game.teamManager.GetEnemies(teamNum).Contains(unit.teamNum))
                    {
                        enemyUnitPresent = true;
                        break;
                    }
                }
                if (enemyUnitPresent)
                {
                    toRemove.Add(queueItem);
                    continue;
                }
                if (!ValidUrbanBuildHex(BuildingLoader.buildingsDict[queueItem.itemName].TerrainTypes, Global.gameManager.game.mainGameBoard.gameHexDict[queueItem.targetHex], BuildingLoader.buildingsDict[queueItem.itemName].DistrictType))
                {
                    toRemove.Add(queueItem);
                    continue;
                }
            }
        }
        foreach (ProductionQueueType item in toRemove)
        {
            RemoveFromQueue(item);
        }
        if (productionQueue.Any())
        {
            float productionLeftTemp = productionQueue[0].productionLeft;
            productionQueue[0].productionLeft -= productionOverflow;
            productionOverflow = Math.Max(productionOverflow - productionLeftTemp, 0);
            if (productionQueue[0].productionLeft <= 0)
            {
                GD.Print(BuildingLoader.buildingsDict.ContainsKey(productionQueue[0].itemName) + " " + UnitLoader.unitsDict.ContainsKey(productionQueue[0].itemName));
                if (BuildingLoader.buildingsDict.ContainsKey(productionQueue[0].itemName))
                {
                    BuildOnHex(productionQueue[0].targetHex, productionQueue[0].itemName);
                }
                else if (UnitLoader.unitsDict.ContainsKey(productionQueue[0].itemName))
                {
                    GD.Print("Try spawn unit");
                    Unit tempUnit = new Unit(productionQueue[0].itemName, Global.gameManager.game.GetUniqueID(teamNum), teamNum);
                    if (!Global.gameManager.game.mainGameBoard.gameHexDict[productionQueue[0].targetHex].SpawnUnit(tempUnit, false, true))
                    {
                        tempUnit.decreaseHealth(99999.9f);
                        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager1)) manager1.NewUnit(tempUnit);
                    }
                    if (UnitLoader.unitsDict.TryGetValue(tempUnit.unitType, out UnitInfo unitInfo))
                    {
                        if (unitInfo.CombatPower > Global.gameManager.game.playerDictionary[teamNum].strongestUnitBuilt)
                        {
                            Global.gameManager.game.playerDictionary[teamNum].strongestUnitBuilt = unitInfo.CombatPower;
                        }
                    }
                    if (productionQueue[0].itemName == "Settler")
                    {
                        Global.gameManager.game.playerDictionary[teamNum].IncreaseAllSettlerCost();
                    }
                }
                productionQueue.RemoveAt(0);
            }
        }
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager2)) manager2.Update2DUI(UIElement.endTurnButton);

        if (productionQueue.Any() == false)
        {
            if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager3)) manager3.UpdateGraphic(id, GraphicUpdateType.Update);
        }

        foodStockpile += yields.food;
        if (foodToGrow <= foodStockpile)
        {
            naturalPopulation += 1; //we increase naturalPopulation only here, and city size is increased for every building we have, rural or urban
            readyToExpand += 1;
            if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager3)) manager3.Update2DUI(UIElement.endTurnButton);
            foodStockpile = Math.Max(0.0f, foodStockpile - foodToGrow);
            foodToGrow = GetFoodToGrowCost(); //30 + (n-1) x 3 + (n-1) ^ 3.0 we use naturalPopulation so we dont punish the placement of urban buildings
        }
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager)) manager.UpdateGraphic(id, GraphicUpdateType.Update);
    }

    public void IncreaseSettlerCost()
    {
        foreach (ProductionQueueType item in productionQueue)
        {
            if (productionQueue[0].itemName == "Settler")
            {
                item.productionCost += 30;
                item.productionLeft += 30;
            }
        }
        ProductionQueueType item2;
        if(partialProductionDictionary.TryGetValue("Settler", out item2))
        {
            item2.productionCost += 30;
            item2.productionLeft += 30;
        }
    }

    public void DecreaseSettlerCost()
    {
        foreach (ProductionQueueType item in productionQueue)
        {
            if (productionQueue[0].itemName == "Settler")
            {
                item.productionLeft -= 30;
                item.productionCost -= 30;
            }
        }
        ProductionQueueType item2;
        if (partialProductionDictionary.TryGetValue("Settler", out item2))
        {
            item2.productionLeft -= 30;
            item2.productionCost -= 30;
        }
    }

    public float GetFoodToGrowCost()
    {
        //15 + (n - 1) x 8 + (n - 1) ^ 1.5
        //30 + (n - 1) x 3 + (n - 1) ^ 3.0
        return (float)(15 + (naturalPopulation) * 8 + Math.Pow((naturalPopulation), 1.5));
    }

    public void OnTurnEnded(int turnNumber)
    {

    }

    public bool ValidBuilding(string buildingName)
    {
        if (BuildingLoader.buildingsDict.ContainsKey(buildingName))
        {
            if (Global.gameManager.game.builtWonders.Contains(buildingName))
            {
                return false;
            }
            int count = 0;
            if (buildingName != "" && BuildingLoader.buildingsDict[buildingName].PerCity != 0)
            {
                count = CountString(buildingName);
                foreach (ProductionQueueType queueItem2 in productionQueue)
                {
                    if (queueItem2.itemName == buildingName)
                    {
                        count += 1;
                    }
                }
                if (count > BuildingLoader.buildingsDict[buildingName].PerCity)
                {
                    return false;
                }
            }            
        }
        return true;
    }

    public List<string> ValidBuildings()
    {
        List<string> buildings = new();
        foreach(string buildingName in Global.gameManager.game.playerDictionary[teamNum].allowedBuildings)
        {
            if(ValidBuilding(buildingName))
            {
                buildings.Add(buildingName);
            }
        }
        return buildings;
    }

    public bool ValidUnit(string unitName)
    {
        UnitInfo unitInfo = UnitLoader.unitsDict[unitName];
        return Global.gameManager.game.mainGameBoard.gameHexDict[hex].ValidHexToSpawn(unitInfo, false, true);
    }
    public List<string> ValidUnits()
    {
        List<string> units = new();
        foreach(string unitName in Global.gameManager.game.playerDictionary[teamNum].allowedUnits)
        {
            if (ValidUnit(unitName))
            {
                units.Add(unitName);
            }
        }
        return units;
    }

    public void DistrictFell()
    {
        bool allDistrictsFell = true;
        bool cityCenterOccupied = false;
        foreach(District district in districts)
        {
            if(district.health > 0.0f)
            {
                allDistrictsFell = false;
            }
            if(district.isCityCenter && district.health <= 0.0f)
            {
                if (Global.gameManager.game.mainGameBoard.gameHexDict[district.hex].units.Any())
                {
                    Unit unit = Global.gameManager.game.unitDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[district.hex].units[0]];
                    if (Global.gameManager.game.teamManager.GetEnemies(teamNum).Contains(unit.teamNum))
                    {
                        cityCenterOccupied = true;
                    }
                }
            }
        }
        if(allDistrictsFell && cityCenterOccupied)
        {
            ChangeTeam(Global.gameManager.game.unitDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[hex].units[0]].teamNum);
        }
    }

    public void MyYieldsRecalculateYields()
    {
        maxResourcesHeld = baseMaxResourcesHeld;
        yields = new();
        myYields = new();
        SetBaseHexYields();
        foreach (District district in districts)
        {
            district.PrepareYieldRecalculate();
        }
        foreach (District district in districts)
        {
            district.RecalculateYields();
            myYields += Global.gameManager.game.mainGameBoard.gameHexDict[district.hex].yields;
        }
        foreach (ResourceType resource in heldResources.Values)
        {
            ResourceLoader.ProcessFunctionString(ResourceLoader.resourceEffects[resource], id);
        }
        myYields.food -= naturalPopulation * 0.25f;
        yields += myYields;
        //myYields will hold onto the pre-export/import values
    }

    public void RecalculateYields()
    {
        MyYieldsRecalculateYields();
        //int exportCost = 0;
        foreach (YieldType export in exportCount.Keys)
        {
            //exportCost += 2;
            if (export == YieldType.food)
            {
                yields.food = 0;
            }
        }
        //yields.gold -= exportCost;
        //no gold cost for exports just a cap
        foreach (ExportRoute exportRoute in Global.gameManager.game.playerDictionary[teamNum].exportRouteList)
        {
            if(exportRoute.targetCityID == id)
            {
                Global.gameManager.game.cityDictionary[exportRoute.sourceCityID].MyYieldsRecalculateYields();
                if (exportRoute.exportType == YieldType.food)
                {
                    City sourceCity = Global.gameManager.game.cityDictionary[exportRoute.sourceCityID];
                    yields.food += sourceCity.myYields.food/ sourceCity.exportCount[exportRoute.exportType];
                }
            }
        }


        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
        {
            manager.Update2DUI(UIElement.goldPerTurn);
            manager.Update2DUI(UIElement.sciencePerTurn);
            manager.Update2DUI(UIElement.culturePerTurn);
            manager.Update2DUI(UIElement.happinessPerTurn);
            manager.Update2DUI(UIElement.influencePerTurn);
            manager.Update2DUI(UIElement.researchTree);
            manager.uiManager.UpdateResearchUI();
            manager.UpdateGraphic(id, GraphicUpdateType.Update);
            foreach(Hex hex in heldHexes)
            {
                manager.UpdateHex(hex);
            }
            manager.uiManager.cityInfoPanel.UpdateCityPanelInfo();
        }
    }

    public void BuildOnHex(Hex hex, String buildingType)
    {
        if(Global.gameManager.game.mainGameBoard.gameHexDict[hex].district == null)
        {
            District district = new District(Global.gameManager.game.mainGameBoard.gameHexDict[hex], buildingType, false, true, id);
            districts.Add(district);
        }
        else
        {
            bool isResource = false;
            if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].resourceType != ResourceType.None)
            {
                isResource = true;
            }
            Building building = new Building(buildingType, Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.hex, isResource);
            Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.AddBuilding(building);
            Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.isUrban = true;
        }
    }

    public void BuildDefenseOnHex(Hex hex, Building building)
    {
        if(Global.gameManager.game.mainGameBoard.gameHexDict[hex].district != null)
        {
            Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.AddDefense(building);
            building.districtHex = Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.hex;
        }
    }

    public void ExpandToHex(Hex hex)
    {
        if(readyToExpand > 0)
        {
            District district = new District(Global.gameManager.game.mainGameBoard.gameHexDict[hex], false, false, id);
            districts.Add(district);
            readyToExpand -= 1;
            RecalculateYields();
            if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
            {
                manager.UpdateHex(hex);
                manager.UpdateGraphic(id, GraphicUpdateType.Update);
                manager.ClearWaitForTarget();
            }
        }
        else
        {
            GD.PushWarning("tried to expand without readyToExpand > 0");
        }
    }

    public void DevelopDistrict(Hex hex, DistrictType districtType)
    {
        District targetDistrict = Global.gameManager.game.mainGameBoard.gameHexDict[hex].district;
        if (targetDistrict != null)
        {
            if(targetDistrict.cityID == this.id)
            {
                targetDistrict.DevelopDistrict(districtType);
                readyToExpand -= 1;
                RecalculateYields();
                if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
                {
                    manager.ClearWaitForTarget();
                }
            }
        }
    }

    public bool ValidExpandHex(List<TerrainType> validTerrain, GameHex targetGameHex)
    {
        if (validTerrain.Count == 0 || validTerrain.Contains(targetGameHex.terrainType))
        {
            //hex is owned by us so continue
            if (targetGameHex.ownedBy == -1 | targetGameHex.ownedBy == teamNum)
            {
                //hex does not have a district
                if (targetGameHex.district == null)
                {
                    //hex doesnt have a enemy unit
                    bool noEnemyUnit = true;
                    foreach (int unitID in targetGameHex.units)
                    {
                        Unit unit = Global.gameManager.game.unitDictionary[unitID];
                        if (Global.gameManager.game.teamManager.GetEnemies(teamNum).Contains(unit.teamNum))
                        {
                            noEnemyUnit = false;
                            break;
                        }
                    }
                    bool adjacentDistrict = false;
                    foreach (Hex hex in targetGameHex.hex.WrappingNeighbors(Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.bottom))
                    {
                        if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].district != null && Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.cityID == id)
                        {
                            adjacentDistrict = true;
                            break;
                        }
                    }
                    if (noEnemyUnit && adjacentDistrict)
                    {
                        return true;

                    }
                }
            }
        }
        return false;
    }
    //valid hexes for a rural district
    public List<Hex> ValidExpandHexes(List<TerrainType> validTerrain, int range = 3)
    {
        List<Hex> validHexes = new();
        //gather valid targets
        foreach(Hex hex in hex.WrappingRange(range, Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.top, Global.gameManager.game.mainGameBoard.bottom))
        {
            if(ValidExpandHex(validTerrain, Global.gameManager.game.mainGameBoard.gameHexDict[hex]))
            {
                validHexes.Add(hex);
            }
        }
        return validHexes;
    }
    

    public bool ValidUrbanBuildHex(List<TerrainType> validTerrain, GameHex targetGameHex, DistrictType districtType, string name="")
    {
        if (validTerrain.Count == 0 || validTerrain.Contains(targetGameHex.terrainType))
        {
            //hex is owned by us so continue
            if (targetGameHex.ownedBy == teamNum)
            {
                //hex has less than the max buildings and the right districtType
                if (targetGameHex.district != null && targetGameHex.district.districtType == districtType && targetGameHex.district.buildings.Count() < targetGameHex.district.maxBuildings && targetGameHex.district.cityID == id)
                {
                    foreach(Building building in targetGameHex.district.buildings)
                    {
                        if(building.name == name)
                        {
                            return false;
                        }
                    }
                    //hex doesnt have a enemy unit
                    bool noEnemyUnit = true;
                    foreach (int unitID in targetGameHex.units)
                    {
                        Unit unit = Global.gameManager.game.unitDictionary[unitID];
                        if (Global.gameManager.game.teamManager.GetEnemies(teamNum).Contains(unit.teamNum))
                        {
                            noEnemyUnit = false;
                            break;
                        }
                    } 
                    if (noEnemyUnit)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    //valid hexes to build a building
    public List<Hex> ValidUrbanBuildHexes(List<TerrainType> validTerrain, DistrictType districtType, int range=3, string name="")
    {
        List<Hex> validHexes = new();
        //gather valid targets
        foreach(Hex hex in hex.WrappingRange(range, Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.top, Global.gameManager.game.mainGameBoard.bottom))
        {
            if(ValidUrbanBuildHex(validTerrain, Global.gameManager.game.mainGameBoard.gameHexDict[hex], districtType, name))
            {
                validHexes.Add(hex);
            }
        }
        return validHexes;
    }

    public bool ValidUrbanExpandHex(List<TerrainType> validTerrain, GameHex targetGameHex)
    {
        if (validTerrain.Count == 0 || validTerrain.Contains(targetGameHex.terrainType))
        {
            //hex is owned by us so continue
            if (targetGameHex.ownedBy == teamNum)
            {
                //hex does have a rural district
                if (targetGameHex.district != null && !targetGameHex.district.isUrban)
                {
                    //hex doesnt have a enemy unit
                    bool noEnemyUnit = true;
                    foreach (int unitID in targetGameHex.units)
                    {
                        Unit unit = Global.gameManager.game.unitDictionary[unitID];
                        if (Global.gameManager.game.teamManager.GetEnemies(teamNum).Contains(unit.teamNum))
                        {
                            noEnemyUnit = false;
                            break;
                        }
                    }
                    //have a valid district to build
                    bool validBuildableDistrict = false;
                    foreach (DistrictType districtType in Global.gameManager.game.playerDictionary[teamNum].allowedDistricts)
                    {
                        if (BuildingLoader.districtDict[districtType].TerrainTypes.Contains(targetGameHex.terrainType) && districtType != DistrictType.citycenter)
                        {
                            validBuildableDistrict = true;
                        }
                    }
                    if(!validBuildableDistrict)
                    {
                        return false;
                    }

                    //have an adjacent urban district
                    bool adjacentUrbanDistrict = false;
                    foreach (Hex hex in targetGameHex.hex.WrappingNeighbors(Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.bottom))
                    {
                        if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].district != null && Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.isUrban && Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.cityID == id)
                        {
                            adjacentUrbanDistrict = true;
                            break;
                        }
                    }
                    if (noEnemyUnit && adjacentUrbanDistrict)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    //valid hex to build an urban district or expand one
    public List<Hex> ValidUrbanExpandHexes(List<TerrainType> validTerrain, int range=3)
    {
        List<Hex> validHexes = new();
        //gather valid targets
        foreach (Hex hex in hex.WrappingRange(range, Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.top, Global.gameManager.game.mainGameBoard.bottom))
        {
            if (ValidUrbanExpandHex(validTerrain, Global.gameManager.game.mainGameBoard.gameHexDict[hex]))
            {
                validHexes.Add(hex);
            }
        }
        return validHexes;
    }

    public List<Hex> ValidDefensiveBuildHexes(List<TerrainType> validTerrain)
    {
        List<Hex> validHexes = new();
        //gather valid targets
        foreach(Hex hex in hex.WrappingRange(3, Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.top, Global.gameManager.game.mainGameBoard.bottom))
        {
            if(validTerrain.Contains(Global.gameManager.game.mainGameBoard.gameHexDict[hex].terrainType))
            {
                //hex is owned by us so continue
                if(Global.gameManager.game.mainGameBoard.gameHexDict[hex].ownedBy == teamNum)
                {
                    //hex has a district with less than the max defenses TODO
                    if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].district != null & Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.defenses.Count() < Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.maxDefenses && Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.cityID == id)
                    {
                        //hex doesnt have a non-friendly unit
                        bool valid = true;
                        foreach(int unitID in Global.gameManager.game.mainGameBoard.gameHexDict[hex].units)
                        {
                            Unit unit = Global.gameManager.game.unitDictionary[unitID];
                            if (!Global.gameManager.game.teamManager.GetAllies(teamNum).Contains(unit.teamNum))
                            {
                                valid = false;
                            }
                        }
                        if(valid)
                        {
                            validHexes.Add(hex);
                        }
                    }
                }
            }
        }
        return validHexes;
    }


    public void NewExport(YieldType exportType)
    {
        if(exportCount.Keys.Contains(exportType))
        {
            exportCount[exportType] += 1;
            Global.gameManager.game.playerDictionary[teamNum].RecalculateExportsFromCity(this.id);
        }
        else
        {
            exportCount.Add(exportType, 1);
        }
        RecalculateYields();
    }
    public void RemoveExport(YieldType exportType)
    {
        if (exportCount.Keys.Contains(exportType) && exportCount[exportType] > 1)
        {
            exportCount[exportType] = 1;
        }
        else
        {
            exportCount.Remove(exportType);
        }
    }

    public void RenameCity(string name)
    {
        this.name = name;
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
        {
            manager.UpdateGraphic(id, GraphicUpdateType.Update);
            manager.uiManager.cityInfoPanel.UpdateCityPanelInfo();
        }
        
    }
    // For terrain types
    public void AddFlatYields(GameHex gameHex)
    {
        gameHex.yields += flatYields;
    }
    
    public void AddRoughYields(GameHex gameHex)
    {
        gameHex.yields += roughYields;
    }
    
    public void AddMountainYields(GameHex gameHex)
    {
        gameHex.yields += mountainYields;
    }
    
    public void AddCoastYields(GameHex gameHex)
    {
        gameHex.yields += coastalYields;
    }
    
    public void AddOceanYields(GameHex gameHex)
    {
        gameHex.yields += oceanYields;
    }
    
    // For terrain temperatures
    public void AddDesertYields(GameHex gameHex)
    {
        gameHex.yields += desertYields;
    }
    
    public void AddPlainsYields(GameHex gameHex)
    {
        gameHex.yields += plainsYields;
    }
    
    public void AddGrasslandYields(GameHex gameHex)
    {
        gameHex.yields += grasslandYields;
    }
    
    public void AddTundraYields(GameHex gameHex)
    {
        gameHex.yields += tundraYields;
    }
    
    public void AddArcticYields(GameHex gameHex)
    {
        gameHex.yields += arcticYields;
    }
}

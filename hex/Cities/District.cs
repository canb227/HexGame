using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data;
using Godot;
using System.Text;
using System.IO;
using System.Security.AccessControl;

[Serializable]
public partial class District
{

    public int id { get; set; }
    public List<Building> buildings { get; set; }
    public List<Building> defenses { get; set; }
    public DistrictType districtType { get; set; }
    public Hex hex { get; set; }
    public bool isCityCenter { get; set; }
    public bool isUrban { get; set; }
    public bool hasWalls { get; set; }
    public int cityID { get; set; }
    public List<Hex> visibleHexes { get; set; } = new();
    public float health { get; set; } = 0.0f;
    public float maxHealth{ get; set; } = 0.0f;
    public int maxBuildings { get; set; }  = 99;
    public int maxDefenses { get; set; } = 1;
    public int turnsUntilHealing { get; set; } = 0;

    public District(GameHex gameHex, String initialString, bool isCityCenter, bool isUrban, int cityID, bool isEncampment = false)
    {
        SetupDistrict(gameHex, isCityCenter, isUrban, cityID, !isEncampment);
        bool isResource = false;
        if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].resourceType != ResourceType.None)
        {
            isResource = true;
        }
        if(isCityCenter)
        {
            AddBuilding(new Building(initialString, hex, false));
        }
        else
        {
            AddBuilding(new Building(initialString, hex, isResource));
        }
        var data = new Godot.Collections.Dictionary
                {
                    { "q", hex.q },
                    { "r", hex.r },
                    { "s", hex.s }
                };
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager)) manager.CallDeferred("UpdateHex", data);
    }

    public District(GameHex gameHex, bool isCityCenter, bool isUrban, int cityID, bool isEncampment = false)
    {
        SetupDistrict(gameHex, isCityCenter, isUrban, cityID, !isEncampment);
    }

    public District()
    {

    }
    protected void SetupDistrict(GameHex gameHex, bool isCityCenter, bool isUrban, int cityID, bool claimSurronding = true)
    {
        id = Global.gameManager.game.GetUniqueID(Global.gameManager.game.cityDictionary[cityID].teamNum);
        this.cityID = cityID;
        buildings = new();
        defenses = new();
        this.hex = gameHex.hex;


        Global.gameManager.game.mainGameBoard.gameHexDict[hex].ClaimHex(Global.gameManager.game.cityDictionary[cityID]);
        Global.gameManager.game.mainGameBoard.gameHexDict[hex].district = this;
        if(claimSurronding)
        {
            foreach (Hex hex in gameHex.hex.WrappingNeighbors(Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.bottom))
            {
                Global.gameManager.game.mainGameBoard.gameHexDict[hex].TryClaimHex(Global.gameManager.game.cityDictionary[cityID]);
            }
        }

        this.isCityCenter = isCityCenter;
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
        {
            var data = new Godot.Collections.Dictionary
                {
                    { "q", hex.q },
                    { "r", hex.r },
                    { "s", hex.s }
                };
            manager.CallDeferred("NewDistrict", data);
        }
        if (isCityCenter)
        {
            AddResource();
            maxHealth = 125.0f;
            health = 125.0f;
            districtType = DistrictType.citycenter;
        }
        else
        {
            districtType = DistrictType.rural;
            AddBuilding(new Building("RuralDistrict", hex, false));
        }
        this.isUrban = isUrban;
        if(isUrban)
        {
            maxBuildings += 1;
            Global.gameManager.game.mainGameBoard.gameHexDict[hex].AddTerrainFeature(FeatureType.Road);
        }

        Global.gameManager.game.cityDictionary[cityID].RecalculateYields();
        AddVision();

    }

    public void BeforeSwitchTeam()
    {
        RemoveVision();
        RemoveLostResource();        
    }
    
    public void AfterSwitchTeam()
    {      
        foreach(Building building in buildings)
        {
            building.SwitchTeams();
        }
        foreach(Building building in defenses)
        {
            building.SwitchTeams();
        }
        AddVision();
        AddResource();
    }

    public void Raze()
    {
        RemoveVision();
        RemoveLostResource();
        foreach (Building building in buildings)
        {
            building.Raze();
        }
        buildings.Clear();
        Global.gameManager.game.mainGameBoard.gameHexDict[hex].district = null;
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
        {
            manager.CallDeferred("UpdateGraphic", id, (int)GraphicUpdateType.Remove);
        }
    }

    public bool decreaseHealth(float amount)
    {
        health -= amount;
        health = Math.Max(0.0f, health);
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
        {
            manager.CallDeferred("UpdateGraphic", Global.gameManager.game.cityDictionary[cityID].id, (int)GraphicUpdateType.Update);
        }
        if (health <= 0.0f)
        {
            Global.gameManager.game.cityDictionary[cityID].DistrictFell();
            turnsUntilHealing = 5;
            return true;
        }
        return false;
    }

    public float GetCombatStrength()
    {
        float strength = Global.gameManager.game.playerDictionary[Global.gameManager.game.cityDictionary[cityID].teamNum].strongestUnitBuilt + Global.gameManager.game.playerDictionary[Global.gameManager.game.cityDictionary[cityID].teamNum].cityCombatStrengthMod;
        if (hasWalls)
        {
            strength += 15.0f;
        }
        return strength;
    }

    public void DestroyDistrict()
    {
        RemoveVision();
        RemoveLostResource();
        Global.gameManager.game.cityDictionary[cityID].districts.Remove(this);
        Global.gameManager.game.mainGameBoard.gameHexDict[hex].district = null;
        foreach(Building building in buildings)
        {
            building.DestroyBuilding();
        }
        foreach(Building building in defenses)
        {
            building.DestroyBuilding();
        }
    }

    public void HealForTurn(float healAmount)
    {
        if(turnsUntilHealing <= 0)
        {
            health += healAmount;
            health = Math.Min(health, maxHealth);
        }
        else if(turnsUntilHealing > 0)
        {
            if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].units.Any())
            {
                Unit unit = Global.gameManager.game.unitDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[hex].units[0]];
                if (Global.gameManager.game.teamManager.GetEnemies(Global.gameManager.game.cityDictionary[cityID].teamNum).Contains(unit.teamNum))
                {
                    turnsUntilHealing += 1;
                }
            }
            turnsUntilHealing -= 1;
        }
    }

    public void AddWalls(float wallStrength)
    {
        if(health >= maxHealth)
        {
            maxHealth += wallStrength;
            health += wallStrength;
            hasWalls = true;
        }
    }

    public void DevelopDistrict(DistrictType districtType)
    {
        maxBuildings += 99;
        this.districtType = districtType;
        isUrban = true;
        if (districtType == DistrictType.town)
        {
            AddBuilding(new Building("TownDistrict", hex, false));
        }
        else if (districtType == DistrictType.refinement)
        {
            AddBuilding(new Building("RefineryDistrict", hex, false));
        }
        else if (districtType == DistrictType.production)
        {
            AddBuilding(new Building("IndustryDistrict", hex, false));
        }
        else if (districtType == DistrictType.gold)
        {
            AddBuilding(new Building("CommerceDistrict", hex, false));
        }
        else if (districtType == DistrictType.science)
        {
            AddBuilding(new Building("CampusDistrict", hex, false));
        }
        else if (districtType == DistrictType.culture)
        {
            AddBuilding(new Building("CulturalDistrict", hex, false));
        }
        else if (districtType == DistrictType.happiness)
        {
            AddBuilding(new Building("EntertainmentDistrict", hex, false));
        }
        else if (districtType == DistrictType.heroic)
        {
            AddBuilding(new Building("HeroicDistrict", hex, false));
        }
        else if (districtType == DistrictType.dock)
        {
            AddBuilding(new Building("HarborDistrict", hex, false));
        }
        else if (districtType == DistrictType.military)
        {
            AddBuilding(new Building("MilitaristicDistrict", hex, false));
        }
        //remove any improvements
        foreach (Building building in buildings.ToList())
        {
            if (building.name == "Farm" || building.name == "Mine" || building.name == "Lumberyard" || building.name == "Pasture" || building.name == "FishingBoat" || building.name == "RuralDistrict")
            {
                RemoveBuilding(building);
            }
        }
    }

    public int CountString(String buildingType)
    {
        int count = 0;
        foreach(Building building in buildings)
        {
            if(building.buildingType == buildingType)
            {
                count += 1;
            }
        }
        return count;
    }
    
    public void RecalculateYields()
    {
        Global.gameManager.game.mainGameBoard.gameHexDict[hex].RecalculateYields();
        foreach(Building building in buildings)
        {
            building.RecalculateYields();
        }
    }

    public void PrepareYieldRecalculate()
    {
        if (buildings.Any())
        {
            foreach (Building building in buildings)
            {
                building.PrepareYieldRecalculate();
            }
        }
    }

    public void RemoveBuilding(Building building)
    {
        buildings.Remove(building);
        building.Raze();
    }

    public void AddBuilding(Building building)
    {
        if(buildings.Count() < maxBuildings)
        {
            buildings.Add(building);
            //Global.gameManager.game.cityDictionary[cityID].citySize += 1;
            Global.gameManager.game.cityDictionary[cityID].RecalculateYields();
        }
    }

    public void AddDefense(Building building)
    {
        if(defenses.Count() < maxDefenses)
        {
            defenses.Add(building);
            Global.gameManager.game.cityDictionary[cityID].RecalculateYields();
        }
    }

    public void UpdateVision()
    {
        RemoveVision(false);
        AddVision(false);
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager)) manager.CallDeferred("UpdateGraphic", Global.gameManager.game.mainGameBoard.id, (int)GraphicUpdateType.Update);
    }

    public void RemoveVision(bool updateGraphic = true)
    {
        foreach (int teamNum in Global.gameManager.game.teamManager.GetAllies(Global.gameManager.game.cityDictionary[cityID].teamNum))
        {
            foreach (Hex hex in visibleHexes)
            {
                int count;
                if (Global.gameManager.game.playerDictionary[Global.gameManager.game.cityDictionary[cityID].teamNum].visibleGameHexDict.TryGetValue(hex, out count))
                {
                    if (count <= 1)
                    {
                        Global.gameManager.game.playerDictionary[Global.gameManager.game.cityDictionary[cityID].teamNum].visibleGameHexDict.Remove(hex);
                        Global.gameManager.game.playerDictionary[Global.gameManager.game.cityDictionary[cityID].teamNum].visibilityChangedList.Add(hex);
                    }
                    else
                    {
                        Global.gameManager.game.playerDictionary[Global.gameManager.game.cityDictionary[cityID].teamNum].visibleGameHexDict[hex] = count - 1;
                    }
                }
            }
        }
        foreach (Hex hex in visibleHexes)
        {
            int count;
            if (Global.gameManager.game.playerDictionary[Global.gameManager.game.cityDictionary[cityID].teamNum].personalVisibleGameHexDict.TryGetValue(hex, out count))
            {
                if (count <= 1)
                {
                    Global.gameManager.game.playerDictionary[Global.gameManager.game.cityDictionary[cityID].teamNum].personalVisibleGameHexDict.Remove(hex);
                }
                else
                {
                    Global.gameManager.game.playerDictionary[Global.gameManager.game.cityDictionary[cityID].teamNum].personalVisibleGameHexDict[hex] = count - 1;
                }
            }
        }
        visibleHexes.Clear();
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager) && updateGraphic) manager.CallDeferred("UpdateGraphic", Global.gameManager.game.mainGameBoard.id, (int)GraphicUpdateType.Update);
    }
    public void AddVision(bool updateGraphic = true)
    {
        foreach (int teamNum in Global.gameManager.game.teamManager.GetAllies(Global.gameManager.game.cityDictionary[cityID].teamNum))
        {
            if (isCityCenter)
            {
                foreach (Hex hex in hex.WrappingRange(2, Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.top, Global.gameManager.game.mainGameBoard.bottom))
                {
                    Global.gameManager.game.playerDictionary[Global.gameManager.game.cityDictionary[cityID].teamNum].seenGameHexDict.TryAdd(hex, true);
                    int count;
                    if (Global.gameManager.game.playerDictionary[Global.gameManager.game.cityDictionary[cityID].teamNum].visibleGameHexDict.TryGetValue(hex, out count))
                    {
                        Global.gameManager.game.playerDictionary[Global.gameManager.game.cityDictionary[cityID].teamNum].visibleGameHexDict[hex] = count + 1;
                    }
                    else
                    {
                        Global.gameManager.game.playerDictionary[Global.gameManager.game.cityDictionary[cityID].teamNum].visibleGameHexDict.TryAdd(hex, 1);
                        Global.gameManager.game.playerDictionary[Global.gameManager.game.cityDictionary[cityID].teamNum].visibilityChangedList.Add(hex);
                    }
                }
            }
            else
            {
                visibleHexes = hex.WrappingNeighbors(Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.bottom).ToList();
                foreach (Hex hex in visibleHexes)
                {
                    Global.gameManager.game.playerDictionary[Global.gameManager.game.cityDictionary[cityID].teamNum].seenGameHexDict.TryAdd(hex, true); //add to the seen dict no matter what since duplicates are thrown out
                    int count;
                    if (Global.gameManager.game.playerDictionary[Global.gameManager.game.cityDictionary[cityID].teamNum].visibleGameHexDict.TryGetValue(hex, out count))
                    {
                        Global.gameManager.game.playerDictionary[Global.gameManager.game.cityDictionary[cityID].teamNum].visibleGameHexDict[hex] = count + 1;
                    }
                    else
                    {
                        Global.gameManager.game.playerDictionary[Global.gameManager.game.cityDictionary[cityID].teamNum].visibleGameHexDict.TryAdd(hex, 1);
                        Global.gameManager.game.playerDictionary[Global.gameManager.game.cityDictionary[cityID].teamNum].visibilityChangedList.Add(hex);
                    }
                }
            }
        }
        if (isCityCenter)
        {
            foreach (Hex hex in hex.WrappingRange(2, Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.top, Global.gameManager.game.mainGameBoard.bottom))
            {
                int count;
                if (Global.gameManager.game.playerDictionary[Global.gameManager.game.cityDictionary[cityID].teamNum].personalVisibleGameHexDict.TryGetValue(hex, out count))
                {
                    Global.gameManager.game.playerDictionary[Global.gameManager.game.cityDictionary[cityID].teamNum].personalVisibleGameHexDict[hex] = count + 1;
                }
                else
                {
                    Global.gameManager.game.playerDictionary[Global.gameManager.game.cityDictionary[cityID].teamNum].personalVisibleGameHexDict.TryAdd(hex, 1);
                }
            }
        }
        else
        {
            visibleHexes = hex.WrappingNeighbors(Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.bottom).ToList();
            foreach (Hex hex in visibleHexes)
            {
                int count;
                if (Global.gameManager.game.playerDictionary[Global.gameManager.game.cityDictionary[cityID].teamNum].personalVisibleGameHexDict.TryGetValue(hex, out count))
                {
                    Global.gameManager.game.playerDictionary[Global.gameManager.game.cityDictionary[cityID].teamNum].personalVisibleGameHexDict[hex] = count + 1;
                }
                else
                {
                    Global.gameManager.game.playerDictionary[Global.gameManager.game.cityDictionary[cityID].teamNum].personalVisibleGameHexDict.TryAdd(hex, 1);
                }
            }
        }
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager) && updateGraphic) manager.CallDeferred("UpdateGraphic", Global.gameManager.game.mainGameBoard.id, (int)GraphicUpdateType.Update);
    }

    public void AddResource()
    {
        if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].resourceType != ResourceType.None)
        {
            if (Global.gameManager.game.localPlayerRef.hiddenResources.Contains(Global.gameManager.game.mainGameBoard.gameHexDict[hex].resourceType))
            {
                Global.gameManager.game.playerDictionary[Global.gameManager.game.cityDictionary[cityID].teamNum].hiddenGlobalResources.Add(hex, Global.gameManager.game.mainGameBoard.gameHexDict[hex].resourceType);
            }
            else if (ResourceLoader.resources[Global.gameManager.game.mainGameBoard.gameHexDict[hex].resourceType].IsGlobal)
            {
                Global.gameManager.game.playerDictionary[Global.gameManager.game.cityDictionary[cityID].teamNum].globalResources.Add(hex, Global.gameManager.game.mainGameBoard.gameHexDict[hex].resourceType);
            }
            else
            {
                Global.gameManager.game.playerDictionary[Global.gameManager.game.cityDictionary[cityID].teamNum].unassignedResources.Add(hex, Global.gameManager.game.mainGameBoard.gameHexDict[hex].resourceType);
                if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
                {
                    if(Global.gameManager.game.cityDictionary[cityID].teamNum == Global.gameManager.game.localPlayerTeamNum)
                    {
                        manager.uiManager.assignResource = true;
                    }
                }
            }
        }
    }

    public void RemoveLostResource()
    {
        if(buildings.Any())
        {
            foreach(Building building in buildings)
            {
                
            }
        }
        Global.gameManager.game.playerDictionary[Global.gameManager.game.cityDictionary[cityID].teamNum].RemoveLostResource(hex);
    }
}

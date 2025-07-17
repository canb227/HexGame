using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data;
using System.IO;
using Godot;

public enum TerrainType
{
    Ocean,
    Coast,
    Flat,
    Rough,
    Mountain,
}

public enum TerrainTemperature
{
    Desert,
    Plains,
    Grassland,
    Tundra,
    Arctic
}

public enum FeatureType
{
    Forest,
    River,
    Road,
    Coral,
    Wetland,
    Fortification,
    None
}

[Serializable]
public class GameHex
{
    public GameHex(Hex hex, int gameBoardID, TerrainType terrainType, TerrainTemperature terrainTemp, ResourceType resourceType, HashSet<FeatureType> featureSet, List<int> units, District district)
    {
        this.hex = hex;
        this.gameBoardID = gameBoardID;
        this.terrainType = terrainType; 
        this.terrainTemp = terrainTemp;
        this.featureSet = featureSet;
        this.resourceType = resourceType;
        this.units = units;
        this.district = district;
        this.ownedBy = -1;
        this.withinCityRange = 0;
        this.rangeToNearestCity = -1;
        RecalculateYields();
    }

    public Hex hex { get; set; }
    public int gameBoardID { get; set; }
    public TerrainType terrainType { get; set; }
    public TerrainTemperature terrainTemp { get; set; }
    public ResourceType resourceType { get; set; }
    public int ownedBy { get; set; }
    public int owningCityID { get; set; }
    public HashSet<FeatureType> featureSet { get; set; } = new();
    public List<int> units { get; set; } = new();
    public District? district { get; set; }
    public int withinCityRange { get; set; } = 0;
    public int rangeToNearestCity { get; set; } = 9;

    public Yields yields { get; set; }

    public void RecalculateYields()
    {
        yields = new();
        
        if (Global.gameManager.game.cityDictionary.Keys.Contains(owningCityID))
        {
            //calculate the rural value
            if(terrainType == TerrainType.Flat)
            {
                Global.gameManager.game.cityDictionary[owningCityID].AddFlatYields(this);
            }
            else if (terrainType == TerrainType.Rough)
            {
                Global.gameManager.game.cityDictionary[owningCityID].AddRoughYields(this);
            }
            else if (terrainType == TerrainType.Mountain)
            {
                Global.gameManager.game.cityDictionary[owningCityID].AddMountainYields(this);
            }
            else if (terrainType == TerrainType.Coast)
            {
                Global.gameManager.game.cityDictionary[owningCityID].AddCoastYields(this);
            }
            else if (terrainType == TerrainType.Ocean)
            {
                Global.gameManager.game.cityDictionary[owningCityID].AddOceanYields(this);
            }
            
            if(terrainTemp == TerrainTemperature.Desert)
            {
                Global.gameManager.game.cityDictionary[owningCityID].AddDesertYields(this);
            }
            else if (terrainTemp == TerrainTemperature.Plains)
            {
                Global.gameManager.game.cityDictionary[owningCityID].AddPlainsYields(this);
            }
            else if (terrainTemp == TerrainTemperature.Grassland)
            {
                Global.gameManager.game.cityDictionary[owningCityID].AddGrasslandYields(this);
            }
            else if (terrainTemp == TerrainTemperature.Tundra)
            {
                Global.gameManager.game.cityDictionary[owningCityID].AddTundraYields(this);
            }
            else
            {
                Global.gameManager.game.cityDictionary[owningCityID].AddArcticYields(this);
            }
            if(featureSet.Contains(FeatureType.Forest))
            {
                Global.gameManager.game.cityDictionary[owningCityID].AddForestYields(this);
            }
            if(featureSet.Contains(FeatureType.Coral))
            {
                Global.gameManager.game.cityDictionary[owningCityID].AddCoralYields(this);
            }
            if (featureSet.Contains(FeatureType.Wetland))
            {
                Global.gameManager.game.cityDictionary[owningCityID].AddWetlandYields(this);
            }
        }
        else
        {
            SetUnownedHexYields();
        }
    }

    public void SetUnownedHexYields()
    {
        yields = new();
        if(Global.gameManager.game.localPlayerRef != null)
        {
            if (terrainType == TerrainType.Flat)
            {
                yields += Global.gameManager.game.localPlayerRef.flatYields;
            }
            else if (terrainType == TerrainType.Rough)
            {
                yields += Global.gameManager.game.localPlayerRef.roughYields;
            }
            else if (terrainType == TerrainType.Mountain)
            {
                yields += Global.gameManager.game.localPlayerRef.mountainYields;
            }
            else if (terrainType == TerrainType.Coast)
            {
                yields += Global.gameManager.game.localPlayerRef.coastalYields;
            }
            else if (terrainType == TerrainType.Ocean)
            {
                yields += Global.gameManager.game.localPlayerRef.oceanYields;
            }

            if (terrainType != TerrainType.Mountain)
            {
                if (terrainTemp == TerrainTemperature.Desert)
                {
                    yields += Global.gameManager.game.localPlayerRef.desertYields;
                }
                else if (terrainTemp == TerrainTemperature.Plains)
                {
                    yields += Global.gameManager.game.localPlayerRef.plainsYields;
                }
                else if (terrainTemp == TerrainTemperature.Grassland)
                {
                    yields += Global.gameManager.game.localPlayerRef.grasslandYields;
                }
                else if (terrainTemp == TerrainTemperature.Tundra)
                {
                    yields += Global.gameManager.game.localPlayerRef.tundraYields;
                }
                else
                {
                    //nothing
                }
            }
            if (featureSet.Contains(FeatureType.Forest))
            {
                yields += Global.gameManager.game.localPlayerRef.forestYields;
            }
            if (featureSet.Contains(FeatureType.Coral))
            {
                yields += Global.gameManager.game.localPlayerRef.coralYields;
            }
            if (featureSet.Contains(FeatureType.Wetland))
            {
                yields += Global.gameManager.game.localPlayerRef.wetlandYields;
            }
        }
    }

    public void OnTurnStarted(int turnNumber)
    {

    }

    public void OnTurnEnded(int turnNumber)
    {

    }
    public bool SetTerrainType(TerrainType newTerrainType)
    {
        this.terrainType = newTerrainType;
        return true;
    }

    public bool AddTerrainFeature(FeatureType newFeature)
    {
        this.featureSet.Add(newFeature);
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
        {
            var data = new Godot.Collections.Dictionary
                {
                    { "q", hex.q },
                    { "r", hex.r },
                    { "s", hex.s }
                };

            manager.CallDeferred("NewFeature", data, (int)newFeature);
        }
        return true;
    }

    public void ClaimHex(City city)
    {
        ownedBy = city.teamNum;
        owningCityID = city.id;
        city.heldHexes.Add(hex);
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
        {
            //manager.CallDeferred("UpdateCityTerritory", city);
        }
    }

    public bool TryClaimHex(City city)
    {
        if(ownedBy == -1 && hex.WrapDistance(city.hex) <= 3)
        {
            ClaimHex(city);
            return true;
        }
        return false;
    }

    public bool IsEnemyPresent(int yourTeamNum)
    {
        bool isEnemy = false;
        foreach (int unitID in Global.gameManager.game.mainGameBoard.gameHexDict[hex].units)
        {
            Unit targetHexUnit = Global.gameManager.game.unitDictionary[unitID];
            if (Global.gameManager.game.teamManager.GetEnemies(yourTeamNum).Contains(targetHexUnit.teamNum))
            {
                isEnemy = true;
                break;
            }
        }                                                                           
        if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].district != null && Global.gameManager.game.teamManager.GetEnemies(yourTeamNum).Contains(Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.cityID].teamNum))
        {
            if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.health > 0)
            {
                isEnemy = true;
            }
        }
        return isEnemy;
    }

    //if stackable is true allow multiple units to stack
    //if flexible is true look for adjacent spaces to place
    public bool SpawnUnit(Unit newUnit, bool stackable, bool flexible)
    {
        if((!stackable && units.Any()) || newUnit.movementCosts[(TerrainMoveType)terrainType] > 100 || newUnit.movementCosts[(TerrainMoveType)terrainType] < 0) //if they cant stack and their are units or the hex is invalid for this unit
        {
            if (flexible)
            {
                foreach(Hex rangeHex in hex.WrappingRange(3, Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.top, Global.gameManager.game.mainGameBoard.bottom).OrderBy(h => hex.Distance(h)))
                {
                    if(Global.gameManager.game.mainGameBoard.gameHexDict[rangeHex].SpawnUnit(newUnit, stackable, false))
                    {
                        return true;
                    }
                }
                //if we still havent found a spot give up
            }
            return false;
        }
        else if(newUnit.movementCosts[(TerrainMoveType)terrainType] < 100 && newUnit.movementCosts[(TerrainMoveType)terrainType] >= 0)//if they cant stack and there aren't units or they can stack and units are/aren't there and the hex is valid for this unit
        {
            units.Add(newUnit.id);
            newUnit.SpawnSetup(this);
            return true;
        }
        return false;
    }

    public bool ValidHexToSpawn(UnitInfo newUnitInfo, bool stackable, bool flexible)
    {
        if ((!stackable & units.Any()) | newUnitInfo.MovementCosts[(TerrainMoveType)terrainType] > 100) //if they cant stack and their are units or the hex is invalid for this unit
        {
            if (flexible)
            {
                foreach (Hex rangeHex in hex.WrappingRange(3, Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.top, Global.gameManager.game.mainGameBoard.bottom).OrderBy(h => hex.Distance(h)))
                {
                    if (Global.gameManager.game.mainGameBoard.gameHexDict[rangeHex].ValidHexToSpawn(newUnitInfo, stackable, false))
                    {
                        return true;
                    }
                }
                //if we still havent found a spot give up
            }
            return false;
        }
        else if (newUnitInfo.MovementCosts[(TerrainMoveType)terrainType] < 100)//if they cant stack and there aren't units or they can stack and units are/aren't there and the hex is valid for this unit
        {
            return true;
        }
        return false;
    }

}

using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data;
using Godot;
using System.IO;
using NetworkMessages;
using Steamworks;

public enum UnitEffectType
{
    MovementSpeed,
    MovementCosts,
    SightRange,
    SightCosts,
    CombatStrength,
    MaintenanceCost,
}

public enum EffectOperation
{
    Multiply,
    Divide,
    Add,
    Subtract,
}
public class UnitEffect
{
    //priority is 0-100 (100 most important)
    public UnitEffect(UnitEffectType effectType, EffectOperation effectOperation, float effectMagnitude, int priority)
    {
        this.effectType = effectType;
        if(effectType == UnitEffectType.MovementCosts | effectType == UnitEffectType.SightCosts)
        {
            throw new InvalidOperationException("Must provide a TerrainMoveType if adjusting the movecost table");
        }
        this.effectOperation = effectOperation;
        this.effectMagnitude = effectMagnitude;
        this.priority = priority;
    }

    public UnitEffect(String functionName, int level=0)
    {
        this.effectLevel = level;
        this.functionName = functionName;
    }
    
    public UnitEffect()
    {
        //used for loading
    }

    public UnitEffectType effectType { get; set; }
    public EffectOperation effectOperation { get; set; }
    public TerrainMoveType terrainMoveType { get; set; }
    public float effectMagnitude { get; set; } = 0f;
    public int priority { get; set; } = 0;
    public String functionName { get; set; } = "";
    public int effectLevel { get; set; } = 0;


    public bool Apply(int unitID, int level = 0, float combatPower = 0.0f, GameHex abilityTarget = null)
    {
        if(level == 0)
        {
            level = effectLevel;
        }
        if (functionName != "")
        {
            return ProcessFunctionString(functionName, unitID, level, combatPower, abilityTarget);
        }
        else
        {
            if(effectType == UnitEffectType.MovementSpeed)
            {
                Global.gameManager.game.unitDictionary[unitID].movementSpeed = ApplyOperation(Global.gameManager.game.unitDictionary[unitID].movementSpeed);
            }
            else if(effectType == UnitEffectType.SightRange)
            {
                Global.gameManager.game.unitDictionary[unitID].sightRange = ApplyOperation(Global.gameManager.game.unitDictionary[unitID].sightRange);
            }
            else if(effectType == UnitEffectType.CombatStrength)
            {
                Global.gameManager.game.unitDictionary[unitID].combatStrength = ApplyOperation(Global.gameManager.game.unitDictionary[unitID].combatStrength);
            }
            else if(effectType == UnitEffectType.MaintenanceCost)
            {
                Global.gameManager.game.unitDictionary[unitID].maintenanceCost = ApplyOperation(Global.gameManager.game.unitDictionary[unitID].maintenanceCost);
            }
            else if(effectType == UnitEffectType.MovementCosts)
            {
                switch (effectOperation)
                {
                    case EffectOperation.Multiply:
                        Global.gameManager.game.unitDictionary[unitID].movementCosts[terrainMoveType] *= effectMagnitude;
                        break;
                    case EffectOperation.Divide:
                        Global.gameManager.game.unitDictionary[unitID].movementCosts[terrainMoveType] /= effectMagnitude;
                        break;
                    case EffectOperation.Add:
                        Global.gameManager.game.unitDictionary[unitID].movementCosts[terrainMoveType] += effectMagnitude;
                        break;
                    case EffectOperation.Subtract:
                        Global.gameManager.game.unitDictionary[unitID].movementCosts[terrainMoveType] -= effectMagnitude;
                        break;
                }
            }
            else if(effectType == UnitEffectType.SightCosts)
            {
                switch (effectOperation)
                {
                    case EffectOperation.Multiply:
                        Global.gameManager.game.unitDictionary[unitID].sightCosts[terrainMoveType] *= effectMagnitude;
                        break;
                    case EffectOperation.Divide:
                        Global.gameManager.game.unitDictionary[unitID].sightCosts[terrainMoveType] /= effectMagnitude;
                        break;
                    case EffectOperation.Add:
                        Global.gameManager.game.unitDictionary[unitID].sightCosts[terrainMoveType] += effectMagnitude;
                        break;
                    case EffectOperation.Subtract:
                        Global.gameManager.game.unitDictionary[unitID].sightCosts[terrainMoveType] -= effectMagnitude;
                        break;
                }
            }
            return true;
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
    bool ProcessFunctionString(String functionString, int unitID, int level, float combatPower, GameHex abilityTarget)
    {
        //effects

        //abilities
        if (functionString == "SettleCapitalAbility")
        {
            return SettleCapitalAbility(Global.gameManager.game.unitDictionary[unitID], "CapitalCityName");
        }
        else if (functionString == "SettleCityAbility")
        {
            return SettleCity(Global.gameManager.game.unitDictionary[unitID], "SettledCityName");
        }
        else if (functionString == "ScoutVisionAbility")
        {
            Global.gameManager.game.unitDictionary[unitID].sightRange += 1;
            Global.gameManager.game.unitDictionary[unitID].UpdateVision();
            return true;
        }
        else if (functionString == "RangedAttack")
        {
            return RangedAttack(Global.gameManager.game.unitDictionary[unitID], combatPower, abilityTarget);
        }
        else if (functionString == "BombardAttack")
        {
            return BombardAttack(Global.gameManager.game.unitDictionary[unitID], combatPower, abilityTarget);
        }
        else if (functionString == "EnableEmbarkDisembark")
        {
            EnableEmbarkDisembark(Global.gameManager.game.unitDictionary[unitID]);
            return true;
        }
        else if (functionString == "Fortify")
        {
            Fortify(Global.gameManager.game.unitDictionary[unitID]);
            return true;
        }
        else if (functionString == "BuildFarm")
        {
            BuildFarm(Global.gameManager.game.unitDictionary[unitID]);
            return true;
        }
        else if (functionString == "BuildMine")
        {
            BuildMine(Global.gameManager.game.unitDictionary[unitID]);
            return true;
        }
        else if (functionString == "BuildPasture")
        {
            BuildPasture(Global.gameManager.game.unitDictionary[unitID]);
            return true;
        }
        else if (functionString == "BuildLumberyard")
        {
            BuildLumberyard(Global.gameManager.game.unitDictionary[unitID]);
            return true;
        }
        else if (functionString == "BuildFishingBoat")
        {
            BuildFishingBoat(Global.gameManager.game.unitDictionary[unitID]);
            return true;
        }
        else if (functionString == "RemoveImprovement")
        {
            RemoveImprovement(Global.gameManager.game.unitDictionary[unitID]);
            return true;
        }
        else if (functionString == "Sleep")
        {
            Sleep(Global.gameManager.game.unitDictionary[unitID]);
            return true;
        }
        else if (functionString == "Skip")
        {
            Skip(Global.gameManager.game.unitDictionary[unitID]);
            return true;
        }
        else if (functionString == "ExploreRuin")
        {
            ExploreRuin(Global.gameManager.game.unitDictionary[unitID]);
            return true;
        }
        else if (functionString == "Trade")
        {
            Trade(Global.gameManager.game.unitDictionary[unitID]);
            return true;
        }
        else if (functionString == "EnableOceanMovement")
        {
            EnableOceanMovement(Global.gameManager.game.unitDictionary[unitID]);
            return true;
        }
        //hero abilities
        else if (functionString == "Fireball")
        {
            return Fireball(Global.gameManager.game.unitDictionary[unitID], level, abilityTarget);
        }
        else if (functionString == "Blink")
        {
            return Blink(Global.gameManager.game.unitDictionary[unitID], level, abilityTarget);
        }
        else if (functionString == "Banish")
        {
            return Banish(Global.gameManager.game.unitDictionary[unitID], level, abilityTarget);
        }
        else if (functionString == "Chop")
        {
            return Chop(Global.gameManager.game.unitDictionary[unitID], level, abilityTarget);
        }
        else if (functionString == "Leap")
        {
            return Leap(Global.gameManager.game.unitDictionary[unitID], level, abilityTarget);
        }
        else if (functionString == "ForTheHorde")
        {
            return ForTheHorde(Global.gameManager.game.unitDictionary[unitID], level, abilityTarget);
        }
        else if (functionString == "PinShot")
        {
            return PinShot(Global.gameManager.game.unitDictionary[unitID], level, abilityTarget);
        }
        else if (functionString == "Recall")
        {
            return Recall(Global.gameManager.game.unitDictionary[unitID], level, abilityTarget);
        }
        else if (functionString == "Onewiththeforest")
        {
            return Onewiththeforest(Global.gameManager.game.unitDictionary[unitID], level, abilityTarget);
        }
        else if (functionString == "ClawSwipe")
        {
            return ClawSwipe(Global.gameManager.game.unitDictionary[unitID], level, abilityTarget);
        }

        throw new NotImplementedException("The Effect Function: " + functionString + " does not exist, implement it in UnitEffect");
    }
    public bool SettleCapitalAbility(Unit unit, String cityName)
    {
        bool validHex = true;
        foreach (Hex hex in unit.hex.WrappingRange(3, Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.top, Global.gameManager.game.mainGameBoard.bottom))
        {
            if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].district != null && Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.isCityCenter)
            {
                validHex = false;
                break;
            }
        }
        //allow settle of resources
/*        if (Global.gameManager.game.mainGameBoard.gameHexDict[unit.hex].resourceType != ResourceType.None)
        {
            validHex = false;
        }*/
        if (validHex)
        {
            if (unit.unitType == "Founder")
            {
                Global.gameManager.game.playerDictionary[unit.teamNum].IncreaseAllSettlerCost();
            }

            //auto complete starting researches
            Global.gameManager.game.playerDictionary[unit.teamNum].OnResearchComplete("Agriculture");
            Global.gameManager.game.playerDictionary[unit.teamNum].OnCultureResearchComplete("TribalDominion");

            new City(Global.gameManager.game.GetUniqueID(unit.teamNum), unit.teamNum, cityName, true, Global.gameManager.game.mainGameBoard.gameHexDict[unit.hex]);
            unit.decreaseHealth(99999.0f);
            //auto spawn hero
            if(Global.gameManager.game.playerDictionary[unit.teamNum].faction == FactionType.Humans)
            {
                Global.gameManager.game.mainGameBoard.gameHexDict[unit.hex].SpawnUnit(new Hero("Arcana", 0, Global.gameManager.game.GetUniqueID(unit.teamNum), unit.teamNum), false, true);
            }
            if (Global.gameManager.game.playerDictionary[unit.teamNum].faction == FactionType.Orcs)
            {
                Global.gameManager.game.mainGameBoard.gameHexDict[unit.hex].SpawnUnit(new Hero("Gorb", 0, Global.gameManager.game.GetUniqueID(unit.teamNum), unit.teamNum), false, true);
            }
            if (Global.gameManager.game.playerDictionary[unit.teamNum].faction == FactionType.Elves)
            {
                Global.gameManager.game.mainGameBoard.gameHexDict[unit.hex].SpawnUnit(new Hero("Silvana", 0, Global.gameManager.game.GetUniqueID(unit.teamNum), unit.teamNum), false, true);
            }
            if (Global.gameManager.game.playerDictionary[unit.teamNum].faction == FactionType.Beastfolk)
            {
                Global.gameManager.game.mainGameBoard.gameHexDict[unit.hex].SpawnUnit(new Hero("Horkin", 0, Global.gameManager.game.GetUniqueID(unit.teamNum), unit.teamNum), false, true);
            }
            if (Global.gameManager.game.playerDictionary[unit.teamNum].faction == FactionType.Hobbits)
            {
                Global.gameManager.game.mainGameBoard.gameHexDict[unit.hex].SpawnUnit(new Hero("Billy", 0, Global.gameManager.game.GetUniqueID(unit.teamNum), unit.teamNum), false, true);
            }

            return true;
        }
        return false;
    }
    public void EnableEmbarkDisembark(Unit unit)
    {
        if(unit.movementCosts[TerrainMoveType.Embark] < 0)
        {
            unit.movementCosts[TerrainMoveType.Embark] = 0;
        }
        if(unit.movementCosts[TerrainMoveType.Disembark] < 0)
        {
            unit.movementCosts[TerrainMoveType.Disembark] = 0;
        }
        if (unit.movementCosts[TerrainMoveType.Coast] < 0)
        {
            unit.movementCosts[TerrainMoveType.Coast] = unit.movementCosts[TerrainMoveType.Coast] * -1;
        }
    }
    public bool SettleCity(Unit unit, String cityName)
    {
        if (unit.CanSettleHere(unit.hex, 3, new List<TerrainType>() { TerrainType.Flat, TerrainType.Rough }, false))
        {
            if (unit.unitType == "Settler")
            {
                Global.gameManager.game.playerDictionary[unit.teamNum].IncreaseAllSettlerCost();
            }
            new City(Global.gameManager.game.GetUniqueID(unit.teamNum), unit.teamNum, cityName, false, Global.gameManager.game.mainGameBoard.gameHexDict[unit.hex]);
            unit.decreaseHealth(99999.0f);
            return true;
        }
        return false;
    }
    public bool RangedAttack(Unit unit, float combatPower, GameHex target)
    {
        return unit.RangedAttackTarget(target, combatPower, Global.gameManager.game.teamManager);
    }
    public bool BombardAttack(Unit unit, float combatPower, GameHex target)
    {
        return unit.BombardAttackTarget(target, combatPower, Global.gameManager.game.teamManager);
    }
    public bool Fortify(Unit unit)
    {
        unit.isSleeping = true;
        unit.CancelMovement();
        unit.fortifying = true;
        if(unit.attacksLeft == unit.maxAttackCount && unit.remainingMovement == unit.movementSpeed)
        {
            unit.attacksLeft = 0;
            unit.remainingMovement = 0;
            unit.fortifyStrength = 3;
        }
        Global.gameManager.graphicManager.CallDeferred("UpdateGraphic", unit.id, (int)GraphicUpdateType.Update);
        Global.gameManager.graphicManager.CallDeferred("UnselectObject");
        return true;
    }

    public bool BuildFarm(Unit unit)
    {
        unit.remainingMovement = 0;
        unit.decreaseHealth(50);
        //find the district on this tile and add building to it, or something so we can "work on a building?"
        GameHex gameHex = Global.gameManager.game.mainGameBoard.gameHexDict[unit.hex];
        if(gameHex.district != null && Global.gameManager.game.teamManager.GetAllies(gameHex.ownedBy).Contains(unit.teamNum))
        {
            bool validDistrict = false;
            bool noBuilding = true;
            //must build on a rural district
            foreach(Building building in gameHex.district.buildings)
            {
                if(building.name == "RuralDistrict")
                {
                    validDistrict = true;
                    continue;
                }
                else
                {
                    noBuilding = false;
                    break;
                }
            }
            if(validDistrict && noBuilding)
            {
                gameHex.district.AddBuilding(new Building("Farm", gameHex.hex, Global.gameManager.game.mainGameBoard.gameHexDict[gameHex.hex].resourceType != ResourceType.None));
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }
    public bool BuildMine(Unit unit)
    {
        unit.remainingMovement = 0;
        unit.decreaseHealth(50);
        //find the district on this tile and add building to it, or something so we can "work on a building?"
        GameHex gameHex = Global.gameManager.game.mainGameBoard.gameHexDict[unit.hex];
        if (gameHex.district != null && Global.gameManager.game.teamManager.GetAllies(gameHex.ownedBy).Contains(unit.teamNum))
        {
            bool validDistrict = false;
            bool noBuilding = true;
            //must build on a rural district
            foreach (Building building in gameHex.district.buildings)
            {
                if (building.name == "RuralDistrict")
                {
                    validDistrict = true;
                    continue;
                }
                else
                {
                    noBuilding = false;
                    break;
                }
            }
            if (validDistrict && noBuilding)
            {
                gameHex.district.AddBuilding(new Building("Mine", gameHex.hex, Global.gameManager.game.mainGameBoard.gameHexDict[gameHex.hex].resourceType != ResourceType.None));
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }
    public bool BuildPasture(Unit unit)
    {
        unit.remainingMovement = 0;
        unit.decreaseHealth(50);
        //find the district on this tile and add building to it, or something so we can "work on a building?"
        GameHex gameHex = Global.gameManager.game.mainGameBoard.gameHexDict[unit.hex];
        if (gameHex.district != null && Global.gameManager.game.teamManager.GetAllies(gameHex.ownedBy).Contains(unit.teamNum))
        {
            bool validDistrict = false;
            bool noBuilding = true;
            //must build on a rural district
            foreach (Building building in gameHex.district.buildings)
            {
                if (building.name == "RuralDistrict")
                {
                    validDistrict = true;
                    continue;
                }
                else
                {
                    noBuilding = false;
                    break;
                }
            }
            if (validDistrict && noBuilding)
            {
                gameHex.district.AddBuilding(new Building("Pasture", gameHex.hex, Global.gameManager.game.mainGameBoard.gameHexDict[gameHex.hex].resourceType != ResourceType.None));
                if (Global.gameManager.game.mainGameBoard.gameHexDict[gameHex.hex].resourceType != ResourceType.None)
                {
                    gameHex.district.AddResource();
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }
    public bool BuildLumberyard(Unit unit)
    {
        unit.remainingMovement = 0;
        unit.decreaseHealth(50);
        //find the district on this tile and add building to it, or something so we can "work on a building?"
        GameHex gameHex = Global.gameManager.game.mainGameBoard.gameHexDict[unit.hex];
        if (gameHex.district != null && Global.gameManager.game.teamManager.GetAllies(gameHex.ownedBy).Contains(unit.teamNum))
        {
            bool validDistrict = false;
            bool noBuilding = true;
            //must build on a rural district
            foreach (Building building in gameHex.district.buildings)
            {
                if (building.name == "RuralDistrict")
                {
                    validDistrict = true;
                    continue;
                }
                else
                {
                    noBuilding = false;
                    break;
                }
            }
            if (validDistrict && noBuilding)
            {
                gameHex.district.AddBuilding(new Building("Lumbermill", gameHex.hex, Global.gameManager.game.mainGameBoard.gameHexDict[gameHex.hex].resourceType != ResourceType.None));
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }
    public bool BuildFishingBoat(Unit unit)
    {
        unit.remainingMovement = 0;
        unit.decreaseHealth(50);
        //find the district on this tile and add building to it, or something so we can "work on a building?"
        GameHex gameHex = Global.gameManager.game.mainGameBoard.gameHexDict[unit.hex];
        if (gameHex.district != null && Global.gameManager.game.teamManager.GetAllies(gameHex.ownedBy).Contains(unit.teamNum))
        {
            bool validDistrict = false;
            bool noBuilding = true;
            //must build on a rural district
            foreach (Building building in gameHex.district.buildings)
            {
                if (building.name == "RuralDistrict")
                {
                    validDistrict = true;
                    continue;
                }
                else
                {
                    noBuilding = false;
                    break;
                }
            }
            if (validDistrict && noBuilding)
            {
                gameHex.district.AddBuilding(new Building("FishingBoat", gameHex.hex, Global.gameManager.game.mainGameBoard.gameHexDict[gameHex.hex].resourceType != ResourceType.None));
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    public bool RemoveImprovement(Unit unit)
    {
        unit.remainingMovement = 0;
        unit.decreaseHealth(50);
        //find the district on this tile and add building to it, or something so we can "work on a building?"
        bool removed = false;
        GameHex gameHex = Global.gameManager.game.mainGameBoard.gameHexDict[unit.hex];
        if (gameHex.district != null && gameHex.ownedBy == unit.teamNum)
        {
            //must build on a rural district
            foreach (Building building in gameHex.district.buildings)
            {
                if (building.name == "Farm" || building.name == "Mine" || building.name == "Lumberyard" || building.name == "Pasture" || building.name == "FishingBoat")
                {
                    gameHex.district.RemoveBuilding(building);
                    removed = true;
                }
            }
        }
        return removed;
    }

    public bool Sleep(Unit unit)
    {
        unit.isSleeping = true;
        unit.CancelMovement();
        Global.gameManager.graphicManager.CallDeferred("UnselectObject");
        return true;
    }

    public bool Skip(Unit unit)
    {
        unit.isSkipping = true;
        unit.CancelMovement();
        Global.gameManager.graphicManager.CallDeferred("UnselectObject");
        return true;
    }

    public bool ExploreRuin(Unit unit)
    {
        unit.remainingMovement = 0;
        unit.isSkipping = true;
        unit.CancelMovement();
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
        {
            AncientRuins ancientRuins = Global.gameManager.game.mainGameBoard.gameHexDict[unit.hex].ancientRuins;
            if (ancientRuins != null)
            {
                ancientRuins.activeEvent = true;
                if(Global.gameManager.game.localPlayerTeamNum == unit.teamNum)
                {
                    Global.gameManager.graphicManager.uiManager.EventSelectionPopUp(ancientRuins);
                }
            }
            if (Global.gameManager.game.localPlayerTeamNum == unit.teamNum)
            {
                Global.gameManager.graphicManager.CallDeferred("UnselectObject");
            }
        }
        if (unit is Hero hero)
        {
            hero.IncreaseExperience(25);
        }
        return true;
    }

    public bool Trade(Unit unit)
    {
        if (Global.gameManager.game.playerDictionary[unit.teamNum].GetMaxTradeRoutes() < Global.gameManager.game.playerDictionary[unit.teamNum].tradeRouteCount)
        if(unit.teamNum == Global.gameManager.game.localPlayerTeamNum)
        {
            Global.gameManager.graphicManager.uiManager.CallDeferred("OpenTradeMenu", unit.teamNum);
        }
        //Global.gameManager.game.playerDictionary[unit.teamNum].NewTradeRoute(Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[unit.hex].district.cityID], );
        return false;
    }

    public void EnableOceanMovement(Unit unit)
    {
        if (unit.movementCosts[TerrainMoveType.Ocean] < 0)
        {
            unit.movementCosts[TerrainMoveType.Ocean] = unit.movementCosts[TerrainMoveType.Ocean] * -1;
        }
    }


    //hero abilities
    public bool Fireball(Unit unit, int level, GameHex target)
    {
        if (level == 0)
        {
            throw new Exception("Ability Fireball is Level 0 " + unit.name + " " + unit.hex);
        }
        else if(level == 1)
        {
            return unit.RangedAttackTarget(target, 20, Global.gameManager.game.teamManager);
        }
        else if(level == 2)
        {
            return unit.RangedAttackTarget(target, 26, Global.gameManager.game.teamManager);
        }
        else if(level == 3)
        {
            return unit.RangedAttackTarget(target, 32, Global.gameManager.game.teamManager);
        }
        else if(level == 4)
        {
            return unit.RangedAttackTarget(target, 40, Global.gameManager.game.teamManager);
        }
        else
        {
            throw new Exception("Ability Fireball is not Level 0,1,2,3,4" + unit.name + " " + unit.hex);
        }
    }
    public bool Blink(Unit unit, int level, GameHex target)
    {
        if (level == 0)
        {
            throw new Exception("Ability Blink is Level 0 " + unit.name + " " + unit.hex);
        }
        else if (level == 1)
        {
            return unit.TrySetGameHex(target);
        }
        else if (level == 2)
        {
            return unit.TrySetGameHex(target);
        }
        else if (level == 3)
        {
            return unit.TrySetGameHex(target);
        }
        else if (level == 4)
        {
            return unit.TrySetGameHex(target);
        }
        else
        {
            throw new Exception("Ability Blink is not Level 0,1,2,3,4" + unit.name + " " + unit.hex);
        }
    }

    public bool Banish(Unit unit, int level, GameHex target)
    {
        if (level == 0)
        {
            throw new Exception("Ability Banish is Level 0 " + unit.name + " " + unit.hex);
        }
        else if (level == 1)
        {
            if(target.units.Any())
            {
                Unit targetUnit = Global.gameManager.game.unitDictionary[target.units[0]];
                City targetCity = null;
                foreach (int cityID in Global.gameManager.game.playerDictionary[targetUnit.teamNum].cityList)
                {
                    City temp = Global.gameManager.game.cityDictionary[cityID];
                    if (temp.isCapital)
                    {
                        targetCity = temp;
                        break;
                    }
                }
                return targetUnit.TrySetGameHex(Global.gameManager.game.mainGameBoard.gameHexDict[targetCity.hex], true);
            }
            return false;
        }
        else if (level == 2)
        {
            if (target.units.Any())
            {
                Unit targetUnit = Global.gameManager.game.unitDictionary[target.units[0]];
                City targetCity = null;
                foreach (int cityID in Global.gameManager.game.playerDictionary[targetUnit.teamNum].cityList)
                {
                    City temp = Global.gameManager.game.cityDictionary[cityID];
                    if (temp.isCapital)
                    {
                        targetCity = temp;
                        break;
                    }
                }
                return targetUnit.TrySetGameHex(Global.gameManager.game.mainGameBoard.gameHexDict[targetCity.hex], true);
            }
            return false;
        }
        else if (level == 3)
        {
            if (target.units.Any())
            {
                Unit targetUnit = Global.gameManager.game.unitDictionary[target.units[0]];
                City targetCity = null;
                foreach (int cityID in Global.gameManager.game.playerDictionary[targetUnit.teamNum].cityList)
                {
                    City temp = Global.gameManager.game.cityDictionary[cityID];
                    if (temp.isCapital)
                    {
                        targetCity = temp;
                        break;
                    }
                }
                return targetUnit.TrySetGameHex(Global.gameManager.game.mainGameBoard.gameHexDict[targetCity.hex], true);
            }
            return false;
        }
        else
        {
            throw new Exception("Ability Banish is not Level 0,1,2,3" + unit.name + " " + unit.hex);
        }
    }

    public bool Chop(Unit unit, int level, GameHex target)
    {
        if (level == 0)
        {
            throw new Exception("Ability Chop is Level 0 " + unit.name + " " + unit.hex);
        }
        else if (level == 1)
        {
            if (target.units.Any())
            {
                Unit targetUnit = Global.gameManager.game.unitDictionary[target.units[0]];
                targetUnit.ApplyBleed(1, 10);
            }
            return unit.RangedAttackTarget(target, 10, Global.gameManager.game.teamManager);
        }
        else if (level == 2)
        {
            if (target.units.Any())
            {
                Unit targetUnit = Global.gameManager.game.unitDictionary[target.units[0]];
                targetUnit.ApplyBleed(2, 10);
            }
            return unit.RangedAttackTarget(target, 10, Global.gameManager.game.teamManager);
        }
        else if (level == 3)
        {
            if (target.units.Any())
            {
                Unit targetUnit = Global.gameManager.game.unitDictionary[target.units[0]];
                targetUnit.ApplyBleed(2, 15);
            }
            return unit.RangedAttackTarget(target, 10, Global.gameManager.game.teamManager);
        }
        else if (level == 4)
        {
            if (target.units.Any())
            {
                Unit targetUnit = Global.gameManager.game.unitDictionary[target.units[0]];
                targetUnit.ApplyBleed(3, 15);
            }
            return unit.RangedAttackTarget(target, 10, Global.gameManager.game.teamManager);
        }
        else
        {
            throw new Exception("Ability Chop is not Level 0,1,2,3,4" + unit.name + " " + unit.hex);
        }
    }

    public bool Leap(Unit unit, int level, GameHex target)
    {
        if (level == 0)
        {
            throw new Exception("Ability Leap is Level 0 " + unit.name + " " + unit.hex);
        }
        else if (level == 1)
        {
            if(!unit.AttackTarget(target, 0, Global.gameManager.game.teamManager, 0))
            { return false; }
            if (!unit.TrySetGameHex(target))
            { return false; }
            return true;
        }
        else if (level == 2)
        {
            if (!unit.AttackTarget(target, 0, Global.gameManager.game.teamManager, 2))
            { return false; }
            if (!unit.TrySetGameHex(target))
            { return false; }
            return true;
        }
        else if (level == 3)
        {
            if (!unit.AttackTarget(target, 0, Global.gameManager.game.teamManager, 5))
            { return false; }
            if (!unit.TrySetGameHex(target))
            { return false; }
            return true;
        }
        else if (level == 4)
        {
            if (!unit.AttackTarget(target, 0, Global.gameManager.game.teamManager, 7))
            { return false; }
            if (!unit.TrySetGameHex(target))
            { return false; }
            return true;
        }
        else
        {
            throw new Exception("Ability Leap is not Level 0,1,2,3,4" + unit.name + " " + unit.hex);
        }
    }

    public bool ForTheHorde(Unit unit, int level, GameHex target)
    {
        if (level == 0)
        {
            throw new Exception("Ability ForTheHorde is Level 0 " + unit.name + " " + unit.hex);
        }
        else if (level == 1)
        {
            float randomFactor = (float)new Random(target.hex.q + target.hex.r + Global.gameManager.game.turnManager.currentTurn).NextDouble();
            //25% chance
            if(randomFactor <= 0.25)
            {
                Unit tempUnit = new Unit("Warrior", 0, Global.gameManager.game.GetUniqueID(unit.teamNum), unit.teamNum);
                if (!target.SpawnUnit(tempUnit, false, true))
                {
                    tempUnit.decreaseHealth(99999.9f);
                    if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager1)) manager1.NewUnit(tempUnit.id);
                }
            }
            return true;
        }
        else if (level == 2)
        {
            float randomFactor = (float)new Random(target.hex.q + target.hex.r + Global.gameManager.game.turnManager.currentTurn).NextDouble();
            //66% chance
            if (randomFactor <= 0.66)
            {
                Unit tempUnit = new Unit("Warrior", 0, Global.gameManager.game.GetUniqueID(unit.teamNum), unit.teamNum);
                if (!target.SpawnUnit(tempUnit, false, true))
                {
                    tempUnit.decreaseHealth(99999.9f);
                    if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager1)) manager1.NewUnit(tempUnit.id);
                }
            }
            return true;
        }
        else if (level == 3)
        {
            float randomFactor = (float)new Random(target.hex.q + target.hex.r + Global.gameManager.game.turnManager.currentTurn).NextDouble();
            //100% chance
            if (randomFactor <= 1.0)
            {
                Unit tempUnit = new Unit("Warrior", 0, Global.gameManager.game.GetUniqueID(unit.teamNum), unit.teamNum);
                if (!target.SpawnUnit(tempUnit, false, true))
                {
                    tempUnit.decreaseHealth(99999.9f);
                    if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager1)) manager1.NewUnit(tempUnit.id);
                }
            }
            return true;
        }
        else
        {
            throw new Exception("Ability ForTheHorde is not Level 0,1,2,3" + unit.name + " " + unit.hex);
        }
    }

    public bool PinShot(Unit unit, int level, GameHex target)
    {
        if (level == 0)
        {
            throw new Exception("Ability PinShot is Level 0 " + unit.name + " " + unit.hex);
        }
        else if (level == 1)
        {
            if(target.units.Any())
            {
                Global.gameManager.game.unitDictionary[target.units[0]].remainingMovement = 0;
            }
            return unit.RangedAttackTarget(target, 20, Global.gameManager.game.teamManager);
        }
        else if (level == 2)
        {
            if (target.units.Any())
            {
                Global.gameManager.game.unitDictionary[target.units[0]].remainingMovement = 0;
            }
            return unit.RangedAttackTarget(target, 26, Global.gameManager.game.teamManager);
        }
        else if (level == 3)
        {
            if (target.units.Any())
            {
                Global.gameManager.game.unitDictionary[target.units[0]].remainingMovement = 0;
            }
            return unit.RangedAttackTarget(target, 32, Global.gameManager.game.teamManager);
        }
        else if (level == 4)
        {
            if (target.units.Any())
            {
                Global.gameManager.game.unitDictionary[target.units[0]].remainingMovement = 0;
            }
            return unit.RangedAttackTarget(target, 40, Global.gameManager.game.teamManager);
        }
        else
        {
            throw new Exception("Ability PinShot is not Level 0,1,2,3,4" + unit.name + " " + unit.hex);
        }
    }

    public bool Recall(Unit unit, int level, GameHex target)
    {
        if (level == 0)
        {
            throw new Exception("Ability Recall is Level 0 " + unit.name + " " + unit.hex);
        }
        else if (level == 1)
        {
            if (Global.gameManager.game.playerDictionary[unit.teamNum].cityList.Any())
            {
                City targetCity = null;
                foreach (int cityID in Global.gameManager.game.playerDictionary[unit.teamNum].cityList)
                {
                    City temp = Global.gameManager.game.cityDictionary[cityID];
                    if (temp.isCapital)
                    {
                        targetCity = temp;
                        break;
                    }
                }
                unit.increaseHealth(10);
                return unit.TrySetGameHex(Global.gameManager.game.mainGameBoard.gameHexDict[targetCity.hex], true);
            }
            return false;
        }
        else if (level == 2)
        {
            if (Global.gameManager.game.playerDictionary[unit.teamNum].cityList.Any())
            {
                City targetCity = null;
                foreach (int cityID in Global.gameManager.game.playerDictionary[unit.teamNum].cityList)
                {
                    City temp = Global.gameManager.game.cityDictionary[cityID];
                    if (temp.isCapital)
                    {
                        targetCity = temp;
                        break;
                    }
                }
                unit.increaseHealth(15);
                return unit.TrySetGameHex(Global.gameManager.game.mainGameBoard.gameHexDict[targetCity.hex], true);
            }
            return false;
        }
        else if (level == 3)
        {
            if (Global.gameManager.game.playerDictionary[unit.teamNum].cityList.Any())
            {
                City targetCity = null;
                foreach (int cityID in Global.gameManager.game.playerDictionary[unit.teamNum].cityList)
                {
                    City temp = Global.gameManager.game.cityDictionary[cityID];
                    if (temp.isCapital)
                    {
                        targetCity = temp;
                        break;
                    }
                }
                unit.increaseHealth(15);
                return unit.TrySetGameHex(Global.gameManager.game.mainGameBoard.gameHexDict[targetCity.hex], true);
            }
            return false;
        }
        else if (level == 4)
        {
            if (Global.gameManager.game.playerDictionary[unit.teamNum].cityList.Any())
            {
                City targetCity = null;
                foreach (int cityID in Global.gameManager.game.playerDictionary[unit.teamNum].cityList)
                {
                    City temp = Global.gameManager.game.cityDictionary[cityID];
                    if (temp.isCapital)
                    {
                        targetCity = temp;
                        break;
                    }
                }
                unit.increaseHealth(20);
                return unit.TrySetGameHex(Global.gameManager.game.mainGameBoard.gameHexDict[targetCity.hex], true);
            }
            return false;
        }
        else
        {
            throw new Exception("Ability Recall is not Level 0,1,2,3,4" + unit.name + " " + unit.hex);
        }
    }

    public bool Onewiththeforest(Unit unit, int level, GameHex target)
    {
        if (level == 0)
        {
            throw new Exception("Ability OWTF is Level 0 " + unit.name + " " + unit.hex);
        }
        else if (level == 1)
        {
            GameHex gameHex = Global.gameManager.game.mainGameBoard.gameHexDict[unit.hex];
            if (gameHex.featureSet.Contains(FeatureType.Forest))
            {
                return false;
            }
            gameHex.AddTerrainFeature(FeatureType.Forest);
            return true;
        }
        else if (level == 2)
        {
            GameHex gameHex = Global.gameManager.game.mainGameBoard.gameHexDict[unit.hex];
            if (gameHex.featureSet.Contains(FeatureType.Forest))
            {
                return false;
            }
            gameHex.AddTerrainFeature(FeatureType.Forest);
            return true;
        }
        else if (level == 3)
        {
            GameHex gameHex = Global.gameManager.game.mainGameBoard.gameHexDict[unit.hex];
            if (gameHex.featureSet.Contains(FeatureType.Forest))
            {
                return false;
            }
            gameHex.AddTerrainFeature(FeatureType.Forest);
            return true;
        }
        else if (level == 4)
        {
            GameHex gameHex = Global.gameManager.game.mainGameBoard.gameHexDict[unit.hex];
            if (gameHex.featureSet.Contains(FeatureType.Forest))
            {
                return false;
            }
            gameHex.AddTerrainFeature(FeatureType.Forest);
            return true;
        }
        else
        {
            throw new Exception("Ability OWTF is not Level 0,1,2,3,4" + unit.name + " " + unit.hex);
        }
    }

    public bool ClawSwipe(Unit unit, int level, GameHex target)
    {
        //find our cleave target for any level of the ability
        List<Hex> options = new();
        foreach (Hex hex in unit.hex.WrappingNeighbors(Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.bottom))
        {
            if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].units.Any())
            {
                foreach (int unitID in Global.gameManager.game.mainGameBoard.gameHexDict[hex].units)
                {
                    if (Global.gameManager.game.teamManager.GetEnemies(unit.teamNum).Contains(Global.gameManager.game.unitDictionary[unitID].teamNum))
                    {
                        options.Add(hex);
                    }
                }
            }
        }
        Random random = new Random(target.hex.q + target.hex.r + Global.gameManager.game.turnManager.currentTurn);
        Hex targetHex = options[random.Next(options.Count)];

        if (level == 0)
        {
            throw new Exception("Ability ClawSwipe is Level 0 " + unit.name + " " + unit.hex);
        }
        else if (level == 1)
        {
            unit.RangedAttackTarget(Global.gameManager.game.mainGameBoard.gameHexDict[targetHex], 15, Global.gameManager.game.teamManager);
            return unit.RangedAttackTarget(target, 15, Global.gameManager.game.teamManager);
        }
        else if (level == 2)
        {
            unit.RangedAttackTarget(Global.gameManager.game.mainGameBoard.gameHexDict[targetHex], 20, Global.gameManager.game.teamManager);
            return unit.RangedAttackTarget(target, 20, Global.gameManager.game.teamManager);
        }
        else if (level == 3)
        {
            unit.RangedAttackTarget(Global.gameManager.game.mainGameBoard.gameHexDict[targetHex], 25, Global.gameManager.game.teamManager);
            return unit.RangedAttackTarget(target, 25, Global.gameManager.game.teamManager);
        }
        else if (level == 4)
        {
            unit.RangedAttackTarget(Global.gameManager.game.mainGameBoard.gameHexDict[targetHex], 30, Global.gameManager.game.teamManager);
            return unit.RangedAttackTarget(target, 30, Global.gameManager.game.teamManager);
        }
        else
        {
            throw new Exception("Ability ClawSwipe is not Level 0,1,2,3,4" + unit.name + " " + unit.hex);
        }
    }
}

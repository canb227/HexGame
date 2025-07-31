using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data;
using Godot;
using System.IO;
using NetworkMessages;

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

    public UnitEffect(String functionName)
    {
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


    public bool Apply(int unitID, int level = 0, float combatPower = 0.0f, GameHex abilityTarget = null)
    {
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
        if(functionString == "SettleCapitalAbility")
        {
            return SettleCapitalAbility(Global.gameManager.game.unitDictionary[unitID], "CapitalCityName");
        }
        else if(functionString == "SettleCityAbility")
        {
            return SettleCity(Global.gameManager.game.unitDictionary[unitID], "SettledCityName");
        }
        else if(functionString == "ScoutVisionAbility")
        {
            Global.gameManager.game.unitDictionary[unitID].sightRange += 1;
            Global.gameManager.game.unitDictionary[unitID].UpdateVision();
            return true;
        }
        else if(functionString == "RangedAttack")
        {
            return RangedAttack(Global.gameManager.game.unitDictionary[unitID], combatPower, abilityTarget);
        }
        else if(functionString == "BombardAttack")
        {
            return BombardAttack(Global.gameManager.game.unitDictionary[unitID], combatPower, abilityTarget);
        }
        else if(functionString == "EnableEmbarkDisembark")
        {
            EnableEmbarkDisembark(Global.gameManager.game.unitDictionary[unitID]);
            return true;
        }
        else if(functionString == "Fortify")
        {
            Fortify(Global.gameManager.game.unitDictionary[unitID]);
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
            Global.gameManager.game.mainGameBoard.gameHexDict[unit.hex].SpawnUnit(new Hero("Arcana", 0, Global.gameManager.game.GetUniqueID(unit.teamNum), unit.teamNum), false, true);
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
                Global.gameManager.graphicManager.uiManager.EventSelectionPopUp(ancientRuins);
            }
            Global.gameManager.graphicManager.CallDeferred("UnselectObject");
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
        if(level == 0)
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
}

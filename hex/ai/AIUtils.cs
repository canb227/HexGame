
using System.Collections.Generic;
using static AIManager;
using System.Linq;
using System.Diagnostics;
using Godot;
using System;
using static AIUtils;


public static class AIUtils
{

    static bool AIDEBUG = false;
    static Random rng = new Random();

    //tired of calling the whole Global.gameManager.game.mainGameBoard.blah all the time
    static int top;
    static int bottom;
    static int right;
    static int left;

    //Internal AI Data Class Block Start
    public class AI
    {
        public Player player;

        public AIPersonality personality = AIPersonality.Standard;
        public AIDefenseArmyBuildingStrategy AIDefenseArmyBuildingStrategy;
        public AIDefenseArmyMovementStrategy AIDefenseArmyMovementStrategy;
        public AIDefenseArmyThreatStrategy AIDefenseArmyThreatStrategy;
        
        public AIAttackArmyBuildingStrategy AIAttackArmyBuildingStrategy;
        public AIAttackArmyBuildupStrategy AIAttackArmyBuildupStrategy;
        public AIAttackArmyMovementStrategy AIAttackArmyMovementStrategy;
        public AIAttackArmyRetreatStrategy AIAttackArmyRetreatStrategy;

        public AICityExpansionStrategy AICityExpansionStrategy;

        public AIMilitaryUnitRetreatStrategy AIMilitaryUnitRetreatStrategy;

        public AIPickBuildingStrategy AIPickBuildingStrategy;
        public AIPickProductionStrategy AIPickProductionStrategy;

        public Dictionary<int, AICity> cities = new();
        public List<int> allDefenders = new();
        public List<int> attackers = new();

        public bool hasGatherTarget = false;
        public Hex gatherTarget;
        public bool isAttacking = false;
        public bool hasAttackTarget = false;
        public Hex attackTarget;

        public bool canBuildSettler = true;
        public bool canSettleCities = true;
        public bool canResearch = true;
        public bool canCulture = true;
        
        public int desiredDefendersPerCity;
        public float desiredDefendersPerCityPerTurnScaling;

        public int desiredAttackersInArmy;
        public float desiredAttackersInArmyPerTurnScaling;

        public int desiredCityCount;
        public float desiredCityCountPerTurnScaling;

        public float cityScoreThresholdPerTurn;
    }

    public class AICity
    {
        public int cityID = 0;
        public List<int> defenderUnits = new();
        public List<Hex> localThreats = new();
        public int desiredDefenders = 2;
        public bool currentlyBuildingDefender = false;
        public bool currentlyBuildingAttacker = false;

        public AICity(int id)
        {
            cityID = id;
        }
    }

    //Internal AI Data Class Block Done
    public enum AICityExpansionStrategy
    {
        NONE,
        Random,
        FocusResources,
    }

    public enum AIMilitaryUnitRetreatStrategy
    {
        NoRetreat,
        FleeIfHurt,
        FleeIfAlmostDead,
        
    }

    public enum AIDefenseArmyBuildingStrategy
    {
        RandomMix,
        FavorRanged,
        FavorMelee,
        AllRanged,
        AllMelee,
        Balanced,
        DontBuildDefenseArmy,
    }

    public enum AIDefenseArmyMovementStrategy
    {
        WaitNearCity,
        WanderWithinBorders,
        SmartPositioning,
        NONE
    }

    public enum AIDefenseArmyThreatStrategy
    {
        CityThreatFirst,
        NearbyThreatFirst,
        DontUseDefenseArmy,
        NONE,
    }
    
    public enum AIAttackArmyBuildingStrategy
    {
        Random,
        RandomNoNaval,
        AllRanged,
        AllSiege,
        AllMelee,
        AllNaval,
        FavorRanged,
        FavorSiege,
        FavorMelee,
        FavorNaval,
        Balanced,
        BalancedNoNaval,
        NoAttackArmy,
    }

    public enum AIAttackArmyMovementStrategy
    {
        DirectToTarget,
        AttackMove,
        PathConstrainedAttackMove,
    }

    public enum AIAttackArmyBuildupStrategy
    {
        NoBuildup,
        WaitForGlobalCount,
        WaitForLocalGroup,
    }


    public enum AIAttackArmyRetreatStrategy
    {
        NoRetreat,
        RetreatToNearestOwnedCity,
    }

    public enum AIPersonality
    {
        NONE,
        Standard,
        Aggressive,
        Defensive,
        Economic,
        RANDOM,
        MinorDumbAggro,
        MinorDumbDefense,
        MinorSmartAggro,
        MinorSmartDefensive,
    }

    public enum AIPickBuildingStrategy
    {
        Random,
        FocusFood,
        FocusCulture,
        FocusProduction,
        FocusResearch,
        FocusMilitary,
        Balanced
    }

    public enum AIPickProductionStrategy
    {
        Random,
        OnlyEcon,
        OnlyArmy,
        FocusEcon,
        FocusArmy,
        Balanced,
    }
    //Gamestate Modifiers Block Start
    public static void EndAITurn(AI ai)
    {
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] End Turn subroutine"); }
        if (!ai.player.turnFinished)
        {
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Ending turn"); }
            Global.gameManager.EndTurn(ai.player.teamNum);
        }
    }
    public static void AIActivateAbility(AI ai, Unit unit, string abilityName, Hex target)
    {
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] AI has activated an Ability: " + abilityName + " unitID: " + unit.id + " unitName: " + unit.name + " unitLocation: " + unit.hex + " targetLocation: " + target); }
        Global.gameManager.ActivateAbility(unit.id, abilityName, target);
    }
    public static void AIMoveUnit(AI ai, Unit unit, Hex target)
    {
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] AI has requested to move a fakeUnit. unitID: " + unit.id + " unitName: " + unit.name + " unitLocation: " + unit.hex + " moveTo: " + target); }
        if (IsSafeHex(ai, target))
        {
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Target hex is safe - enemyPresent=false"); }
            Global.gameManager.MoveUnit(unit.id, target, false);
        }
        else
        {
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Target hex is UNsafe - enemyPresent=true"); }
            Global.gameManager.MoveUnit(unit.id, target, true);
        }
    }
    //Gamestate Modifiers Block Done

    //Unit Action Helpers Block Start
    public static void MilitaryUnitMoveOrAttack(AI ai, Unit unit, Hex hex)
    {
        if ((unit.unitClass & UnitClass.Ranged) == UnitClass.Ranged)
        {
            UnitAbility rangedAttack = unit.abilities.FirstOrDefault(a => a.name.Equals("RangedAttack"));
            if (IsRangedAttackableHex(ai, hex))
            {
                if (hex.WrapDistance(unit.hex) <= rangedAttack.range)
                {
                    AIActivateAbility(ai, unit, "RangedAttack", hex);
                    return;
                }
                else
                {
                    AIMoveUnit(ai, unit, hex);
                    return;
                }
            }
            else
            {
                AIMoveUnit(ai, unit, hex);
                return;
            }
        }
        else if ((unit.unitClass & UnitClass.Infantry) == UnitClass.Infantry)
        {
            AIMoveUnit(ai, unit, hex);
        }
    }
    public static void RandomMoveOrAttack(AI ai, Unit unit, List<Hex> validMoves)
    {
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Picking a random hex from validMoves and moving to it - even if it has an enemy."); }
        Hex target = validMoves[rng.Next(validMoves.Count)];
        AIMoveUnit(ai, unit, target);
    }
    public static void RandomMoveNoAttack(AI ai, Unit unit, List<Hex> validMoves)
    {
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Picking a random hex from validMoves and moving to it - only if it has no enemy"); }
        List<Hex> safeMoves = new List<Hex>();
        foreach (Hex hex in validMoves.ToList())
        {
            if (IsSafeHex(ai, hex))
            {
                safeMoves.Add(hex);
            }
        }
        Hex target = safeMoves[rng.Next(safeMoves.Count)];
        AIMoveUnit(ai, unit, target);
    }
    public static Hex RandomValidMoveInsideCityBorders(AI ai, Unit unit, City city)
    {
        List<Hex> heldHexes = city.heldHexes.ToList();
        Hex randomHex = new();
        float cost = 9999;
        while (cost>10)
        {
            randomHex = heldHexes[rng.Next(heldHexes.Count)];
            heldHexes.Remove(randomHex);
            unit.PathFind(unit.hex, randomHex, Global.gameManager.game.teamManager, unit.movementCosts, unit.movementSpeed, out cost);
        }
        return randomHex;
    }
    //Unit Action Helpers Block Done


    //Hex Search Helpers Block Start
    public static Hex PickClosest(AI ai, List<Hex> options, int unitID)
    {
        Hex target = new();
        if (options.Count == 0)
        {
            return target;
        }
        Unit unit = Global.gameManager.game.unitDictionary[unitID];
        Unit fakeUnit = new Unit(unit.unitType, -1, ai.player.teamNum);
        fakeUnit.hex = unit.hex; //create a dummy unit at the given hex
        float lowCost = float.MaxValue;
        foreach (Hex h in options)
        {
            fakeUnit.PathFind(fakeUnit.hex, h, Global.gameManager.game.teamManager, fakeUnit.movementCosts, fakeUnit.movementSpeed, out float cost);
            if (cost < lowCost)
            {
                target = h; //find the closest district
            }
        }
        Global.gameManager.game.unitDictionary.Remove(-1);
        return target;
    }
    public static List<Hex> FindAllEnemiesInRange(AI ai, Hex hex, int range)
    {
        List<Hex> hexesInRange = hex.WrappingRange(range, left, right, top, bottom);
        List<Hex> targets = new List<Hex>();
        foreach (Hex h in hexesInRange)
        {
            if (!IsSafeHex(ai, hex))
            {
                targets.Add(hex); //found an enemy unit in range
            }
        }
        return targets;
    }
    public static bool FindClosestAnyEnemyInRange(AI ai, Hex hex, int range, out Hex target)
    {
        Unit unit = new Unit("Warrior", -ai.player.teamNum, ai.player.teamNum);
        unit.hex = hex; //create a dummy unit at the given hex
        List<Hex> hexesInRange = unit.hex.WrappingRange(range, left, right, top, bottom);
        List<Hex> targets = new();
        foreach (Hex h in hexesInRange)
        {
            if (!IsSafeHex(ai, h))
            {
                targets.Add(h); //found an enemy unit in range
            }
        }
        if (targets.Count > 0)
        {
            target = targets[0];
            float lowCost = float.MaxValue;
            foreach (Hex h in targets)
            {
                unit.PathFind(unit.hex, h, Global.gameManager.game.teamManager, unit.movementCosts, unit.movementSpeed, out float cost);
                if (cost < lowCost)
                {
                    target = h; //find the closest district
                }
            }
            Global.gameManager.game.unitDictionary.Remove(-ai.player.teamNum);
            ai.player.unitList.Remove(-ai.player.teamNum);
            return true;
        }
        else
        {
            Global.gameManager.game.unitDictionary.Remove(-ai.player.teamNum);
            ai.player.unitList.Remove(-ai.player.teamNum);
            target = new Hex(); //return an empty hex if no district found
            return false; //no enemy district found in range
        }
    }
    public static bool FindClosestAnyEnemyInRange(AI ai, Unit unit, int range, out Hex target)
    {
        List<Hex> hexesInRange = unit.hex.WrappingRange(range, left, right, top, bottom);
        List<Hex> targets = new();
        foreach (Hex h in hexesInRange)
        {
            if (!IsSafeHex(ai, h))
            {
                targets.Add(h); //found an enemy unit in range
            }
        }
        if (targets.Count > 0)
        {
            target = targets[0];
            float lowCost = float.MaxValue;
            foreach (Hex h in targets)
            {
                unit.PathFind(unit.hex, h, Global.gameManager.game.teamManager, unit.movementCosts, unit.movementSpeed, out float cost);
                if (cost < lowCost)
                {
                    target = h; //find the closest district
                }
            }
            return true;
        }
        else
        {
            target = new Hex(); //return an empty hex if no district found
            return false; //no enemy district found in range
        }
    }
    public static bool FindClosestValidSettleInRange(AI ai, Unit unit, int range, out Hex target)
    {
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] FindClosestValidSettleInRange Function Start"); }
        List<Hex> hexesInRange = unit.hex.WrappingRange(range, left, right, top, bottom);
        //if (AIDEBUG) { Global.Log("    [AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] WrappingRange provides the following set of hexes in range: " + string.Join(",",hexesInRange)); }
        List<Hex> targets = new();
        foreach (Hex h in hexesInRange)
        {
            if (unit.CanSettleHere(h, 3, new List<TerrainType>() { TerrainType.Flat, TerrainType.Rough }, false))
            {
                //if (AIDEBUG) { Global.Log("    [AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Found a valid settle hex: " + h); }
                targets.Add(h); //found a valid settle hex in range
            }
        }
        //if (AIDEBUG) { Global.Log("    [AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Of those, these are valid settle hexes: " + string.Join(",", hexesInRange)); }
        target = new Hex();
        float lowCost = float.MaxValue;
        if (targets.Count > 0)
        {
            foreach (Hex hex in targets)
            {
                //if (AIDEBUG) { Global.Log("    [AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Testing pathfind distance to potential settle location: " + hex + " from unit location : " + unit.hex); }

                float cost = float.MaxValue;
                unit.PathFind(unit.hex, hex, Global.gameManager.game.teamManager, unit.movementCosts, unit.movementSpeed, out cost);

                if (cost < lowCost)
                {
                    //if (AIDEBUG) { Global.Log("        path cost: " + cost + " is new shortest!"); }
                    lowCost = cost;
                    target = hex;
                }
                else
                {
                    //if (AIDEBUG) { Global.Log("        path cost: " + cost + " is too high, discarding."); }
                }
            }
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Found a settle location within range: " + target); }
            return true;
        }
        //return an empty hex if no district found
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] No valid settle location within range."); }
        return false; //no enemy district found in range
    }
    public static Hex FindClosestEnemyCity(AI ai, Unit unit )
    {
        Hex target = new Hex();
        float lowCost = float.MaxValue;
        foreach (int cityID in Global.gameManager.game.cityDictionary.Keys)
        {
            City city = Global.gameManager.game.cityDictionary[cityID];
            if (IsEnemy(ai.player.teamNum, city.teamNum))
            {
                unit.PathFind(unit.hex, city.hex, Global.gameManager.game.teamManager, unit.movementCosts, unit.movementSpeed, out float cost);
                if (cost < lowCost)
                {
                    target = city.hex;
                }
            }
        }
        return target;
    }
    public static Hex FindClosestFriendlyCity(AI ai, Unit unit)
    {
        Hex target = new Hex();
        float lowCost = float.MaxValue;
        foreach (int cityID in Global.gameManager.game.cityDictionary.Keys)
        {
            City city = Global.gameManager.game.cityDictionary[cityID];
            if (!IsEnemy(ai.player.teamNum, city.teamNum))
            {
                unit.PathFind(unit.hex, city.hex, Global.gameManager.game.teamManager, unit.movementCosts, unit.movementSpeed, out float cost);
                if (cost < lowCost)
                {
                    target = city.hex;
                }
            }
        }
        return target;
    }
    public static Hex FindClosestEnemyDistrict(AI ai, Unit unit)
    {
        Hex target = new Hex();
        float lowCost = float.MaxValue;
        foreach (int cityID in Global.gameManager.game.cityDictionary.Keys)
        {
            City city = Global.gameManager.game.cityDictionary[cityID];
            if (IsEnemy(ai.player.teamNum, city.teamNum))
            {
                foreach (District district in city.districts)
                {
                    if (district.health > 0)
                    {
                        unit.PathFind(unit.hex, district.hex, Global.gameManager.game.teamManager, unit.movementCosts, unit.movementSpeed, out float cost);
                        if (cost < lowCost)
                        {
                            target = district.hex;
                        }
                    }

                }

            }
        }
        return target;
    }
    //Hex Search Helpers Block Done

    //Hex Check Helpers Block Start
    public static bool IsEnemy(int selfTeam, int otherTeam)
    {
        return Global.gameManager.game.teamManager.GetEnemies(selfTeam).Contains(otherTeam);
    }
    public static bool IsSafeHex(AI ai, Hex hex)
    {
        //check if the hex is safe from enemy units 
        foreach (int unitID in Global.gameManager.game.mainGameBoard.gameHexDict[hex].units)
        {
            Unit unit = Global.gameManager.game.unitDictionary[unitID];
            if (IsEnemy(ai.player.teamNum, unit.teamNum))
            {
                return false; //hex is not safe, it has an enemy unit
            }
        }
        if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].district != null && Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.health > 0)
        {
            if (IsEnemy(ai.player.teamNum, Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.cityID].teamNum))
            {
                return false; //hex is not safe, it has an enemy district
            }
        }
        return true; //hex is safe
    }
    public static bool IsMeleeAttackableHex(AI ai, Hex hex)
    {
        //check if the hex is safe from enemy units
        foreach (int unitID in Global.gameManager.game.mainGameBoard.gameHexDict[hex].units)
        {
            Unit unit = Global.gameManager.game.unitDictionary[unitID];
            if (IsEnemy(ai.player.teamNum, unit.teamNum))
            {
                return true;
            }
        }
        if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].district != null && Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.health > 0)
        {
            if (IsEnemy(ai.player.teamNum, Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.cityID].teamNum))
            {
                return true;
            }
        }
        return false;
    }
    public static bool IsRangedAttackableHex(AI ai, Hex hex)
    {
        //check if the hex is safe from enemy units
        foreach (int unitID in Global.gameManager.game.mainGameBoard.gameHexDict[hex].units)
        {
            Unit unit = Global.gameManager.game.unitDictionary[unitID];
            if (IsEnemy(ai.player.teamNum, unit.teamNum))
            {
                return true;
            }
        }
        if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].district != null && Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.health > 0)
        {
            if (IsEnemy(ai.player.teamNum, Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.cityID].teamNum))
            {
                return true;
            }
        }
        return false;
    }
    //Hex Check Helpers Block Done

    //City Helpers Block Start
    public static string PickRandomLandMilitaryUnit(AI ai, City city)
    {
        List<string> validUnits = city.ValidUnits();
        foreach (string unit in validUnits.ToList())
        {
            if (((UnitLoader.unitsDict[unit].Class & UnitClass.Combat) != UnitClass.Combat) || ((UnitLoader.unitsDict[unit].Class & UnitClass.Naval) == UnitClass.Naval) || ((UnitLoader.unitsDict[unit].Class & UnitClass.Recon) == UnitClass.Recon))
            {
                validUnits.Remove(unit);
            }
        }
        Global.Log(validUnits.Count.ToString());
        return validUnits[rng.Next(validUnits.Count)];
    }
    public static int GetNumberOfUnit(AI ai, string name)
    {
        int retval = 0;
        foreach (int unitID in ai.player.unitList)
        {
            Unit unit = Global.gameManager.game.unitDictionary[unitID];
            if (unit.name.Equals(name))
            {
                retval++;
            }
        }
        return retval;
    }
    public static int GetNumberOfBuildingInCity(AI ai, City city, string name)
    {
        int retval = 0;
        foreach (District district in city.districts)
        {
            foreach (Building building in district.buildings)
            {
                if (building.name.Equals(name))
                {
                    retval++;
                }
            }
        }
        return retval;
    }
    public static List<District> GetDistrictsWithOpenSlots(AI ai, City city)
    {
        List<District> retVal = new List<District>();
        foreach (District district in city.districts)
        {
            if (district.buildings.Count < district.maxBuildings)
            {
                retVal.Add(district);
            }
        }
        return retVal;
    }
    public static void AttemptAnyBuildingAtRandomValidHex(List<string> listOfValidBuildings, AI ai, City city)
    {

        if (listOfValidBuildings.Count == 0)
        {
            //no valid buildings left
            return;
        }

        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Recursive building attempt. List of valid buildings to try: " + string.Join(",", listOfValidBuildings)); }

        string buildingName = listOfValidBuildings[rng.Next(listOfValidBuildings.Count)];
        BuildingInfo buildingInfo = BuildingLoader.buildingsDict[buildingName];

        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Picked building: " + buildingName + " checking valid hexes for this building."); }

        List<Hex> validHexes = city.ValidUrbanBuildHexes(buildingInfo.TerrainTypes, buildingInfo.DistrictType, 3, buildingName);
        if (validHexes.Count > 0)
        {
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] This building can be built at the following hexes: " + string.Join(",", validHexes)); }
            Hex target = validHexes[rng.Next(validHexes.Count)];
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Picking random hex to build at from list: " + target); }
            Global.gameManager.AddToProductionQueue(city.id, buildingName, target);
        }
        else
        {
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] This building cannot be built anywhere. Removing it from validBuildingList and recurring."); }
            listOfValidBuildings.Remove(buildingName);
            AttemptAnyBuildingAtRandomValidHex(new List<string>(listOfValidBuildings), ai, city);
        }
    }
    public static bool AttemptBuildingSpecificAtRandomValidHex(string buildingName, AI ai, City city)
    {
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Attempting to build specific building: " + buildingName + " at random valid location."); }

        BuildingInfo buildingInfo = BuildingLoader.buildingsDict[buildingName];
        List<Hex> validHexes = city.ValidUrbanBuildHexes(buildingInfo.TerrainTypes, buildingInfo.DistrictType);

        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] List of valid hexes to build: " + string.Join(",", validHexes)); }

        if (validHexes.Count > 0)
        {
            Hex target = validHexes[rng.Next(validHexes.Count)];
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] building at randomly chosen valid hex: " + target); }
            Global.gameManager.AddToProductionQueue(city.id, buildingName, target);
            return true;
        }
        else
        {
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Attempting to build specific building: " + buildingName + " FAILED."); }
            return false;
        }
    }
    public static string PickRandomMeleeMilitaryUnit(AI ai,City city)
    {
        List<string> validUnits = new();
        foreach (string unit in city.ValidUnits().ToList())
        {
            if ((UnitLoader.unitsDict[unit].Class & UnitClass.Infantry) == UnitClass.Infantry)
            {
                validUnits.Add(unit);
            }
        }
        //Global.Log(validUnits.Count.ToString());
        return validUnits[rng.Next(validUnits.Count)];
    }
    public static string PickRandomNonMeleeMilitaryUnit(AI ai, City city)
    {
        List<string> validUnits = new();
        foreach (string unit in city.ValidUnits().ToList())
        {
            if (((UnitLoader.unitsDict[unit].Class & UnitClass.Combat) == UnitClass.Combat) && ((UnitLoader.unitsDict[unit].Class & UnitClass.Infantry) != UnitClass.Infantry))
            {
                validUnits.Add(unit);
            }
        }
       // Global.Log(validUnits.Count.ToString());
        return validUnits[rng.Next(validUnits.Count)];
    }
    //City Helpers Block Done

}


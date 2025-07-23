using Godot;
using ImGuiNET;
using NetworkMessages;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static AIUtils;
public partial class AIManager
{
    const bool AIDEBUG = false; 
    public List<AI> aiList = new List<AI>();
    Random rng = new Random();

    //tired of calling the whole Global.gameManager.game.mainGameBoard.blah all the time
    int top;
    int bottom;
    int right;
    int left;

    public void InitAI()
    {
        top = Global.gameManager.game.mainGameBoard.top;
        bottom = Global.gameManager.game.mainGameBoard.bottom;
        right = Global.gameManager.game.mainGameBoard.right;
        left = Global.gameManager.game.mainGameBoard.left;
        AIUtils.UpdateLayout();
        
        foreach (Player player in Global.gameManager.game.playerDictionary.Values)
        {
            if (player.isAI)
            {
                AI ai = new AI { player = player };
                aiList.Add(ai);
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] AI Created"); }
                InitStrategy(ai);
            }
        }
    }

    public void AddNewAI(Player player)
    {
        if (player.isAI)
        {
            AI ai = new AI { player = player };
            aiList.Add(ai);
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] AI Created"); }
            InitStrategy(ai);
        }
        else
        {
            Global.Log($"Non AI teamNum cannot be assigned an AI controller.");
        }
    }
    private void InitStrategy(AI ai)
    {

        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Picking Strategies based on personality"); }
        switch (ai.player.faction)
        {
            case FactionType.Human:
                ai.personality = PickMajorAIPersonality(ai,ai.player.faction);
                switch (ai.personality)
                {
                    case AIPersonality.Standard:
                        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Major AI - Standard Personality"); }
                        ai.canBuildSettler = true;
                        ai.canSettleCities = true;
                        ai.canResearch = true;
                        ai.canCulture = true;
                        ai.desiredCityCount = 4;
                        ai.desiredCityCountPerTurnScaling = 1 / 50;
                        ai.desiredDefendersPerCity = 2;
                        ai.desiredDefendersPerCityPerTurnScaling = 1/50;
                        ai.desiredAttackersInArmy = 4;
                        ai.desiredAttackersInArmyPerTurnScaling = 1/50;
                        ai.AIAttackArmyBuildingStrategy = AIAttackArmyBuildingStrategy.Balanced;
                        ai.AIAttackArmyBuildupStrategy = AIAttackArmyBuildupStrategy.WaitForGlobalCount;
                        ai.AIAttackArmyMovementStrategy = AIAttackArmyMovementStrategy.AttackMove;
                        ai.AIAttackArmyRetreatStrategy = AIAttackArmyRetreatStrategy.RetreatToNearestOwnedCity;
                        ai.AIMilitaryUnitRetreatStrategy = AIMilitaryUnitRetreatStrategy.FleeIfAlmostDead;
                        ai.AIDefenseArmyBuildingStrategy = AIDefenseArmyBuildingStrategy.Balanced;
                        ai.AIDefenseArmyMovementStrategy = AIDefenseArmyMovementStrategy.WanderWithinBorders;
                        ai.AIDefenseArmyThreatStrategy = AIDefenseArmyThreatStrategy.CityThreatFirst;
                        ai.AIPickProductionStrategy = AIPickProductionStrategy.Balanced;
                        ai.AIPickBuildingStrategy = AIPickBuildingStrategy.Balanced;
                        ai.AICityExpansionStrategy = AICityExpansionStrategy.FocusResources;
                        ai.desiredDefendersPerCity = 2;
                        break;
                }
                break;
            case FactionType.Goblins:
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Minor AI - Goblin Faction - DumbAggro Personality"); }
                ai.personality = PickMinorAIPersonality(ai, ai.player.faction);
                switch (ai.personality)
                {
                    case AIPersonality.MinorDumbAggro:
                        ai.canBuildSettler = false;
                        ai.canSettleCities = false;
                        ai.canResearch = true;
                        ai.canCulture = true;
                        ai.desiredCityCount = 1;
                        ai.desiredCityCountPerTurnScaling = 0;
                        ai.desiredDefendersPerCity = 0;
                        ai.desiredDefendersPerCityPerTurnScaling = 0;
                        ai.desiredAttackersInArmy = 2;
                        ai.desiredAttackersInArmyPerTurnScaling = 0;
                        ai.AICityExpansionStrategy = AICityExpansionStrategy.NONE;
                        ai.AIAttackArmyBuildingStrategy = AIAttackArmyBuildingStrategy.FavorMelee;
                        ai.AIAttackArmyBuildupStrategy = AIAttackArmyBuildupStrategy.WaitForGlobalCount;
                        ai.AIAttackArmyMovementStrategy = AIAttackArmyMovementStrategy.AttackMove;
                        ai.AIAttackArmyRetreatStrategy = AIAttackArmyRetreatStrategy.NoRetreat;
                        ai.AIMilitaryUnitRetreatStrategy = AIMilitaryUnitRetreatStrategy.NoRetreat;
                        ai.AIDefenseArmyBuildingStrategy = AIDefenseArmyBuildingStrategy.DontBuildDefenseArmy;
                        ai.AIDefenseArmyMovementStrategy = AIDefenseArmyMovementStrategy.NONE;
                        ai.AIDefenseArmyThreatStrategy = AIDefenseArmyThreatStrategy.NONE;
                        ai.AIPickProductionStrategy = AIPickProductionStrategy.FocusArmy;
                        ai.AIPickBuildingStrategy = AIPickBuildingStrategy.FocusProduction;
                        break;
                }
                break;
            default:
                throw new Exception("unknown faction type "+ ai.player.faction);
                break;
        }
        foreach (var teamNum in Global.gameManager.game.playerDictionary.Keys)
        {
            if (teamNum!=ai.player.teamNum)
            {
                Global.gameManager.SetDiplomaticState(ai.player.teamNum, teamNum, DiplomaticState.War);
            }

        }
    }

    private AIPersonality PickMinorAIPersonality(AI ai, FactionType faction)
    {
        if (faction == FactionType.Goblins)
        {
            return AIPersonality.MinorDumbAggro;
        }
        return AIPersonality.NONE;
    }

    public bool OnTurnStart()
    {
        foreach (AI ai in aiList)
        {
            if (!ai.player.turnFinished)
            {
                
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Hasn't ended turn - iterating through logic"); }
                HandleSettlers(ai);
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Starting City Handling"); }
                HandleCities(ai);
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Starting Unit Handling"); }
                HandleUnits(ai);
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Starting Research/Culture Handling"); }
                HandleResearchAndCulture(ai);
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Starting End Turn process"); }
                EndAITurn(ai);
            }

        }
        return true;
    }

    public bool OnTurnStartOneAI(AI ai)
    {
        if (!ai.player.turnFinished)
        {
            if (ai.player.teamNum == 0)
            {
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Skipping AI processing for team 0"); }
                EndAITurn(ai); //end turn for team 0 AI
                return true;
            }
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Hasn't ended turn - iterating through logic"); }
            HandleSettlers(ai);
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Starting City Handling"); }
            HandleCities(ai);
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Starting Unit Handling"); }
            HandleUnits(ai);
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Starting Research/Culture Handling"); }
            HandleResearchAndCulture(ai);
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Starting End Turn process"); }
            EndAITurn(ai);
        }
        return true;
    }

    private void HandleSettlers(AI ai)
    {
        if (!ai.canSettleCities)
        {
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] This AI can't use settlers, but has one or more. Skipping them."); }
            return;
        }
        foreach (int unitID in ai.player.unitList.ToList())
        {
            Unit unit = Global.gameManager.game.unitDictionary[unitID];
            if (unit.unitType == "Settler" || unit.unitType == "Founder")
            {
                if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] This unitName: {unit.name} with type {unit.unitType} is a settler."); }
                HandleSettler(ai, unit);
            }
        }
    }

    private void HandleUnits(AI ai)
    {
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Handle Units subroutine"); }
        foreach (int unitID in ai.player.unitList.ToList())
        {
            Unit unit = Global.gameManager.game.unitDictionary[unitID];
            if ((unit.unitClass & UnitClass.Combat) ==UnitClass.Combat)
            {
                foreach (Hex hex in unit.hex.WrappingNeighbors(left, right, bottom))
                {
                    foreach (int uid in Global.gameManager.game.mainGameBoard.gameHexDict[hex].units)
                    {
                        if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] unit{unit.id} is next to another unit. Checking if its a baddie."); }
                        if (IsEnemy(ai.player.teamNum, Global.gameManager.game.unitDictionary[unitID].teamNum))
                        {
                            if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] unit{unit.id} is next to an enemy, attacking it."); }
                            MilitaryUnitMoveOrAttack(ai, unit, hex);
                        }
                    }
                }
            }

            if (ai.allDefenders.Contains(unitID))
            {
                if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] This unitName: {unit.name} with type {unit.unitType} is a defender, skipping general handling."); }
                continue; //this unit will be handled as a defender - with special AI
            }
            else if (ai.attackers.Contains(unitID))
            {
                if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] This unitName: {unit.name} with type {unit.unitType} is an attacker, skipping general handling."); }
                continue; //this unit will be handled as an attacker - with special AI
            }
            else if (unit.unitType=="Settler" || unit.unitType=="Founder")
            {
                continue;
            }
            else
            {
                if ((unit.unitClass & UnitClass.Recon) == UnitClass.Recon)
                {
                    if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] This unitName: {unit.name} with type {unit.unitType} is a scout."); }
                    HandleScout(ai,unit);
                }
                else if ((unit.unitClass & UnitClass.Combat) == UnitClass.Combat)
                {
                    if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] This unitName: {unit.name} with type {unit.unitType} is a military unitName."); }
                    HandleMilitary(ai,unit);
                }
                else
                {
                    throw new Exception("AI tried to manage a unitName it doesnt understand:"+unit.unitClass);

                }
            }
        }
        HandleDefenderUnitsForAllCities(ai);
        HandleAttackerUnits(ai);
    }
    private void HandleAttackerUnits(AI ai)
    {
        if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] Army/Attacker Management Subroutine"); }
        if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] Attacking?: {ai.isAttacking} Army size: {ai.attackers.Count} Desired army size: {GetDesiredArmySize(ai)}"); }
        
        if (ai.isAttacking)
        {
            if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] We are in attack mode!"); }
            if (ai.attackers.Count < GetDesiredArmySize(ai) * 0.2)
            {
                if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] Army dead, retreating."); }
                if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] Choosing new gather target"); }
                Unit unit = new Unit("Scout", 0, -ai.player.teamNum, ai.player.teamNum);
                unit.hex = ai.attackTarget;
                ai.gatherTarget = FindClosestFriendlyCity(ai, unit);
                ai.hasGatherTarget = true;
                ai.isAttacking = false;
                ai.hasAttackTarget = false;
                ai.attackTarget = new Hex();
                Global.gameManager.game.unitDictionary.Remove(-ai.player.teamNum);
            }
            else if (Global.gameManager.game.mainGameBoard.gameHexDict[ai.attackTarget].district.health <= 0)
            {
                if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] Attack t dead, retargeting."); }
                Unit unit = new Unit("Scout", 0, -ai.player.teamNum, ai.player.teamNum);
                unit.hex = ai.attackTarget;
                ai.attackTarget = FindClosestEnemyDistrict(ai, unit);
                GameHex h = Global.gameManager.game.mainGameBoard.gameHexDict[ai.attackTarget];
                if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] New Attack Target: {h.hex} with district {h.district.districtType} that has health {h.district.health}."); }
                Global.gameManager.game.unitDictionary.Remove(-ai.player.teamNum);
            }
        }
        else if (!ai.isAttacking)
        {
            if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] We are NOT attacking"); }
            if (ai.attackers.Count >= GetDesiredArmySize(ai))
            {
                if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] Army size hit, starting attack."); }
                Unit unit = new Unit("Scout", 0, -ai.player.teamNum, ai.player.teamNum);
                unit.hex = ai.gatherTarget;
                ai.attackTarget = FindClosestEnemyDistrict(ai, Global.gameManager.game.unitDictionary[-ai.player.teamNum]);
                GameHex h = Global.gameManager.game.mainGameBoard.gameHexDict[ai.attackTarget];
                if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] New Attack Target: {h.hex} with district {h.district.districtType} that has health {h.district.health}."); }
                ai.isAttacking = true;
                if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] Choosing new attack t"); }
                Global.gameManager.game.unitDictionary.Remove(-ai.player.teamNum);
            }
        }

        foreach (int attacker in ai.attackers.ToList())
        {
            Unit unit = Global.gameManager.game.unitDictionary[attacker];
            if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] UNIT: {unit.id} Attacker processing."); }
            if (!ai.player.unitList.Contains(attacker))
            {
                if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] UNIT: {unit.id} Attacker dead, updating record."); }
                ai.attackers.Remove(attacker);
                continue;
            }
            if (FindClosestAnyEnemyInRange(ai, unit.hex, 2, out Hex target))
            {
                if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}]UNIT: {unit.id} Soldier attacking nearby enemy."); }
                MilitaryUnitMoveOrAttack(ai, unit, target);
            }
            else if (ai.isAttacking)
            {
                if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] UNIT: {unit.id} Moving soldier to attack point."); }
                MilitaryUnitMoveOrAttack(ai, unit, ai.attackTarget);
            }
            else
            {
                if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}]  UNIT: {unit.id} Moving soldier to gather point."); }
                MilitaryUnitMoveOrAttack(ai, Global.gameManager.game.unitDictionary[attacker], ai.gatherTarget);
            }
        }
    }


    private void HandleDefenderUnitsForCity(AI ai, int cityID)
    {
        List<Hex> threats = new();
        switch (ai.AIDefenseArmyThreatStrategy)
        {
            case AIDefenseArmyThreatStrategy.CityThreatFirst:
                foreach (District district in Global.gameManager.game.cityDictionary[cityID].districts)
                {
                    List<Hex> localThreats = FindAllEnemiesInRange(ai, district.hex, 5);
                    foreach (Hex hex in localThreats)
                    {
                        if (!threats.Contains(hex))
                        {
                            if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] Adding new threat to city threat tracker"); }
                            threats.Add(hex);
                        }
                    }
                }
                foreach (int defender in ai.cities[cityID].defenderUnits.ToList())
                {
                    Unit unit = Global.gameManager.game.unitDictionary[defender];
                    if (!ai.player.unitList.Contains(defender))
                    {
                        if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] UNIT: {unit.id} Defender dead updating record."); }
                        ai.cities[cityID].defenderUnits.Remove(defender);
                        ai.allDefenders.Remove(defender);
                        continue;
                    }
                    if (threats.Count > 0)
                    {
                        if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] UNIT: {unit.id} Defender intercepting threat to city."); }
                        MilitaryUnitMoveOrAttack(ai, Global.gameManager.game.unitDictionary[defender], PickClosest(ai, threats, defender));
                    }
                    else
                    {
                        if (FindClosestAnyEnemyInRange(ai, unit.hex, 4, out Hex target))
                        {
                            if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] UNIT: {unit.id} Defender attacking nearby enemy."); }
                            MilitaryUnitMoveOrAttack(ai, unit, target);
                        }
                        else
                        {
                            MoveDefender(ai, unit,cityID);
                        }
                    }
                }
                break;
            case AIDefenseArmyThreatStrategy.NearbyThreatFirst:
                throw new NotImplementedException();
                break;
            case AIDefenseArmyThreatStrategy.DontUseDefenseArmy:
                throw new NotImplementedException();
                break;
            case AIDefenseArmyThreatStrategy.NONE:
                break;
            default:
                break;
        }

    }

    private void MoveDefender(AI ai, Unit defender, int cityID)
    {
        switch (ai.AIDefenseArmyMovementStrategy)
        {
            case AIDefenseArmyMovementStrategy.WaitNearCity:
                break;
            case AIDefenseArmyMovementStrategy.WanderWithinBorders:
                City city = Global.gameManager.game.cityDictionary[cityID];
                Hex t = city.heldHexes.ToList()[rng.Next(city.heldHexes.Count)];
                MilitaryUnitMoveOrAttack(ai, defender, t);
                break;
            case AIDefenseArmyMovementStrategy.SmartPositioning:
                throw new NotImplementedException();
                break;
            case AIDefenseArmyMovementStrategy.NONE:
                break;
            default:
                break;
        }
    }

    private void HandleDefenderUnitsForAllCities(AI ai)
    {
        foreach (int cityID in ai.player.cityList)
        {
            HandleDefenderUnitsForCity(ai, cityID);
        }
    }
    private void HandleMilitary(AI ai, Unit unit)
    {
        if ((unit.unitClass & UnitClass.Ranged) == UnitClass.Ranged)
        {
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id+"] Starting Ranged Military Handling"); }
            HandleRangedMilitary(ai, unit);
        }
        else if ((unit.unitClass & UnitClass.Infantry) == UnitClass.Infantry)
        {
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id+"] Starting Melee Military Handling"); }
            HandleMeleeMilitary(ai, unit);
        }
        else if (((unit.unitClass & UnitClass.Naval) == UnitClass.Naval))
        {
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id+"] Starting Naval Military Handling"); }
            HandleNavalMilitary(ai, unit);
        }
        else
        {
            throw new NotImplementedException($"Military Unit {unit.name} of class {unit.unitClass} is not handled in AI logic.");
        }
    }
    private void HandleNavalMilitary(AI ai, Unit unit)
    {

        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id+"] Naval fakeUnit AI not supported - sleeping fakeUnit"); }
        AIActivateAbility(ai, unit, "Sleep", unit.hex);
    }
    private void HandleRangedMilitary(AI ai, Unit unit)
    {
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Ranged Military subroutine"); }
        List<Hex> validMoves = unit.MovementRange().Keys.ToList<Hex>();
        Hex target;
        UnitAbility rangedAttack = unit.abilities.FirstOrDefault(a => a.name.Equals("RangedAttack"));

        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Using RANDOM_AGGRESSIVE ranged military strategy"); }
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Searching for t within range 6"); }
        if (FindClosestAnyEnemyInRange(ai, unit.hex, 6, out target))
        {
            MilitaryUnitMoveOrAttack(ai, unit, target);
        }
        else
        {
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] No Target in range - moving randomly."); }
            RandomMoveNoAttack(ai, unit, validMoves);
        }
 
    }

    private void HandleMeleeMilitary(AI ai, Unit unit)
    {
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Melee Military subroutine"); }
        List<Hex> validMoves = unit.MovementRange().Keys.ToList<Hex>();
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Using RANDOM_AGGRESSIVE melee military strategy"); }
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Searching for t within range 6"); }
        if (FindClosestAnyEnemyInRange(ai, unit.hex, 6, out Hex target))
        {
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Target found at location: " + target + " moving to/attacking t"); }
            AIMoveUnit(ai, unit, target);
        }
        else
        {
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] No Target in range - moving randomly."); }
            RandomMoveOrAttack(ai, unit, validMoves);
        }
    }
    private void HandleScout(AI ai, Unit unit)
    {
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Recon Unit subroutine"); }
        List<Hex> validMoves = unit.MovementRange().Keys.ToList<Hex>();
        RandomMoveNoAttack(ai, unit, validMoves);
    }
    private void HandleSettler(AI ai, Unit unit)
    {
        if(!ai.canSettleCities)
        {
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] This AI can't use settlers, but has one. Skipping it."); }
            return;
        }
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Settler Unit subroutine"); }
        List<Hex> validMoves = unit.MovementRange().Keys.ToList<Hex>();
        
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Checking to see if I can settle where I stand unitLocation: " + unit.hex); }
        if (unit.CanSettleHere(unit.hex, 3, new List<TerrainType>() { TerrainType.Flat, TerrainType.Rough }, false))
        {
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Current hex is valid to settle - settling"); }
            if (unit.unitType == "Founder")
            {
                AIActivateAbility(ai, unit, "SettleCapitalAbility", unit.hex);
            }
            else
            {
                AIActivateAbility(ai, unit, "SettleCityAbility", unit.hex);
            }

        }
        else
        {
            Hex target = new();
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Current hex is NOT VALID for settling - searching within range 6 for a valid settle hex"); }
           
            if (FindClosestValidSettleInRange(ai, unit, 9, out target))
            {
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Of these, picking the closest valid settle hex: "+target.ToString()+" - moving towards it"); }
                AIMoveUnit(ai, unit, target);
            }
            else
            {
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] No valid settle hex found within range 6 - moving randomly."); }
                RandomMoveNoAttack(ai, unit, validMoves);
            }
        }

    }
    private void HandleCities(AI ai)
    {
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Handle Cities subroutine"); }
        foreach (int cID in ai.cities.Keys)
        {
            if (!ai.player.cityList.Contains(cID))
            {
                ai.cities.Remove(cID);
            }
        }
        foreach (int cityID in ai.player.cityList)
        {
            City city = Global.gameManager.game.cityDictionary[cityID];
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Starting handling for cityID: " + city.id + " cityName: " + city.name); }

            if (!ai.cities.ContainsKey(city.id))
            {
                ai.cities.Add(city.id, new AICity(city.id));
            }

            if (city is Encampment e)
            {
                if (e.overlordTeamNum >= 0)
                {
                
                }
                switch (e.ownershipState)
                {
                    case FactionOwnership.Free:
                        break;
                    case FactionOwnership.Occupied:
                        break;
                    case FactionOwnership.Vassalized:
                        break;
                    default:
                        break;
                }
            }


            if (city.lastProducedUnitID != 0)
            {
                Unit lastProducedUnit = Global.gameManager.game.unitDictionary[city.lastProducedUnitID];
                if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] We finished building a unitName last turn. This unitName: {lastProducedUnit.name} with type {lastProducedUnit.unitType}."); }
                if (ai.cities[city.id].currentlyBuildingDefender)
                {
                    if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] We had requested a defender, assigning that unitName as defender"); }
                    ai.cities[city.id].defenderUnits.Add(lastProducedUnit.id);
                    ai.allDefenders.Add(lastProducedUnit.id);
                    ai.cities[city.id].currentlyBuildingDefender = false;
                }
                else if (ai.cities[city.id].currentlyBuildingAttacker)
                {
                    if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] We had requested a attacker, assigning that unitName as attacker"); }
                    ai.attackers.Add(lastProducedUnit.id);
                    ai.cities[city.id].currentlyBuildingAttacker = false;
                }
                city.lastProducedUnitID = 0;
            }



            if (city.productionQueue.Count == 0)
            {
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Production Queue Empty - selecting something"); }
                HandleCityProduction(ai, city);
            }
            else
            {
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Production Queue has item - skipping production"); }
            }

            if (city.readyToExpand > 0)
            {
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] City is ready to expand - selecting expansion tile"); }
                HandleCityExpansion(ai, city);
            }
            else
            {
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] city no ready to grow - skipping expansion"); }
            }
        }
    }
    private void HandleCityExpansion(AI ai, City city)
    {
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Handle city expansion subroutine"); }
        List<Hex> validRuralExpandHexes = city.ValidExpandHexes(new List<TerrainType> { TerrainType.Flat, TerrainType.Rough, TerrainType.Coast });
        List<Hex> validUrbanExpandHexes = city.ValidUrbanExpandHexes(new List<TerrainType> { TerrainType.Flat, TerrainType.Rough, TerrainType.Coast });
        List<Hex> allValidHexes = validRuralExpandHexes.Concat(validUrbanExpandHexes).ToList();
        switch (ai.AICityExpansionStrategy)
        {
            case AICityExpansionStrategy.NONE:
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] This AI cannot expand. Skipping."); }
                break;
            case AICityExpansionStrategy.Random:
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Using RANDOM Expansion Strategy"); }
                Global.gameManager.ExpandToHex(city.id, allValidHexes[rng.Next(allValidHexes.Count)]);
                break;
            case AICityExpansionStrategy.FocusResources:
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Using Focus Resources Expansion Strategy"); }
                foreach (Hex hex in validRuralExpandHexes)
                {
                    if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].resourceType != ResourceType.None)
                    {
                        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Found Resource in validExpandHexes, expanding to it."); }
                        Global.gameManager.ExpandToHex(city.id, hex);
                        return;
                    }
                }
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] No Resource in validExpandHexes, expanding randomly."); }
                if (rng.NextDouble()>0.5 && validRuralExpandHexes.Count>0)
                {
                    if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Expanding to random rural hex"); }
                    Global.gameManager.ExpandToHex(city.id, validRuralExpandHexes[rng.Next(validRuralExpandHexes.Count)]);
                }
                else if (validUrbanExpandHexes.Count>0)
                {
                    if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] developing random district"); }
                    Global.gameManager.DevelopDistrict(city.id, validUrbanExpandHexes[rng.Next(validUrbanExpandHexes.Count)], ai.player.allowedDistricts.ToList()[rng.Next(ai.player.allowedDistricts.Count)]);
                }
                else
                {
                    if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Can't expand anywhere sadge."); }
                }
                break;
            default:
                break;
        }

    }

    private void HandleCityProduction(AI ai, City city)
    {
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Handle city production subroutine"); }

        AICity aiCity = ai.cities[city.id];

        if (aiCity.defenderUnits.Count<ai.desiredDefendersPerCity)
        {
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] City needs defenders, building one."); }
            aiCity.currentlyBuildingDefender = true;
            Global.gameManager.AddToProductionQueue(city.id, PickRandomLandMilitaryUnit(ai, city), city.hex);
        }
        else if (ai.canBuildSettler && AssessSettlerNeed(ai))
        {
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] City has enough defenders, building a settler."); }
            Global.gameManager.AddToProductionQueue(city.id, "Settler", city.hex);
        }
        else if(AssessLocalEconNeed(ai,city,out string building))
        {
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] City needs defenders and we dont need a settler, fixing local econ."); }
            AttemptBuildingSpecificAtRandomValidHex(building, ai, city);
        }
        else
        {
            double econThreshold = 0.5f;
            switch (ai.AIPickProductionStrategy)
            {
                case AIPickProductionStrategy.Random:
                    econThreshold = 0.5f;
                    break;
                case AIPickProductionStrategy.OnlyEcon:
                    econThreshold = 1f;
                    break;
                case AIPickProductionStrategy.OnlyArmy:
                    econThreshold = 0f;
                    break;
                case AIPickProductionStrategy.FocusEcon:
                    econThreshold = .75f;
                    break;
                case AIPickProductionStrategy.FocusArmy:
                    econThreshold = 0.25f;
                    break;
                case AIPickProductionStrategy.Balanced:
                    econThreshold = 0.5f;
                    break;
                default:
                    econThreshold = 0.5f;
                    break;
            }
            double decision = rng.NextDouble();
            if (decision < econThreshold && city.ValidBuildings().Count > 0)
            {
                AssessGlobalEconNeed(ai, city, out string gBuilding);
                AttemptBuildingSpecificAtRandomValidHex(gBuilding, ai, city);
            }
            else
            {
                PickMilitaryUnit(ai, city, out string unit);
                Global.gameManager.AddToProductionQueue(city.id, unit, city.hex);
                aiCity.currentlyBuildingAttacker = true;
            }
        }
    }

    private bool AssessGlobalEconNeed(AI ai, City city, out string building)
    {
        List<string> listOfValidBuildings = city.ValidBuildings();
        building = listOfValidBuildings[rng.Next(listOfValidBuildings.Count)];
        return true;
    }

    private bool PickMilitaryUnit(AI ai, City city, out string unitName)
    {
        int melee = 0;
        int ranged = 0;
        int siege = 0;
        int naval = 0;
        unitName = null;
        foreach (int unitID in ai.attackers)
        {
            Unit unit = Global.gameManager.game.unitDictionary[unitID];
            if ((unit.unitClass & UnitClass.Naval) == UnitClass.Naval)
            {
                naval++;
            }
            else if ((unit.unitClass & UnitClass.Ranged) == UnitClass.Ranged)
            {
                ranged++;
            }
            else if ((unit.unitClass & UnitClass.Infantry) == UnitClass.Infantry)
            {
                melee++;
            }
            else if ((unit.unitClass & UnitClass.Siege) == UnitClass.Siege)
            {
                siege++;
            }
        }
        int total = melee + ranged + siege + naval;
        if (total <1)
        {
            total = 1;
        }
        float meleeRatio = melee / total;
        float rangedRatio = ranged / total;
        float siegeRatio = siege / total;
        float navalRatio = naval / total;   
        switch (ai.AIAttackArmyBuildingStrategy)
        {
            case AIAttackArmyBuildingStrategy.Random:
                break;
            case AIAttackArmyBuildingStrategy.RandomNoNaval:
                unitName = PickRandomLandMilitaryUnit(ai, city);
                break;
            case AIAttackArmyBuildingStrategy.AllRanged:
                break;
            case AIAttackArmyBuildingStrategy.AllSiege:
                break;
            case AIAttackArmyBuildingStrategy.AllMelee:
                break;
            case AIAttackArmyBuildingStrategy.AllNaval:
                break;
            case AIAttackArmyBuildingStrategy.FavorRanged:
                break;
            case AIAttackArmyBuildingStrategy.FavorSiege:
                break;
            case AIAttackArmyBuildingStrategy.FavorMelee:
                if (meleeRatio<0.75f)
                {
                    unitName = PickRandomMeleeMilitaryUnit(ai, city);
                }
                else
                {
                    unitName = PickRandomNonMeleeMilitaryUnit(ai, city);
                }
                return true;
            case AIAttackArmyBuildingStrategy.FavorNaval:
                break;
            case AIAttackArmyBuildingStrategy.Balanced:
                if (meleeRatio < 0.4f)
                {
                    unitName = PickRandomMeleeMilitaryUnit(ai, city);
                }
                else
                {
                    unitName = PickRandomNonMeleeMilitaryUnit(ai, city);
                }
                break;
            case AIAttackArmyBuildingStrategy.BalancedNoNaval:
                break;
            case AIAttackArmyBuildingStrategy.NoAttackArmy:
                break;
            default:
                break;
        }
        return false;
    }

    private int GetDesiredArmySize(AI ai)
    {
        return ai.desiredAttackersInArmy + (int)Math.Floor((ai.desiredAttackersInArmyPerTurnScaling * Global.gameManager.game.turnManager.currentTurn));
    }

    private bool AssessLocalEconNeed(AI ai, City city, out string toBuild)
    {
        toBuild = "";
        return false;
    }

    private bool AssessSettlerNeed(AI ai)
    {
        return GetNumberOfUnit(ai, "Settler") < 1;
    }

    private void HandleResearchAndCulture(AI ai)
    {
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Research and Culture subroutine"); }
        if (ai.canResearch)
        {
            if (ai.player.queuedResearch.Count == 0)
            {
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] No research queued - picking a random available one"); }
                string researchName = ai.player.AvaliableResearches()[rng.Next(ai.player.AvaliableResearches().Count)];
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Selected research: " + researchName); }
                Global.gameManager.SelectResearch(ai.player.teamNum, researchName);
            }
            else
            {
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Research already Queued, no change needed."); }
            }
        }
        else
        {
            if (AIDEBUG)
            {
                Global.Log("[AI#" + ai.player.teamNum + "] This AI doesn't do research. Skipping.");
            }
        }

        if (ai.canCulture)
        {
            if (ai.player.queuedCultureResearch.Count == 0)
            {
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] No culture queued - picking a random available one"); }
                string cultureName = ai.player.AvaliableCultureResearches()[rng.Next(ai.player.AvaliableCultureResearches().Count)];
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Selected culture: " + cultureName); }
                Global.gameManager.SelectCulture(ai.player.teamNum, cultureName);
            }
            else
            {
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Culture already Queued, no change needed."); }
            }
        }
        else
        {
            if (AIDEBUG)
            {
                Global.Log("[AI#" + ai.player.teamNum + "] This AI doesn't do Culture. Skipping.");
            }
        }

    }
    private AIPersonality PickMajorAIPersonality(AI ai, FactionType faction)
    {
        return AIPersonality.Standard;
    }
}


using Godot;
using ImGuiNET;
using NetworkMessages;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
public partial class AIManager: Node
{
    const bool AIDEBUG = false; 
    List<AI> aiList = new List<AI>();
    Random rng = new Random();

    //tired of calling the whole Global.gameManager.game.mainGameBoard.blah all the time
    int top;
    int bottom;
    int right;
    int left;
    private class AI
    {
        public Player player;
        public AIPersonality personality = AIPersonality.Standard;
        public AIUnitProductionStrategy unitProductionStrategy = AIUnitProductionStrategy.GameStart;
        public AIBuildingProductionStrategy buildingProductionStrategy = AIBuildingProductionStrategy.GameStart;
        public AICityExpansionStrategy cityExpansionStrategy = AICityExpansionStrategy.GameStart;
        public AICitySettlingStrategy citySettlingStrategy = AICitySettlingStrategy.GameStart;
        public AIMilitaryUnitStrategy militaryUnitStrategy = AIMilitaryUnitStrategy.GameStart;
        public AIScoutStrategy scoutStrategy = AIScoutStrategy.GameStart;
        public AIStrategicState strategicState = AIStrategicState.GameStart;
        public AIOverallProductionStrategy overallProductionStrategy = AIOverallProductionStrategy.GameStart;
        public List<Unit> defenders = new List<Unit>();
        public List<Unit> attackers = new List<Unit>();
        public Hex attackGather = new Hex();
        public Hex attackTarget = new Hex();
        public List<Hex> urgentDefenseTargets = new List<Hex>();

    }
    enum AIStrategicState
    {
        GameStart,
        War,
        Expansion,
        Economic
    }
    enum AIPersonality
    {
        Standard,
        Aggressive,
        Defensive,
        Economic,
        RANDOM
    }
    enum AIOverallProductionStrategy
    {

       GameStart,
        RANDOM
    }
    enum AIUnitProductionStrategy
    {
        GameStart,
        RANDOM
    }
    enum AIBuildingProductionStrategy
    {
        GameStart,
        RANDOM
    }
    enum AICityExpansionStrategy
    {
        GameStart,
        RANDOM
    }
    enum AICitySettlingStrategy
    {
        GameStart,
        ClosestValidSettle,
        ClosestRecommendedSettle,
        RANDOM
    }
    enum AIMilitaryUnitStrategy
    {
        GameStart,
        RANDOM,
        RANDOM_AGGRESSIVE,
    }
    enum AIScoutStrategy
    {
        GameStart,
        RANDOM
    }
    public override void _Ready()
    {
        top = Global.gameManager.game.mainGameBoard.top;
        bottom = Global.gameManager.game.mainGameBoard.bottom;
        right = Global.gameManager.game.mainGameBoard.right;
        left = Global.gameManager.game.mainGameBoard.left;
        foreach (Player player in Global.gameManager.game.playerDictionary.Values)
        {
            if (player.isAI)
            {
                AI ai = new AI { player = player };
                aiList.Add(ai);
                if (ai.player.teamNum == 0)
                {
                    if (AIDEBUG) { Global.Log("Skipping normal AI for team 0"); }
                }
                else
                {
                    if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] AI Created"); }
                    InitStrategy(ai);
                }

            }
        }
    }
    private void InitStrategy(AI ai)
    {
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Picking Strategies based on personality"); }
        switch (ai.personality)
        {
            case AIPersonality.RANDOM:
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Random Personality"); }
                ai.unitProductionStrategy = AIUnitProductionStrategy.RANDOM;
                ai.buildingProductionStrategy = AIBuildingProductionStrategy.RANDOM;
                ai.cityExpansionStrategy = AICityExpansionStrategy.RANDOM;
                ai.overallProductionStrategy = AIOverallProductionStrategy.RANDOM;
                ai.citySettlingStrategy = AICitySettlingStrategy.RANDOM;
                ai.militaryUnitStrategy = AIMilitaryUnitStrategy.RANDOM;
                ai.scoutStrategy = AIScoutStrategy.RANDOM;
                break;
            case AIPersonality.Standard:
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Standard Personality"); }
                ai.unitProductionStrategy = AIUnitProductionStrategy.RANDOM;
                ai.buildingProductionStrategy = AIBuildingProductionStrategy.RANDOM;
                ai.overallProductionStrategy = AIOverallProductionStrategy.GameStart;
                ai.cityExpansionStrategy = AICityExpansionStrategy.RANDOM;
                ai.citySettlingStrategy = AICitySettlingStrategy.ClosestValidSettle;
                ai.militaryUnitStrategy = AIMilitaryUnitStrategy.RANDOM_AGGRESSIVE;
                ai.scoutStrategy = AIScoutStrategy.RANDOM;
                break;
        }
    }
    public override void _Process(double delta)
    {
        foreach (AI ai in aiList)
        {


            if (!ai.player.turnFinished)
            {
                if (ai.player.teamNum == 0)
                {
                    if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Skipping AI processing for team 0"); }
                    EndAITurn(ai); //end turn for team 0 AI
                    continue; //skip AI for team 0
                }
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Hasn't ended turn - iterating through logic"); }
                //AssessGameState(ai);
                //PickStrategy(ai);
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Starting Unit Handling"); }
                HandleUnits(ai);
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Starting City Handling"); }
                HandleCities(ai);
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Starting Research/Culture Handling"); }
                HandleResearchAndCulture(ai);
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Starting End Turn process"); }
                EndAITurn(ai);
            }

        }
    }
    private void AssessGameState(AI ai)
    {
        AssignDefenders(ai);
        AssignAttackers(ai);
        IdentifyHighThreatDefensiveTargets(ai);

    }
    private void IdentifyHighThreatDefensiveTargets(AI ai)
    {
        foreach (int cityID in ai.player.cityList)
        {
            City city = Global.gameManager.game.cityDictionary[cityID];
            if (city.districts.Count > 0)
            {
                foreach (District district in city.districts)
                {
                    if (FindClosestAnyEnemyInRange(ai, district.hex, 2, out Hex target))
                    {
                        if (!ai.urgentDefenseTargets.Contains(target))
                        {
                            ai.urgentDefenseTargets.Add(target);
                        }
                    }
                }
            }
        }
    }
    private void AssignAttackers(AI ai)
    {
        return;
    }
    private void AssignDefenders(AI ai)
    {
        while (ai.defenders.Count < (2 * ai.player.cityList.Count))
        {
            foreach (int unitID in ai.player.unitList.ToList())
            {
                Unit unit = Global.gameManager.game.unitDictionary[unitID];
                if ((unit.unitClass & UnitClass.Combat) == UnitClass.Combat && !ai.defenders.Contains(unit))
                {
                    ai.defenders.Add(unit);
                    break;
                }
            }
        }
    }
    private void HandleResearchAndCulture(AI ai)
    {
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Research and Culture subroutine"); }
        if (ai.player.queuedResearch.Count==0)
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

        if (ai.player.queuedCultureResearch.Count==0)
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
    private void EndAITurn(AI ai)
    {
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] End Turn subroutine"); }
        if (!ai.player.turnFinished)
        {
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Ending turn"); }
            Global.gameManager.EndTurn(ai.player.teamNum);
        }
    }
    private void HandleCities(AI ai)
    {
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Handle Cities subroutine"); }
        foreach (int cityID in ai.player.cityList)
        {
            City city = Global.gameManager.game.cityDictionary[cityID];
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id+"] Starting handling for cityID: " + city.id + " cityName: " + city.name); }
            if (city.productionQueue.Count==0)
            {
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id+"] Production Queue Empty - selecting something"); }
                HandleCityProduction(ai,city);
            }
            else
            {
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Production Queue has item - skipping production"); }
            }
            if (city.readyToExpand>0)
            {
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id+"] City is ready to expand - selecting expansion tile"); }
                HandleCityExpansion(ai, city);
            }
            else
            {
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] city no ready to grow - skipping expansion"); }
            }
        }
    }
    private void HandleUnits(AI ai)
    {
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Handle Units subroutine"); }
        foreach (int unitID in ai.player.unitList.ToList())
        {
            Unit unit = Global.gameManager.game.unitDictionary[unitID];
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Starting handling for unitID: " + unitID + " which is a " + unit.name); }
            if (unit.name.Equals("Founder"))
            {
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id+"] Starting Founder handling"); }
                HandleFounder(ai, unit);
            }
            else if (unit.name.Equals("Settler"))
            {
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id+"] Starting Settler Handling"); }
                HandleSettler(ai, unit);
            }
            else if ((unit.unitClass & UnitClass.Recon) == UnitClass.Recon)
            {
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id+"] Starting Recon Handling"); }
                HandleScouts(ai, unit);
            }
            else if ((unit.unitClass & UnitClass.Combat) == UnitClass.Combat)
            {
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id+"] Starting Military Handling"); }
                HandleMilitary(ai, unit);
            }
            else
            {
                //what is this dude
                throw new NotImplementedException($"Unit {unit.name} of class {unit.unitClass} is not handled in AI logic.");
            }

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

        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id+"] Naval unit AI not supported - sleeping unit"); }
        AIActivateAbility(ai, unit, "Sleep", unit.hex);
    }
    private void HandleRangedMilitary(AI ai, Unit unit)
    {
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Ranged Military subroutine"); }
        List<Hex> validMoves = unit.MovementRange().Keys.ToList<Hex>();
        Hex target;
        UnitAbility rangedAttack = unit.abilities.FirstOrDefault(a => a.name.Equals("RangedAttack"));
        switch (ai.militaryUnitStrategy)
        {
            case AIMilitaryUnitStrategy.GameStart:
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Using GameStart ranged military strategy - UNIMPLEMENTED"); }
                break;
            case AIMilitaryUnitStrategy.RANDOM:
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Using RANDOM ranged military strategy"); }
                target = validMoves[rng.Next(validMoves.Count)];
                if (IsRangedAttackableHex(ai,target))
                {
                    if (target.WrapDistance(unit.hex) <= rangedAttack.range)
                    {
                        AIActivateAbility(ai, unit, "RangedAttack", target);
                        break;
                    }
                    else
                    {
                        AIMoveUnit(ai, unit, target);
                    }
                }
                else
                {
                    AIMoveUnit(ai, unit, target);
                }
                break;
            case AIMilitaryUnitStrategy.RANDOM_AGGRESSIVE:
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Using RANDOM_AGGRESSIVE ranged military strategy"); }
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Searching for target within range 6"); }
                if (FindClosestAnyEnemyInRange(ai, unit.hex, 6, out target))
                {
                    if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Target found at location: " + target); }
                    if (target.WrapDistance(unit.hex) <= rangedAttack.range)
                    {
                        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Target in range, attacking with ranged attack"); }
                        AIActivateAbility(ai, unit, "RangedAttack", target);
                    }
                    else
                    {
                        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Target in NOT range, moving closer"); }
                        AIMoveUnit(ai, unit, target);
                    }
                }
                else
                {
                    if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] No Target in range - moving randomly."); }
                    RandomMoveNoAttack(ai, unit, validMoves);
                }
                break;
            default:
                throw new NotImplementedException($"Military strategy {ai.militaryUnitStrategy} is not implemented.");
        }
    }

    private void AIActivateAbility(AI ai, Unit unit, string abilityName, Hex target)
    {
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] AI has activated an Ability: " + abilityName + " unitID: " + unit.id + " unitName: " + unit.name + " unitLocation: " + unit.hex + " targetLocation: " + target); }
        Global.gameManager.ActivateAbility(unit.id, abilityName, target);
    }

    private void HandleMeleeMilitary(AI ai, Unit unit)
    {
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Melee Military subroutine"); }
        List<Hex> validMoves = unit.MovementRange().Keys.ToList<Hex>();
        switch (ai.militaryUnitStrategy)
        {
            case AIMilitaryUnitStrategy.GameStart:
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Using gamestart melee military strategy"); }
                break;
            case AIMilitaryUnitStrategy.RANDOM:
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Using RANDOM melee military strategy"); }
                RandomMoveOrAttack(ai, unit, validMoves);
                break;
            case AIMilitaryUnitStrategy.RANDOM_AGGRESSIVE:
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Using RANDOM_AGGRESSIVE melee military strategy"); }
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Searching for target within range 6"); }
                if (FindClosestAnyEnemyInRange(ai, unit.hex, 6, out Hex target))
                {
                    if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Target found at location: " + target + " moving to/attacking target"); }
                    AIMoveUnit(ai, unit, target);
                }
                else
                {
                    if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] No Target in range - moving randomly."); }
                    RandomMoveOrAttack(ai, unit, validMoves);
                }
                break;
            default:
                throw new NotImplementedException($"Military strategy {ai.militaryUnitStrategy} is not implemented.");
        }

    }

    private void AIMoveUnit(AI ai, Unit unit, Hex target)
    {
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] AI has requested to move a unit. unitID: " + unit.id + " unitName: " + unit.name + " unitLocation: " + unit.hex + " moveTo: " + target); }
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

    private void RandomMoveOrAttack(AI ai, Unit unit, List<Hex> validMoves)
    {
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Picking a random hex from validMoves and moving to it - even if it has an enemy."); }
        Hex target = validMoves[rng.Next(validMoves.Count)]; 
        AIMoveUnit(ai, unit, target);
    }

    private void RandomMoveNoAttack(AI ai, Unit unit, List<Hex> validMoves)
    {
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "] Picking a random hex from validMoves and moving to it - only if it has no enemy"); }
        List<Hex> safeMoves = new List<Hex>();
        foreach (Hex hex in validMoves.ToList())
        {
            if (IsSafeHex(ai,hex))
            {
                safeMoves.Add(hex);
            }
        }
        Hex target = safeMoves[rng.Next(safeMoves.Count)];
        AIMoveUnit(ai, unit, target);
    }
    private bool FindClosestAnyEnemyInRange(AI ai, Hex hex, int range, out Hex target)
    {
        Unit unit = new Unit("Warrior",-1,ai.player.teamNum); 
        unit.hex = hex; //create a dummy unit at the given hex
        List<Hex> hexesInRange = unit.hex.WrappingRange(range, left, right, top, bottom);
        List<Hex> targets = new();
        foreach (Hex h in hexesInRange)
        {
            if (!IsSafeHex(ai,hex))
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
            Global.gameManager.game.unitDictionary.Remove(-1);
            return true;
        }
        else
        {
            Global.gameManager.game.unitDictionary.Remove(-1);
            target = new Hex(); //return an empty hex if no district found
            return false; //no enemy district found in range
        }
    }

    private bool IsEnemy(int selfTeam, int otherTeam)
    {
        return Global.gameManager.game.teamManager.GetEnemies(selfTeam).Contains(otherTeam);
    }

    private bool IsSafeHex(AI ai, Hex hex)
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

    private bool IsMeleeAttackableHex(AI ai, Hex hex)
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

    private bool IsRangedAttackableHex(AI ai, Hex hex)
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

    private bool FindClosestValidSettleInRange(AI ai, Unit unit, int range, out Hex target)
    {
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] FindClosestValidSettleInRange Function Start"); }
        List<Hex> hexesInRange = unit.hex.WrappingRange(range, left, right, top, bottom);
        //if (AIDEBUG) { Global.Log("    [AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] WrappingRange provides the following set of hexes in range: " + string.Join(",",hexesInRange)); }
        List<Hex> targets = new();
        foreach (Hex h in hexesInRange)
        {
            if (unit.CanSettleHere(h, 3 , new List<TerrainType>(){ TerrainType.Flat, TerrainType.Rough}, false))
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
    private void HandleScouts(AI ai, Unit unit)
    {
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Recon Unit subroutine"); }
        List<Hex> validMoves = unit.MovementRange().Keys.ToList<Hex>();
        switch (ai.scoutStrategy)
        {
            case AIScoutStrategy.GameStart:
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Using GameStart Recon strategy"); }
                break;
            case AIScoutStrategy.RANDOM:
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Using RANDOM Recon subroutine"); }
                RandomMoveNoAttack(ai, unit, validMoves);
                break;
            default:
                break;
        }
    }
    private void HandleSettler(AI ai, Unit unit)
    {
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Settler Unit subroutine"); }
        List<Hex> validMoves = unit.MovementRange().Keys.ToList<Hex>();
        Hex target;
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Checking to see if I can settle where I stand unitLocation: " + unit.hex); }
        if (unit.CanSettleHere(unit.hex, 3, new List<TerrainType>() { TerrainType.Flat, TerrainType.Rough }, false))
        {
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Current hex is valid to settle - settling"); }
            AIActivateAbility(ai, unit, "SettleCityAbility", unit.hex);
        }
        else
        {
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Current hex is NOT VALID for settling - searching within range 6 for a valid settle hex"); }
            if (FindClosestValidSettleInRange(ai, unit, 6, out target))
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

        /* Global.Log("Starting Settler Handler");
        List<Hex> validMoves = unit.MovementRange().Keys.ToList<Hex>();
        Hex target;
        switch (ai.citySettlingStrategy)
        {
            case AICitySettlingStrategy.GameStart:
                break;
            case AICitySettlingStrategy.ClosestValidSettle:
                if (unit.CanSettleHere(unit.hex, 3))
                {
                    Global.Log("Settler: settling here.");
                    AIActivateAbility(ai, unit, "SettleCityAbility", unit.hex);
                }
                else if (FindClosestValidSettleInRange(ai, unit, 6, out target))
                {
                    Global.Log("Settler: Can't settle here, moving towards the closest valid spot.");
                    AIMoveUnit(ai, unit, target);
                }
                else
                {
                    Global.Log("Settler: No valid settle found, moving randomly.");
                    RandomMoveNoAttack(ai, unit, validMoves);
                }
                break;
            case AICitySettlingStrategy.RANDOM:
                if (unit.CanSettleHere(unit.hex,3))
                {
                    AIActivateAbility(ai, unit, "SettleCityAbility", unit.hex);
                }
                else
                {
                    RandomMoveNoAttack(ai, unit, validMoves);
                }
                break;
            default:
                break;
        }*/
    }
    private void HandleFounder(AI ai, Unit unit)
    {
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Founder Unit subroutine"); }
        List<Hex> validMoves = unit.MovementRange().Keys.ToList<Hex>();
        Hex target;
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Checking to see if I can settle where I stand unitLocation: " + unit.hex); }
        if (unit.CanSettleHere(unit.hex, 3, new List<TerrainType>() { TerrainType.Flat, TerrainType.Rough }, false))
        {
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Current hex is valid to settle - settling"); }
            AIActivateAbility(ai, unit, "SettleCapitalAbility", unit.hex);
        }
        else 
        {
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Current hex is NOT VALID for settling - searching within range 6 for a valid settle hex"); }
            if (FindClosestValidSettleInRange(ai, unit, 6, out target))
            {
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] valid settle hex: " + target.ToString() + " within range 6 found - moving towards it"); }
                AIMoveUnit(ai, unit, target);
            }
            else
            {
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] No valid settle hex found within range 6 - moving randomly."); }
                RandomMoveNoAttack(ai, unit, validMoves);
            }
        }

    }
    private void HandleCityExpansion(AI ai, City city)
    {
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Handle city expansion subroutine"); }
        switch (ai.cityExpansionStrategy)
        {
            case AICityExpansionStrategy.GameStart:
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Using GameStart Expansion Strategy"); }
                break;
            case AICityExpansionStrategy.RANDOM:
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Using RANDOM Expansion Strategy"); }
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Generating List of valid rural expansion hexes"); }
                List<Hex> validExpandHexes = city.ValidExpandHexes(new List<TerrainType> { TerrainType.Flat, TerrainType.Rough, TerrainType.Coast });
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] List of valid rural expansion hexes: " + string.Join(",",validExpandHexes)); }
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Generating List of valid urban expansion hexes"); }
                List<Hex> validUrbanExpandHexes = city.ValidUrbanExpandHexes(new List<TerrainType> { TerrainType.Flat, TerrainType.Rough, TerrainType.Coast });
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] List of valid urban expansion hexes: " + string.Join(",", validUrbanExpandHexes)); }
                List<Hex> allValidHexes = validExpandHexes.Concat(validUrbanExpandHexes).ToList();
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Full list of valid expansion hexes: " + string.Join(",", allValidHexes)); }
                Hex target = allValidHexes[rng.Next(allValidHexes.Count)];
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Expanding to random target from list: " + target); }
                Global.gameManager.ExpandToHex(city.id, target);
                break;
            default:
                break;
        }


    }
    private void HandleCityProduction(AI ai, City city)
    {
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Handle city production subroutine"); }
        switch (ai.overallProductionStrategy)
        {
            case AIOverallProductionStrategy.GameStart:
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Using GameStart production Strategy"); }
                if (GetNumberOfUnit(ai, "Settler") < 1)
                {
                    if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Team has 0 settlers - building a settler"); }
                    Global.gameManager.AddToProductionQueue(city.id, "Settler", city.hex);
                }
                else if (GetNumberOfUnit(ai, "Warrior") < 1)
                {
                    if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Team has 0 warriors - building a warrior"); }
                    Global.gameManager.AddToProductionQueue(city.id, "Warrior", city.hex);
                }
                else if (GetNumberOfUnit(ai, "Slinger") < 1)
                {
                    if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Team has 0 slingers - building a slinger"); }
                    Global.gameManager.AddToProductionQueue(city.id, "Slinger", city.hex);
                }
                else if (GetNumberOfBuildingInCity(ai, city, "Granary") < 1)
                {
                    if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] City has 0 Granary - building a granary"); }
                    if (!AttemptBuildingSpecificAtRandomValidHex("Granary", ai, city))
                    {
                        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] failed to build a granary - falling back on random production"); }
                        RandomCityProduction(ai, city);
                    }
                }
                else if (GetNumberOfUnit(ai, "Settler") < 1)
                {
                    if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Team has 0 settlers - building a settler"); }
                    Global.gameManager.AddToProductionQueue(city.id, "Settler", city.hex);
                }
                else
                {
                    if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] all GameStart requirements met - producing a random item instead"); }
                    RandomCityProduction(ai, city);
                }
                break;
            case AIOverallProductionStrategy.RANDOM:
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Using RANDOM production Strategy"); }
                RandomCityProduction(ai, city);
                break;
            default:
                break;
        }

    }
    private void RandomCityProduction(AI ai, City city)
    {
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Attempting to select a random valid production item."); }
        if (city.ValidUnits().Count > 0 && rng.NextDouble() < 0.5)
        {
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Randomly picked UNIT"); }
            ProduceUnit(ai, city);
        }
        else if (city.ValidBuildings().Count > 0 && GetDistrictsWithOpenSlots(ai, city).Count > 0)
        {
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Randomly picked BUILDING"); }
            ProduceBuilding(ai, city);
        }
        else if (city.ValidUnits().Count > 0)
        {
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Tried to produce a building but failed - falling back to unit"); }
            ProduceUnit(ai, city);
        }
        else
        {
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] failed to produce anything"); }
            //cant build anything
            return;
        }
    }
    private int GetNumberOfUnit(AI ai, string name)
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
    private int GetNumberOfBuildingInCity(AI ai, City city, string name)
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
    private void ProduceBuilding(AI ai, City city)
    {
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] City choosing building to build..."); }
        switch (ai.buildingProductionStrategy)
        {
            case AIBuildingProductionStrategy.GameStart:
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Using GameStart building strategy"); }
                break;
            case AIBuildingProductionStrategy.RANDOM:
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Using RANDOM building strategy"); }
                List<string> listOfValidBuildings = city.ValidBuildings();
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Start recursive building attempts..."); }
                AttemptAnyBuildingAtRandomValidHex(listOfValidBuildings, ai, city);
                break;
            default:
                break;
        }
    }
    private void AttemptAnyBuildingAtRandomValidHex(List<string> listOfValidBuildings, AI ai, City city)
    {
        
        if (listOfValidBuildings.Count == 0)
        {
            //no valid buildings left
            return;
        }

        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Recursive building attempt. List of valid buildings to try: " + string.Join(",",listOfValidBuildings)); }

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
    private bool AttemptBuildingSpecificAtRandomValidHex(string buildingName, AI ai, City city)
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
    private List<District> GetDistrictsWithOpenSlots(AI ai, City city)
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
    private void ProduceUnit(AI ai, City city)
    {
        switch (ai.unitProductionStrategy)
        {
            case AIUnitProductionStrategy.GameStart:
                break;
            case AIUnitProductionStrategy.RANDOM:
                List<string> listOfValidUnits = city.ValidUnits();
                string unit = listOfValidUnits[rng.Next(listOfValidUnits.Count)];
                Global.gameManager.AddToProductionQueue(city.id, unit, city.hex);
                break;
            default:
                break;
        }



    }
    private void PickStrategy(AI ai)
    {

    }
}


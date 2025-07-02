using Godot;
using ImGuiNET;
using NetworkMessages;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static AIUtils;
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
                ai.desiredDefendersPerCity = 2;
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
                ai.desiredDefendersPerCity = 2;
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
    }

    private void HandleSettlers(AI ai)
    {
        foreach (int unitID in ai.player.unitList.ToList())
        {
            Unit unit = Global.gameManager.game.unitDictionary[unitID];
            if (unit.unitType == "Settler" || unit.unitType == "Founder")
            {
                if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] This unit: {unit.name} with type {unit.unitType} is a settler."); }
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
            if (ai.allDefenders.Contains(unitID))
            {
                if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] This unit: {unit.name} with type {unit.unitType} is a defender, skipping general handling."); }
                continue; //this unit will be handled as a defender - with special AI
            }
            else if (ai.attackers.Contains(unitID))
            {
                if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] This unit: {unit.name} with type {unit.unitType} is an attacker, skipping general handling."); }
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
                    if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] This unit: {unit.name} with type {unit.unitType} is a scout."); }
                    HandleScout(ai,unit);
                }
                else if ((unit.unitClass & UnitClass.Combat) == UnitClass.Combat)
                {
                    if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] This unit: {unit.name} with type {unit.unitType} is a military unit."); }
                    HandleMilitary(ai,unit);
                }
                else
                {
                    throw new Exception("AI tried to manage a unit it doesnt understand:"+unit.unitClass);

                }
            }
        }
        HandleDefenderUnitsForAllCities(ai);
        HandleAttackerUnits(ai);
    }
    private void HandleAttackerUnits(AI ai)
    {
        if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] Army/Attacker Management Subroutine"); }
        if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] Current Army Size: {ai.attackers.Count}, waiting to hit army size: {GetDesiredArmySize(ai)}"); }
        if (ai.attackers.Count>=GetDesiredArmySize(ai))
        {
            if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] Army size hit, starting attack."); }
            ai.isAttacking = true;
        }
        if (ai.attackers.Count<GetDesiredArmySize(ai)*0.2)
        {
            if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] Army dead, retreating."); }
            ai.isAttacking = false;
            ai.hasAttackTarget = false;
            ai.hasGatherTarget = false;
        }    
        foreach (int attacker in ai.attackers)
        {
            if (!ai.player.unitList.Contains(attacker))
            {
                if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] Attacker dead, updating record."); }
                ai.attackers.Remove(attacker);
                continue;
            }


            if (!ai.hasGatherTarget)
            {
                if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] Choosing new gather target"); }
                ai.gatherTarget = FindClosestFriendlyCity(ai, Global.gameManager.game.unitDictionary[attacker]);
            }

            if (ai.isAttacking)
            {
                if (!ai.hasAttackTarget)
                {
                    ai.attackTarget = FindClosestEnemyDistrict(ai, Global.gameManager.game.unitDictionary[attacker]);
                    if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] Choosing new attack target"); }
                }
                if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] Moving soldier to attack point."); }
                MilitaryUnitMoveOrAttack(ai, Global.gameManager.game.unitDictionary[attacker], ai.attackTarget);
            }
            else
            {

                MilitaryUnitMoveOrAttack(ai, Global.gameManager.game.unitDictionary[attacker], ai.gatherTarget);
            }
        }
    }

    private void HandleDefenderUnitsForCity(AI ai, int cityID)
    {
        List<Hex> threats = new();
        foreach (District district in Global.gameManager.game.cityDictionary[cityID].districts)
        {
            List<Hex> localThreats = FindAllEnemiesInRange(ai, district.hex, 3);
            foreach (Hex hex in localThreats)
            {
                if (!threats.Contains(hex))
                {
                    threats.Add(hex);
                }
            }
        }

        foreach (int defender in ai.cities[cityID].defenderUnits)
        {
            if (!ai.player.unitList.Contains(defender))
            {
                ai.cities[cityID].defenderUnits.Remove(defender);
                ai.allDefenders.Remove(defender);
                continue;
            }
            if (threats.Count > 0)
            {
                MilitaryUnitMoveOrAttack(ai, Global.gameManager.game.unitDictionary[defender], PickClosest(ai, threats, defender));
            }
            else
            {
                City city = Global.gameManager.game.cityDictionary[cityID];
                Hex target = city.heldHexes.ToList()[rng.Next(city.heldHexes.Count)];
                MilitaryUnitMoveOrAttack(ai, Global.gameManager.game.unitDictionary[defender], target);
            }
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
        switch (ai.militaryUnitStrategy)
        {
            case AIMilitaryUnitStrategy.GameStart:
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Using GameStart ranged military strategy - UNIMPLEMENTED"); }
                break;
            case AIMilitaryUnitStrategy.RANDOM:
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Using RANDOM ranged military strategy"); }
                target = validMoves[rng.Next(validMoves.Count)];
                MilitaryUnitMoveOrAttack(ai, unit, target);
                break;
            case AIMilitaryUnitStrategy.RANDOM_AGGRESSIVE:
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Using RANDOM_AGGRESSIVE ranged military strategy"); }
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Searching for target within range 6"); }
                if (FindClosestAnyEnemyInRange(ai, unit.hex, 6, out target))
                {
                    MilitaryUnitMoveOrAttack(ai,unit,target);
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
    private void HandleMeleeMilitary(AI ai, Unit unit)
    {
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Melee Military subroutine"); }
        List<Hex> validMoves = unit.MovementRange().Keys.ToList<Hex>();
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
    }
    private void HandleScout(AI ai, Unit unit)
    {
        if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][UNIT:" + unit.id + "] Recon Unit subroutine"); }
        List<Hex> validMoves = unit.MovementRange().Keys.ToList<Hex>();
        RandomMoveNoAttack(ai, unit, validMoves);
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

            

            if (city.lastProducedUnitID != 0)
            {
                Unit lastProducedUnit = Global.gameManager.game.unitDictionary[city.lastProducedUnitID];
                if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] We finished building a unit last turn. This unit: {lastProducedUnit.name} with type {lastProducedUnit.unitType}."); }
                if (ai.cities[city.id].currentlyBuildingDefender)
                {
                    if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] We had requested a defender, assigning that unit as defender"); }
                    ai.cities[city.id].defenderUnits.Add(lastProducedUnit.id);
                    ai.allDefenders.Add(lastProducedUnit.id);
                    ai.cities[city.id].currentlyBuildingDefender = false;
                }
                else if (ai.cities[city.id].currentlyBuildingAttacker)
                {
                    if (AIDEBUG) { Global.Log($"[AI#{ai.player.teamNum}] We had requested a attacker, assigning that unit as attacker"); }
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

        AICity aiCity = ai.cities[city.id];

        if (aiCity.defenderUnits.Count<ai.desiredDefendersPerCity)
        {
            if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] City needs defenders, building one."); }
            aiCity.currentlyBuildingDefender = true;
            Global.gameManager.AddToProductionQueue(city.id, PickRandomLandMilitaryUnit(ai, city), city.hex);
        }
        else if (AssessSettlerNeed(ai))
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
            double decision = rng.NextDouble();
            if (decision < 0.5f && AssessMilitaryNeed(ai, city, out string unit))
            {
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Defenders,Settlers,LocalEcon all good - building army."); }
                Global.gameManager.AddToProductionQueue(city.id, unit, city.hex);
                aiCity.currentlyBuildingAttacker = true;
            }
            else if (AssessGlobalEconNeed(ai, city, out string gBuilding))
            {
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Defenders,Settlers,LocalEcon all good - building global econ."); }
                AttemptBuildingSpecificAtRandomValidHex(gBuilding, ai, city);
            }
            else if (city.ValidBuildings().Count>0)
            {
                if (AIDEBUG) { Global.Log("[AI#" + ai.player.teamNum + "][CITY:" + city.id + "] Random build fall back"); }
                //random build fallback
                List<string> listOfValidBuildings = city.ValidBuildings();
                AttemptAnyBuildingAtRandomValidHex(listOfValidBuildings, ai, city);
            }
            else
            {
                throw new Exception("AI literally cant build anything");
            }
        }
    }

    private bool AssessGlobalEconNeed(AI ai, City city, out string building)
    {
        List<string> listOfValidBuildings = city.ValidBuildings();
        building = listOfValidBuildings[rng.Next(listOfValidBuildings.Count)];
        return true;
    }

    private bool AssessMilitaryNeed(AI ai, City city, out string unit)
    {
        if (ai.attackers.Count<GetDesiredArmySize(ai))
        {
            unit = PickRandomLandMilitaryUnit(ai,city);
            return true;
        }
        else
        {
            unit = null;
            return false;
        }
    }

    private int GetDesiredArmySize(AI ai)
    {
        return (int)Math.Floor(1+(0.15*Global.gameManager.game.turnManager.currentTurn));
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




}


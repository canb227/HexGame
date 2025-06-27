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
                AI ai = new AI { player = player};
                aiList.Add(ai);
                InitStrategy(ai);
            }
        }
    }
    private void InitStrategy(AI ai)
    {
        switch (ai.personality)
        {
            case AIPersonality.RANDOM:
                ai.unitProductionStrategy = AIUnitProductionStrategy.RANDOM;
                ai.buildingProductionStrategy = AIBuildingProductionStrategy.RANDOM;
                ai.cityExpansionStrategy = AICityExpansionStrategy.RANDOM;
                ai.overallProductionStrategy = AIOverallProductionStrategy.RANDOM;
                ai.citySettlingStrategy = AICitySettlingStrategy.RANDOM;
                ai.militaryUnitStrategy = AIMilitaryUnitStrategy.RANDOM;
                ai.scoutStrategy = AIScoutStrategy.RANDOM;
                break;
            case AIPersonality.Standard:
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
                //AssessGameState(ai);
                //PickStrategy(ai);
                HandleUnits(ai);
                HandleCities(ai);
                HandleResearchAndCulture(ai);
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
                    if (FindClosestEnemyUnitInRange(ai, district.hex, 2, out Hex target))
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
        if (ai.player.queuedResearch.Count==0)
        {
            string researchName = ai.player.AvaliableResearches()[rng.Next(ai.player.AvaliableResearches().Count)];
            Global.gameManager.SelectResearch(ai.player.teamNum, researchName);
        }

        if (ai.player.queuedCultureResearch.Count==0)
        {
            string cultureName = ai.player.AvaliableCultureResearches()[rng.Next(ai.player.AvaliableCultureResearches().Count)];
            Global.gameManager.SelectCulture(ai.player.teamNum, cultureName);
        }
    }
    private void EndAITurn(AI ai)
    {
        if (!ai.player.turnFinished)
        {
            Global.gameManager.EndTurn(ai.player.teamNum);
        }
    }
    private void HandleCities(AI ai)
    {
        foreach (int cityID in ai.player.cityList)
        {
            City city = Global.gameManager.game.cityDictionary[cityID];
            if (city.productionQueue.Count==0)
            {
                Global.Log("AI producing something");
                HandleCityProduction(ai,city);
            }
            if (city.readyToExpand>0)
            {
                Global.Log("AI city expansion");
                HandleCityExpansion(ai, city);
            }
        }
    }
    private void HandleUnits(AI ai)
    {
        foreach (int unitID in ai.player.unitList.ToList())
        {
            Unit unit = Global.gameManager.game.unitDictionary[unitID];
            if (unit.name.Equals("Founder"))
            {
                HandleFounder(ai, unit);
            }
            else if (unit.name.Equals("Settler"))
            {
                HandleSettler(ai, unit);
            }
            else if ((unit.unitClass & UnitClass.Recon) == UnitClass.Recon)
            {
                HandleScouts(ai, unit);
            }
            else if ((unit.unitClass & UnitClass.Combat) == UnitClass.Combat)
            {
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
            HandleRangedMilitary(ai, unit);
        }
        else if ((unit.unitClass & UnitClass.Infantry) == UnitClass.Infantry)
        {
            HandleMeleeMilitary(ai, unit);
        }
        else if (((unit.unitClass & UnitClass.Naval) == UnitClass.Naval))
        {
            HandleNavalMilitary(ai, unit);
        }
    }
    private void HandleNavalMilitary(AI ai, Unit unit)
    {
        Global.gameManager.ActivateAbility(unit.id, "Sleep", unit.hex);
    }
    private void HandleRangedMilitary(AI ai, Unit unit)
    {
        List<Hex> validMoves = unit.MovementRange().Keys.ToList<Hex>();
        Hex target;
        bool isEnemyPresent = false;
        UnitAbility rangedAttack = unit.abilities.FirstOrDefault(a => a.name.Equals("RangedAttack"));
        switch (ai.militaryUnitStrategy)
        {
            case AIMilitaryUnitStrategy.GameStart:
                break;
            case AIMilitaryUnitStrategy.RANDOM:
                target = validMoves[rng.Next(validMoves.Count)];
                if (Global.gameManager.game.mainGameBoard.gameHexDict[target].units.Count > 0)
                {
                    foreach (int unitID in Global.gameManager.game.mainGameBoard.gameHexDict[target].units)
                    {
                        if (Global.gameManager.game.unitDictionary[unitID].teamNum != ai.player.teamNum)
                        {
                            isEnemyPresent = true;
                        }
                    }
                }
                if (target.WrapDistance(unit.hex)<=rangedAttack.range)
                {
                    Global.gameManager.ActivateAbility(unit.id, "RangedAttack", target);
                }
                else
                {
                    Global.gameManager.MoveUnit(unit.id, target, isEnemyPresent);
                }
                break;
            case AIMilitaryUnitStrategy.RANDOM_AGGRESSIVE:
                if (FindClosestEnemyUnitInRange(ai, unit.hex, 6, out target))
                {
                    if (target.WrapDistance(unit.hex) <= rangedAttack.range)
                    {
                        Global.gameManager.ActivateAbility(unit.id, "RangedAttack", target);
                    }
                    else
                    {
                        Global.gameManager.MoveUnit(unit.id, target, isEnemyPresent);
                    }
                    Global.gameManager.MoveUnit(unit.id, target, true);
                }
                else
                {
                    //no enemy unit found in range, just move randomly
                    target = validMoves[rng.Next(validMoves.Count)];
                    Global.gameManager.MoveUnit(unit.id, target, false);
                }
                break;
            default:
                throw new NotImplementedException($"Military strategy {ai.militaryUnitStrategy} is not implemented.");
        }
    }
    private void HandleMeleeMilitary(AI ai, Unit unit)
    {
        List<Hex> validMoves = unit.MovementRange().Keys.ToList<Hex>();
        Hex target;
        bool isEnemyPresent = false;
        switch (ai.militaryUnitStrategy)
        {
            case AIMilitaryUnitStrategy.GameStart:
                break;
            case AIMilitaryUnitStrategy.RANDOM:
                target = validMoves[rng.Next(validMoves.Count)];
                if (Global.gameManager.game.mainGameBoard.gameHexDict[target].units.Count > 0)
                {
                    foreach (int unitID in Global.gameManager.game.mainGameBoard.gameHexDict[target].units)
                    {
                        if (Global.gameManager.game.unitDictionary[unitID].teamNum != ai.player.teamNum)
                        {
                            isEnemyPresent = true;
                        }
                    }
                }
                Global.gameManager.MoveUnit(unit.id, target, isEnemyPresent);
                break;
            case AIMilitaryUnitStrategy.RANDOM_AGGRESSIVE:
                if (FindClosestEnemyUnitInRange(ai, unit.hex, 6, out target))
                {
                    Global.gameManager.MoveUnit(unit.id, target, true);
                }
                else
                {
                    //no enemy unit found in range, just move randomly
                    target = validMoves[rng.Next(validMoves.Count)];
                    Global.gameManager.MoveUnit(unit.id, target, false);
                }
                break;
            default:
                throw new NotImplementedException($"Military strategy {ai.militaryUnitStrategy} is not implemented.");
        }

    }
    private bool FindClosestEnemyUnitInRange(AI ai, Hex hex, int range, out Hex target)
    {
        Unit unit = new Unit("Warrior",-1,ai.player.teamNum); 
        unit.hex = hex; //create a dummy unit at the given hex
        List<Hex> hexesInRange = unit.hex.WrappingRange(range, left, right, top, bottom);
        List<Hex> targets = new();
        foreach (Hex h in hexesInRange)
        {
            if (Global.gameManager.game.mainGameBoard.gameHexDict[h].units.Count>0)
            {
                foreach (int unitID in Global.gameManager.game.mainGameBoard.gameHexDict[h].units)
                {
                    if (IsEnemy(Global.gameManager.game.unitDictionary[unitID].teamNum))
                    {
                        targets.Add(h); //found an enemy unit in range
                        break; //no need to check other units in this hex
                    }
                }
            }
        }
        if (targets.Count > 0)
        {
            foreach (Hex h in targets)
            {
                float lowCost = float.MaxValue;
                float cost = 0f;
                unit.PathFind(unit.hex, h, Global.gameManager.game.teamManager, unit.movementCosts, unit.movementSpeed, out cost);
                if (cost < lowCost)
                {
                    target = h; //find the closest district
                    return true;
                }
            }
        }
        Global.gameManager.game.unitDictionary.Remove(-1);
        target = new Hex(); //return an empty hex if no district found
        return false; //no enemy district found in range
    }
    private bool FindClosestAnyTargetInRange(AI ai, Unit unit, int range, out Hex target)
    {
        List<Hex> hexesInRange = unit.hex.WrappingRange(range, left, right, top, bottom);
        List<Hex> targets = new();
        foreach (Hex h in hexesInRange)
        {
            if (Global.gameManager.game.mainGameBoard.gameHexDict[h].units.Count > 0)
            {
                foreach (int unitID in Global.gameManager.game.mainGameBoard.gameHexDict[h].units)
                {
                    if (IsEnemy(Global.gameManager.game.unitDictionary[unitID].teamNum))
                    {
                        targets.Add(h); //found an enemy unit in range
                        break; //no need to check other units in this hex
                    }
                }
            }
            else if (Global.gameManager.game.mainGameBoard.gameHexDict[h].district != null)
            {
                if (IsEnemy(Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[h].district.cityID].teamNum))
                {
                    if (Global.gameManager.game.mainGameBoard.gameHexDict[h].district.health > 0)
                    {
                        targets.Add(h); //found an enemy district in range
                    }
                }
            }
        }
        if (targets.Count > 0)
        {
            foreach (Hex hex in targets)
            {
                float lowCost = float.MaxValue;
                float cost = 0f;
                unit.PathFind(unit.hex, hex, Global.gameManager.game.teamManager, unit.movementCosts, unit.movementSpeed, out cost);
                if (cost < lowCost)
                {
                    target = hex; //find the closest district
                    return true;
                }
            }
        }
        target = new Hex(); //return an empty hex if no district found
        return false; //no enemy district found in range
    }
    private bool FindClosestEnemyDistrictInRange(AI ai, Unit unit, int range, out Hex target)
    {
        List<Hex> hexesInRange = unit.hex.WrappingRange(range, left, right, top, bottom);
        List<Hex> targets = new();
        foreach (Hex h in hexesInRange)
        {
            if (Global.gameManager.game.mainGameBoard.gameHexDict[h].district!=null)
            {
                if (IsEnemy(Global.gameManager.game.cityDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[h].district.cityID].teamNum))
                {
                    if (Global.gameManager.game.mainGameBoard.gameHexDict[h].district.health>0)
                    {
                        targets.Add(h); //found an enemy district in range
                    }
                }
            }
        }
        if (targets.Count > 0)
        {   
           foreach(Hex hex in targets)
           {
                float lowCost = float.MaxValue;
                float cost = 0f;
                unit.PathFind(unit.hex, hex, Global.gameManager.game.teamManager, unit.movementCosts, unit.movementSpeed, out cost);
                if (cost<lowCost)
                {
                    target = hex; //find the closest district
                    return true;
                }
           }
        }
        target = new Hex(); //return an empty hex if no district found
        return false; //no enemy district found in range
    }
    private bool IsEnemy(int teamNum)
    {
        return Global.gameManager.game.teamManager.GetEnemies(Global.gameManager.game.localPlayerTeamNum).Contains(teamNum);
    }
    private bool FindClosestValidSettleInRange(AI ai, Unit unit, int range, out Hex target)
    {
        List<Hex> hexesInRange = unit.hex.WrappingRange(range, left, right, top, bottom);
        List<Hex> targets = new();
        foreach (Hex h in hexesInRange)
        {
            if (unit.CanSettleHere(h, 3))
            {
                targets.Add(h); //found a valid settle hex in range
            }
        }
        if (targets.Count > 0)
        {
            foreach (Hex hex in targets)
            {
                float lowCost = float.MaxValue;
                float cost = 0f;
                unit.PathFind(unit.hex, hex, Global.gameManager.game.teamManager, unit.movementCosts, unit.movementSpeed, out cost);
                if (cost < lowCost)
                {
                    target = hex; //find the closest district
                    return true;
                }
            }
        }
        target = new Hex(); //return an empty hex if no district found
        return false; //no enemy district found in range
    }
    private void HandleScouts(AI ai, Unit unit)
    {
        List<Hex> validMoves = unit.MovementRange().Keys.ToList<Hex>();
        switch (ai.scoutStrategy)
        {
            case AIScoutStrategy.GameStart:
                break;
            case AIScoutStrategy.RANDOM:
                foreach (Hex hex in validMoves.ToList())
                {
                    if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].units.Count > 0)
                    {
                        validMoves.Remove(hex);
                    }
                }
                Hex target = validMoves[rng.Next(validMoves.Count)];
                Global.gameManager.MoveUnit(unit.id, target, false);
                break;
            default:
                break;
        }
    }
    private void HandleSettler(AI ai, Unit unit)
    {
        List<Hex> validMoves = unit.MovementRange().Keys.ToList<Hex>();
        Hex target;
        switch (ai.citySettlingStrategy)
        {
            case AICitySettlingStrategy.GameStart:
                break;
            case AICitySettlingStrategy.ClosestValidSettle:
                if (unit.CanSettleHere(unit.hex, 3))
                {
                    Global.gameManager.ActivateAbility(unit.id, "SettleCityAbility", unit.hex);
                }
                else if (FindClosestValidSettleInRange(ai, unit, 6, out target))
                {
                    Global.gameManager.MoveUnit(unit.id, target, false);
                }
                else
                {
                    //no valid settle found in range, just move randomly
                    target = validMoves[rng.Next(validMoves.Count)];
                    Global.gameManager.MoveUnit(unit.id, target, false);
                }
                break;
            case AICitySettlingStrategy.RANDOM:
                if (unit.CanSettleHere(unit.hex,3))
                {
                    Global.gameManager.ActivateAbility(unit.id, "SettleCityAbility", unit.hex);
                }
                else
                {
                    foreach (Hex hex in validMoves.ToList())
                    {
                        if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].units.Count > 0)
                        {
                            validMoves.Remove(hex);
                        }
                    }
                    target = validMoves[rng.Next(validMoves.Count)];
                    Global.gameManager.MoveUnit(unit.id, target, false);
                }
                break;
            default:
                break;
        }
    }
    private void HandleFounder(AI ai, Unit unit)
    {
        List<Hex> validMoves = unit.MovementRange().Keys.ToList<Hex>();
        Hex target;
        if (unit.CanSettleHere(unit.hex, 3))
        {
            Global.gameManager.ActivateAbility(unit.id, "SettleCityAbility", unit.hex);
        }
        else if (FindClosestValidSettleInRange(ai, unit, 6, out target))
        {
            Global.gameManager.MoveUnit(unit.id, target, false);
        }
        else
        {
            //no valid settle found in range, just move randomly
            target = validMoves[rng.Next(validMoves.Count)];
            Global.gameManager.MoveUnit(unit.id, target, false);
        }
    }
    private void HandleCityExpansion(AI ai, City city)
    {
        Global.Log(ai.cityExpansionStrategy.ToString() + " expansion strategy for city " + city.name + " (ID: " + city.id + ")");
        switch (ai.cityExpansionStrategy)
        {
            case AICityExpansionStrategy.GameStart:
                break;
            case AICityExpansionStrategy.RANDOM:
                Global.Log("Ai attempting random expand");
                List<Hex> validExpandHexes = city.ValidExpandHexes(new List<TerrainType> { TerrainType.Flat, TerrainType.Rough, TerrainType.Coast });
                List<Hex> validUrbanExpandHexes = city.ValidUrbanExpandHexes(new List<TerrainType> { TerrainType.Flat, TerrainType.Rough, TerrainType.Coast });
                List<Hex> allValidHexes = validExpandHexes.Concat(validUrbanExpandHexes).ToList();
                Hex target = allValidHexes[rng.Next(allValidHexes.Count)];
                Global.gameManager.ExpandToHex(city.id, target);
                break;
            default:
                break;
        }


    }
    private void HandleCityProduction(AI ai, City city)
    {
        switch (ai.overallProductionStrategy)
        {
            case AIOverallProductionStrategy.GameStart:
                if (GetNumberOfUnit(ai, "Warrior") < 1)
                {
                    Global.gameManager.AddToProductionQueue(city.id, "Warrior", city.hex);
                }
                else if (GetNumberOfUnit(ai, "Slinger") < 1)
                {
                    Global.gameManager.AddToProductionQueue(city.id, "Slinger", city.hex);
                }
                else if (GetNumberOfBuildingInCity(ai, city, "Granary") < 1)
                {
                    AttemptBuildingSpecificAtRandomValidHex("Granary", ai, city);
                }
                else if (GetNumberOfUnit(ai, "Settler") < 1)
                {
                    Global.gameManager.AddToProductionQueue(city.id, "Settler", city.hex);
                }
                else
                {
                    RandomCityProduction(ai, city);
                }
                break;
            case AIOverallProductionStrategy.RANDOM:
                RandomCityProduction(ai, city);
                break;
            default:
                break;
        }

    }
    private void RandomCityProduction(AI ai, City city)
    {
        if (city.ValidUnits().Count > 0 && rng.NextDouble() < 0.5)
        {
            ProduceUnit(ai, city);
        }
        else if (city.ValidBuildings().Count > 0 && GetDistrictsWithOpenSlots(ai, city).Count > 0)
        {
            ProduceBuilding(ai, city);
        }
        else if (city.ValidUnits().Count > 0)
        {
            ProduceUnit(ai, city);
        }
        else
        {
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
        switch (ai.buildingProductionStrategy)
        {
            case AIBuildingProductionStrategy.GameStart:
                break;
            case AIBuildingProductionStrategy.RANDOM:
                List<string> listOfValidBuildings = city.ValidBuildings();
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

        string buildingName = listOfValidBuildings[rng.Next(listOfValidBuildings.Count)];
        BuildingInfo buildingInfo = BuildingLoader.buildingsDict[buildingName];
        List<Hex> validHexes = city.ValidUrbanBuildHexes(buildingInfo.TerrainTypes, buildingInfo.DistrictType, 3, buildingName);

        if (validHexes.Count > 0)
        {
            Hex target = validHexes[rng.Next(validHexes.Count)];
            Global.gameManager.AddToProductionQueue(city.id, buildingName, target);
        }
        else
        {
            listOfValidBuildings.Remove(buildingName);
            AttemptAnyBuildingAtRandomValidHex(listOfValidBuildings, ai, city);
        }
    }
    private void AttemptBuildingSpecificAtRandomValidHex(string buildingName, AI ai, City city)
    {
        BuildingInfo buildingInfo = BuildingLoader.buildingsDict[buildingName];
        List<Hex> validHexes = city.ValidUrbanBuildHexes(buildingInfo.TerrainTypes, buildingInfo.DistrictType);

        if (validHexes.Count > 0)
        {
            Hex target = validHexes[rng.Next(validHexes.Count)];
            Global.gameManager.AddToProductionQueue(city.id, buildingName, target);
        }
        else
        {
            //no valid hexes found for this building
            return;
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


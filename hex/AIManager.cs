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

    private class AI
    {
        public Player player;
        public AIOverallStrategy overallStrategy = AIOverallStrategy.RANDOM;
        public AIUnitProductionStrategy unitProductionStrategy = AIUnitProductionStrategy.GameStart;
        public AIBuildingProductionStrategy buildingProductionStrategy = AIBuildingProductionStrategy.GameStart;
        public AICityExpansionStrategy cityExpansionStrategy = AICityExpansionStrategy.GameStart;
        public AICitySettlingStrategy citySettlingStrategy = AICitySettlingStrategy.GameStart;
        public AIMilitaryUnitStrategy militaryUnitStrategy = AIMilitaryUnitStrategy.GameStart;
        public AIScoutStrategy scoutStrategy = AIScoutStrategy.GameStart;
        public int turnCount;
        public int cityCount;
        public int unitCount;
    }

    enum AIOverallStrategy
    {
        Standard,
        Aggressive,
        Defensive,
        Economic,
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
        foreach (Player player in Global.gameManager.game.playerDictionary.Values)
        {
            if (player.isAI)
            {
                aiList.Add(new AI { player=player});
            }
        }
    }

    public override void _Process(double delta)
    {
        foreach (AI ai in aiList)
        {
            if (!ai.player.turnFinished)
            {
                PickStrategy(ai);
                HandleUnits(ai);
                HandleCities(ai);
                HandleResearchAndCulture(ai);
                EndAITurn(ai);
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
                HandleCityProduction(ai,city);
            }
            if (city.readyToExpand>0)
            {
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
        List<Hex> validMoves = unit.MovementRange().Keys.ToList<Hex>();
        bool isEnemyPresent = false;
        switch (ai.militaryUnitStrategy)
        {
            case AIMilitaryUnitStrategy.GameStart:
                break;
            case AIMilitaryUnitStrategy.RANDOM:
                Hex target = validMoves[rng.Next(validMoves.Count)];
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
                break;
            default:
                throw new NotImplementedException($"Military strategy {ai.militaryUnitStrategy} is not implemented.");
        }

    }

    private Hex FindClosestEnemyUnit(AI ai, Hex hex)
    {
        return hex;
    }

    private Hex FindClosestEnemyCity(AI ai, Hex hex)
    {
        return hex;
    }

    private void HandleScouts(AI ai, Unit unit)
    {
        List<Hex> validMoves = unit.MovementRange().Keys.ToList<Hex>();
        switch (ai.scoutStrategy)
        {
            case AIScoutStrategy.GameStart:
                break;
            case AIScoutStrategy.RANDOM:
                foreach (Hex hex in validMoves)
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
        switch (ai.citySettlingStrategy)
        {
            case AICitySettlingStrategy.GameStart:
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
                    Hex target = validMoves[rng.Next(validMoves.Count)];
                    Global.gameManager.MoveUnit(unit.id, target, false);
                }
                break;
            default:
                break;
        }
    }

    private void HandleFounder(AI ai, Unit unit)
    {
        Global.gameManager.ActivateAbility(unit.id, "SettleCapitalAbility", unit.hex);
    }

    private void HandleCityExpansion(AI ai, City city)
    {
        switch (ai.cityExpansionStrategy)
        {
            case AICityExpansionStrategy.GameStart:
                break;
            case AICityExpansionStrategy.RANDOM:
                List<Hex> validExpandHexes = city.ValidExpandHexes(new List<TerrainType> { TerrainType.Flat, TerrainType.Rough, TerrainType.Coast });
                List<Hex> validUrbanExpandHexes = city.ValidUrbanExpandHexes(new List<TerrainType> { TerrainType.Flat, TerrainType.Rough, TerrainType.Coast });
                Hex target;
                if (rng.NextDouble() < 0.5f)
                {
                    target = validExpandHexes[rng.Next(validExpandHexes.Count)];
                }
                else
                {
                    target = validExpandHexes[rng.Next(validUrbanExpandHexes.Count)];
                }
                Global.gameManager.ExpandToHex(city.id, target);
                break;
            default:
                break;
        }


    }

    private void HandleCityProduction(AI ai, City city)
    {

        if (city.ValidUnits().Count > 0 && rng.NextDouble() < 0.5)
        {
            ProduceUnit(ai, city);

        }
        else if (city.ValidBuildings().Count > 0 && GetDistrictsWithOpenSlots(ai,city).Count > 0)
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

    private void ProduceBuilding(AI ai, City city)
    {
        switch (ai.buildingProductionStrategy)
        {
            case AIBuildingProductionStrategy.GameStart:
                break;
            case AIBuildingProductionStrategy.RANDOM:
                List<string> listOfValidBuildings = city.ValidBuildings();
                AttemptBuilding(listOfValidBuildings, ai, city);
                break;
            default:
                break;
        }
    }

    private void AttemptBuilding(List<string> listOfValidBuildings, AI ai, City city)
    {
        
        if (listOfValidBuildings.Count == 0)
        {
            //no valid buildings left
            return;
        }

        string buildingName = listOfValidBuildings[rng.Next(listOfValidBuildings.Count)];
        BuildingInfo buildingInfo = BuildingLoader.buildingsDict[buildingName];
        List<Hex> validHexes = city.ValidUrbanBuildHexes(buildingInfo.TerrainTypes, buildingInfo.DistrictType);

        if (validHexes.Count > 0)
        {
            Hex target = validHexes[rng.Next(validHexes.Count)];
            Global.gameManager.AddToProductionQueue(city.id, buildingName, target);
        }
        else
        {
            listOfValidBuildings.Remove(buildingName);
            AttemptBuilding(listOfValidBuildings, ai, city);
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
        switch (ai.overallStrategy)
        {
            case AIOverallStrategy.RANDOM:
                ai.unitProductionStrategy = AIUnitProductionStrategy.RANDOM;
                ai.buildingProductionStrategy = AIBuildingProductionStrategy.RANDOM;
                ai.cityExpansionStrategy = AICityExpansionStrategy.RANDOM;
                ai.citySettlingStrategy = AICitySettlingStrategy.RANDOM;
                ai.militaryUnitStrategy = AIMilitaryUnitStrategy.RANDOM;
                ai.scoutStrategy = AIScoutStrategy.RANDOM;
                break;
        }
    }
}


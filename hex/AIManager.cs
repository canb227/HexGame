using Godot;
using NetworkMessages;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public partial class AIManager: Node
{

    List<AI> aiList = new List<AI>();
    Random rng = new Random();

    private class AI
    {
        public Player player;
        public AIStrategy strategy;
        public int turnCount;
        public int cityCount;
        public int unitCount;
    }

    enum AIStrategy
    {
        GameStart,
        MilitaryAggression,
        GrowTall,
        Expand,
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
            PickStrategy(ai);
            HandleUnits(ai);
            HandleCities(ai);
            EndAITurn(ai);
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
        foreach (int unitID in ai.player.unitList)
        {
            Unit unit = Global.gameManager.game.unitDictionary[unitID];
            if (unit.name.Equals( "Founder"))
            {
                HandleFounder(ai, unit);
            }
            else if (unit.name.Equals("Settler"))
            {
                HandleSettler(ai, unit);
            }
            else if (unit.unitClass == (UnitClass.Recon))
            {
                HandleScouts(ai, unit);
            }
            else if (unit.unitClass == (UnitClass.Combat))
            {
                HandleMilitary(ai,unit);
            }

        }
    }

    private void HandleMilitary(AI ai, Unit unit)
    {
        throw new NotImplementedException();
    }

    private void HandleScouts(AI ai, Unit unit)
    {
        throw new NotImplementedException();
    }

    private void HandleSettler(AI ai, Unit unit)
    {
        throw new NotImplementedException();
    }

    private void HandleFounder(AI ai, Unit unit)
    {
        Global.gameManager.ActivateAbility(unit.id, "SettleCapitalAbility", unit.hex);
    }

    private void HandleCityExpansion(AI ai, City city)
    {
        switch (ai.strategy)
        {
            case AIStrategy.GameStart:
                
                break;
            case AIStrategy.MilitaryAggression:
                
                break;
            case AIStrategy.GrowTall:
                
                break;
            case AIStrategy.Expand:
                
                break;
            case AIStrategy.RANDOM:
                List<Hex> validExpandHexes = city.ValidExpandHexes(new List<TerrainType> { TerrainType.Flat, TerrainType.Rough, TerrainType.Coast });
                Hex target = validExpandHexes[rng.Next(validExpandHexes.Count)];
                Global.gameManager.ExpandToHex(city.id, target);
                break;
        }

    }

    private void HandleCityProduction(AI ai, City city)
    {
        List<UnitInfo> listOfValidUnits = city.ValidUnits();
        List<BuildingInfo> listOfValidBuildings = city.ValidBuildings();


        if (listOfValidUnits.Count>0 && rng.NextDouble() < 0.5)
        {
            ProduceUnit(ai, city);

        }
        else if (listOfValidBuildings.Count > 0)
        {
            ProduceBuilding(ai, city);
        }
        else
        {
            return;
        }
    }

    private void ProduceBuilding(AI ai, City city)
    {
        throw new NotImplementedException();
    }

    private void ProduceUnit(AI ai, City city)
    {
        List<UnitInfo> listOfValidUnits = city.ValidUnits();
        UnitInfo unit = listOfValidUnits[rng.Next(listOfValidUnits.Count)];
        //Global.gameManager.AddToProductionQueue(city.id, unit, city.hex);

    }

    private List<string> GetListOfValidUnitNames(Player player, City city)
    {
        return new List<string> { "settler" };
    }



    private void PickStrategy(AI ai)
    {
        ai.strategy = AIStrategy.RANDOM;
    }
}


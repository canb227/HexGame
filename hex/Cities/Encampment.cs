using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum FactionOwnership
{
    Free,
    Occupied,
    Vassalized
}
public partial class Encampment : City
{
    public FactionOwnership ownershipState = FactionOwnership.Free;
    public int overlordTeamNum = -1;

    public Encampment(int id, int teamNum, String name, bool isCapital, GameHex gameHex)
    {
        cityRange = 2;



        this.id = id;
        originalCapitalTeamID = id;
        this.teamNum = teamNum;
        Global.gameManager.game.cityDictionary.Add(this.id, this);
        Global.gameManager.game.playerDictionary[this.teamNum].cityList.Add(this.id);

        this.name = name;
        this.hex = gameHex.hex;
        productionQueue = new();
        partialProductionDictionary = new();
        heldResources = new();
        heldHexes = new();
        districts = new();
        naturalPopulation = 1;
        readyToExpand = 0;
        maxDistrictSize = 2;
        baseMaxResourcesHeld = 3;
        foodToGrow = GetFoodToGrowCost();
        yields = new();
        myYields = new();
        SetBaseHexYields();
        UpdateNearbyHexes();

        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
        {
            manager.CallDeferred("NewCity", id);
        }
        AddEncampmentCenter();
        this.isCapital = isCapital;
        this.wasCapital = isCapital;
        RecalculateYields();
    }

    public new void DistrictFell()
    {
        bool allDistrictsFell = true;
        bool cityCenterOccupied = false;
        Unit unit = null;
        foreach (District district in districts)
        {
            if (district.health > 0.0f)
            {
                allDistrictsFell = false;
            }
            if (district.isCityCenter && district.health <= 0.0f)
            {
                if (Global.gameManager.game.mainGameBoard.gameHexDict[district.hex].units.Any())
                {
                    unit = Global.gameManager.game.unitDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[district.hex].units[0]];
                    if (Global.gameManager.game.teamManager.GetEnemies(teamNum).Contains(unit.teamNum))
                    {
                        cityCenterOccupied = true;
                    }
                }
            }
        }
        if (allDistrictsFell && cityCenterOccupied)
        {
            Global.gameManager.graphicManager.uiManager.EncampmentTakenPopUp(this, unit.teamNum);
        }
    }

    public void AddEncampmentCenter()
    {
        District district = new District(Global.gameManager.game.mainGameBoard.gameHexDict[hex], FactionLoader.GetFactionCapitalBuilding(Global.gameManager.game.playerDictionary[teamNum].faction), true, true, id, true);
        districts.Add(district);
    }

    public new void OnTurnStarted(int turnNumber)
    {
        base.OnTurnStartedBody();
    }

    public new void RecalculateYields()
    {
        base.RecalculateYields();
        if (ownershipState == FactionOwnership.Occupied)
        {
            myYields.production = myYields.production * 0.75f;
            yields.production = yields.production * 0.25f;
        }
    }

    public void EncampmentOccupied(int overlord)
    {
        foreach (int cityID in Global.gameManager.game.playerDictionary[overlordTeamNum].cityList)
        {
            Global.gameManager.game.playerDictionary[overlordTeamNum].NewExportRoute(id, cityID, YieldType.production);
        }
    }

    public void EncampmentFreed()
    {
        if(overlordTeamNum != -1)
        {
            overlordTeamNum = -1;
            ownershipState = FactionOwnership.Free;
            foreach (int cityID in Global.gameManager.game.playerDictionary[overlordTeamNum].cityList)
            {
                Global.gameManager.game.playerDictionary[overlordTeamNum].RemoveExportRoute(id, cityID, YieldType.production);
            }
        }
    }

    public new void ExpandToHex(Hex hex)
    {
        if (readyToExpand > 0)
        {
            District district = new District(Global.gameManager.game.mainGameBoard.gameHexDict[hex], false, false, id, true); //create district as encampment meaning we only claim the main hex
            districts.Add(district);
            readyToExpand -= 1;
            RecalculateYields();
            if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
            {
                var data = new Godot.Collections.Dictionary
                {
                    { "q", hex.q },
                    { "r", hex.r },
                    { "s", hex.s }
                };
                manager.CallDeferred("UpdateHex", data); manager.CallDeferred("UpdateGraphic", id, (int)GraphicUpdateType.Update);
                manager.CallDeferred("ClearWaitForTarget");
            }
        }
        else
        {
            GD.PushWarning("tried to expand without readyToExpand > 0");
        }
    }

    public new bool ValidExpandHex(List<TerrainType> validTerrain, GameHex targetGameHex)
    {
        if (validTerrain.Count == 0 || validTerrain.Contains(targetGameHex.terrainType))
        {
            //hex is owned by us not owned
            if (targetGameHex.ownedBy == -1 | targetGameHex.ownedBy == teamNum)
            {
                //hex does not have a district
                if (targetGameHex.district == null)
                {
                    //hex doesnt have a enemy unit
                    bool noEnemyUnit = true;
                    foreach (int unitID in targetGameHex.units)
                    {
                        Unit unit = Global.gameManager.game.unitDictionary[unitID];
                        if (Global.gameManager.game.teamManager.GetEnemies(teamNum).Contains(unit.teamNum))
                        {
                            noEnemyUnit = false;
                            break;
                        }
                    }
                    bool adjacentDistrict = false;
                    foreach (Hex hex in targetGameHex.hex.WrappingNeighbors(Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.bottom))
                    {
                        if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].district != null && Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.cityID == id)
                        {
                            adjacentDistrict = true;
                            break;
                        }
                    }
                    if (noEnemyUnit && adjacentDistrict)
                    {
                        return true;

                    }
                }
            }
        }
        return false;
    }
    //valid hexes for a rural district
    public new List<Hex> ValidExpandHexes(List<TerrainType> validTerrain)
    {
        List<Hex> validHexes = new();
        //gather valid targets
        foreach (Hex hex in hex.WrappingRange(cityRange, Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.top, Global.gameManager.game.mainGameBoard.bottom))
        {
            if (ValidExpandHex(validTerrain, Global.gameManager.game.mainGameBoard.gameHexDict[hex]))
            {
                validHexes.Add(hex);
            }
        }
        return validHexes;
    }
}

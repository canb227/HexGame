using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public enum NeutralType
{
    Goblins
}
public partial class Encampment : City
{
    public bool isCaptured = false;
    public NeutralType neutralType;
    public Encampment(int id, int teamNum, String name, bool isCapital, NeutralType neutralType, GameHex gameHex)
    {
        cityRange = 2;

        this.neutralType = neutralType;
        Global.gameManager.game.cityDictionary.Add(id, this);
        this.id = id;
        originalCapitalTeamID = id;
        this.teamNum = teamNum;
        this.name = name;
        this.hex = gameHex.hex;
        productionQueue = new();
        partialProductionDictionary = new();
        heldResources = new();
        heldHexes = new();
        Global.gameManager.game.playerDictionary[teamNum].cityList.Add(this.id);
        districts = new();
        naturalPopulation = 2;
        readyToExpand = 1;
        maxDistrictSize = 2;
        baseMaxResourcesHeld = 3;
        foodToGrow = GetFoodToGrowCost();
        yields = new();
        myYields = new();
        SetBaseHexYields();
        UpdateNearbyHexes();

        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
        {
            manager.CallDeferred("NewCity", this);
        }
        AddEncampmentCenter();
        this.isCapital = isCapital;
        this.wasCapital = isCapital;
        RecalculateYields();
    }

    public void AddEncampmentCenter()
    {
        District district;
        if (neutralType == NeutralType.Goblins)
        {
            district = new District(Global.gameManager.game.mainGameBoard.gameHexDict[hex], "GoblinDen", true, true, id);
        }
        else
        {
            district = new District(Global.gameManager.game.mainGameBoard.gameHexDict[hex], "GoblinDen", true, true, id);
        }
        districts.Add(district);
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
            if (targetGameHex.ownedBy == -1)
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

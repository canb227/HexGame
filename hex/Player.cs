using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data;
using Godot;
using System.IO;
using NetworkMessages;

[Serializable]
public class Player
{
    public Player(float goldTotal, int teamNum, Godot.Color teamColor)
    {
        this.teamColor = teamColor;
        this.teamNum = teamNum;
        this.goldTotal = goldTotal;
        Global.gameManager.game.teamManager.AddTeam(teamNum, 50);
        OnResearchComplete("Agriculture");
        OnCultureResearchComplete("Tribal Dominion");
/*        SelectResearch("FutureTech");
        SelectCultureResearch("FutureTech");*/
        if(teamNum == 0)
        {
            foreach(Hex hex in Global.gameManager.game.mainGameBoard.gameHexDict.Keys)
            {
                visibleGameHexDict.Add(hex, 99);
                seenGameHexDict.Add(hex, true);
            }
        }
        
    }

    public Player()
    {
        //used for loading
    }
    public int teamNum { get; set; }
    public bool turnFinished { get; set; }
    public Dictionary<Hex, int> visibleGameHexDict { get; set; } = new();
    public Dictionary<Hex, bool> seenGameHexDict { get; set; } = new();
    public List<Hex> visibilityChangedList { get; set; } = new();
    public List<ResearchQueueType> queuedResearch { get; set; } = new();
    public Dictionary<String, ResearchQueueType> partialResearchDictionary { get; set; } = new();
    public HashSet<String> completedResearches { get; set; } = new();
    public List<ResearchQueueType> queuedCultureResearch { get; set; } = new();
    public Dictionary<String, ResearchQueueType> partialCultureResearchDictionary { get; set; } = new();
    public HashSet<String> completedCultureResearches { get; set; } = new();
    public List<int> unitList { get; set; } = new();
    public List<int> cityList { get; set; } = new();
    public List<(UnitEffect, UnitClass)> unitResearchEffects { get; set; } = new();
    public List<(BuildingEffect, String)> buildingResearchEffects { get; set; } = new();
    public HashSet<String> allowedBuildings { get; set; } = new();
    public HashSet<DistrictType> allowedDistricts { get; set; } = new();
    public HashSet<String> allowedUnits { get; set; } = new();
    public Dictionary<Hex, ResourceType> unassignedResources { get; set; } = new();
    public Dictionary<Hex, ResourceType> globalResources {  get; set; } = new();
    public float strongestUnitBuilt { get; set; } = 0.0f;

    public float goldTotal { get; set; }
    public float scienceTotal { get; set; }
    public float cultureTotal { get; set; }
    public float happinessTotal { get; set; }
    public float influenceTotal { get; set; }

    private int idCounter = 0;

    public Godot.Color teamColor;

    public void SetGoldTotal(float goldTotal)
    {
        this.goldTotal = goldTotal;
        if(teamNum == Global.gameManager.game.localPlayerTeamNum)
        {
            if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager)) manager.Update2DUI(UIElement.gold);
        }
    }

    public float GetGoldTotal()
    {
        return goldTotal;
    }

    public void SetScienceTotal(float scienceTotal)
    {
        this.scienceTotal = scienceTotal;
        if (teamNum == Global.gameManager.game.localPlayerTeamNum)
        {
            if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager)) manager.Update2DUI(UIElement.science);
        }
    }

    public float GetScienceTotal()
    {
        return scienceTotal;
    }

    public void SetCultureTotal(float cultureTotal)
    {
        this.cultureTotal = cultureTotal;
        if (teamNum == Global.gameManager.game.localPlayerTeamNum)
        {
            if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager)) manager.Update2DUI(UIElement.culture);
        }
    }

    public float GetCultureTotal()
    {
        return cultureTotal;
    }

    public void SetHappinessTotal(float happinessTotal)
    {
        this.happinessTotal = happinessTotal;
        if (teamNum == Global.gameManager.game.localPlayerTeamNum)
        {
            if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager)) manager.Update2DUI(UIElement.happiness);
        }
    }

    public float GetHappinessTotal()
    {
        return happinessTotal;
    }

    public void SetInfluenceTotal(float influenceTotal)
    {
        this.influenceTotal = influenceTotal;
        if (teamNum == Global.gameManager.game.localPlayerTeamNum)
        {
            if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager)) manager.Update2DUI(UIElement.influence);
        }
    }

    public float GetInfluenceTotal()
    {
        return influenceTotal;
    }

    public float GetGoldPerTurn()
    {
        float goldPerTurn = 0.0f;
        foreach (int cityID in cityList)
        {
            City city = Global.gameManager.game.cityDictionary[cityID];
            goldPerTurn += city.yields.gold;
        }
        return goldPerTurn;
    }

    public float GetSciencePerTurn()
    {
        float sciencePerTurn = 0.0f;
        foreach(int cityID in cityList)
        {
            City city = Global.gameManager.game.cityDictionary[cityID];
            sciencePerTurn += city.yields.science;
        }
        return sciencePerTurn;
    }

    public float GetCulturePerTurn()
    {
        float culturePerTurn = 0.0f;
        foreach (int cityID in cityList)
        {
            City city = Global.gameManager.game.cityDictionary[cityID];
            culturePerTurn += city.yields.culture;
        }
        return culturePerTurn;
    }

    public float GetHappinessPerTurn()
    {
        float happinessPerTurn = 0.0f;
        foreach (int cityID in cityList)
        {
            City city = Global.gameManager.game.cityDictionary[cityID];
            happinessPerTurn += city.yields.happiness;
        }
        return happinessPerTurn;
    }

    public float GetInfluencePerTurn()
    {
        float influencePerTurn = 0.0f;
        foreach (int cityID in cityList)
        {
            City city = Global.gameManager.game.cityDictionary[cityID];
            influencePerTurn += city.yields.influence;
        }
        return influencePerTurn;
    }

    public void OnTurnStarted(int turnNumber)
    {
        turnFinished = false;
        foreach (int unitID in unitList)
        {
            Unit unit = Global.gameManager.game.unitDictionary[unitID];
            unit.OnTurnStarted(turnNumber);
        }
        foreach (int cityID in cityList)
        {
            City city = Global.gameManager.game.cityDictionary[cityID];
            city.OnTurnStarted(turnNumber);
        }
        if(queuedResearch.Any())
        {
            float cost = queuedResearch[0].researchLeft;
            queuedResearch[0].researchLeft -= (int)Math.Round(scienceTotal);
            scienceTotal -= cost;
            scienceTotal = Math.Max(0.0f, scienceTotal);
            if(queuedResearch[0].researchLeft <= 0)
            {
                OnResearchComplete(queuedResearch[0].researchType);
                queuedResearch.RemoveAt(0);
            }
        }
        if (queuedCultureResearch.Any())
        {
            float cost = queuedCultureResearch[0].researchLeft;
            queuedCultureResearch[0].researchLeft -= (int)Math.Round(cultureTotal);
            cultureTotal -= cost;
            cultureTotal = Math.Max(0.0f, cultureTotal);
            if (queuedCultureResearch[0].researchLeft <= 0)
            {
                OnCultureResearchComplete(queuedCultureResearch[0].researchType);
                queuedCultureResearch.RemoveAt(0);
            }
        }
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
        {
            manager.Update2DUI(UIElement.gold);
            manager.Update2DUI(UIElement.happiness);
            manager.Update2DUI(UIElement.influence);
            manager.Update2DUI(UIElement.researchTree);
            manager.uiManager.UpdateResearchUI();
        }
    }

    public void OnTurnEnded(int turnNumber)
    {
        foreach (int unitID in unitList)
        {
            Unit unit = Global.gameManager.game.unitDictionary[unitID];
            unit.OnTurnEnded(turnNumber);
        }
        foreach (int cityID in cityList)
        {
            City city = Global.gameManager.game.cityDictionary[cityID];
            city.OnTurnEnded(turnNumber);
        }
        turnFinished = true;
    }

    public void UpdateTerritoryGraphic()
    {
        GD.Print("UpdateGraphic");
        //remove old lines
        GraphicGameBoard ggb = ((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);
        foreach (HexChunk hexChunk in ggb.chunkList)
        {
            foreach (Node child in hexChunk.multiMeshInstance.GetChildren())
            {
                if (child.Name.ToString().Contains("TerritoryLines"))
                {
                    child.Free();
                }
            }
        }
        //calculate where to draw lines
        foreach (int cityID in cityList)
        {
            City city = Global.gameManager.game.cityDictionary[(int)cityID];
            foreach (Hex hex in city.heldHexes)
            {
                Node3D territoryLinesNode = (Node3D)Global.gameManager.graphicManager.territoryLinesScene.GetChild(0).Duplicate();

                int newHexQ = (Global.gameManager.game.mainGameBoard.left + (hex.r >> 1) + hex.q) % ggb.chunkSize - (hex.r >> 1);
                Hex modHex = new Hex(newHexQ, hex.r, -newHexQ - hex.r);
                Point hexPoint = Global.gameManager.graphicManager.layout.HexToPixel(modHex);
                float height = 2.0f;//ggb.chunkList[ggb.hexToChunkDictionary[hex]].Vector3ToHeightMapVal(territoryLinesNode.Transform.Origin); //TODO
                Transform3D newTransform = territoryLinesNode.Transform;
                newTransform.Origin = new Vector3((float)hexPoint.y, height, (float)hexPoint.x);
                territoryLinesNode.Transform = newTransform;

                int index = 0;
                foreach (MeshInstance3D mesh in territoryLinesNode.GetChildren())
                {
                    if (index == 0)
                    {
                        int newQ = hex.q + 1;
                        int newR = hex.r - 1;
                        Hex adjacentHex = (new Hex(newQ, newR, -newQ - newR)).WrapHex();
                        if (Global.gameManager.game.mainGameBoard.gameHexDict[adjacentHex].ownedBy == city.teamNum)
                        {
                            mesh.QueueFree();
                        }
                    }
                    if (index == 1)
                    {
                        int newQ = hex.q + 1;
                        int newR = hex.r;
                        Hex adjacentHex = (new Hex(newQ, newR, -newQ - newR)).WrapHex();
                        if (Global.gameManager.game.mainGameBoard.gameHexDict[adjacentHex].ownedBy == city.teamNum)
                        {
                            mesh.QueueFree();
                        }
                    }
                    if (index == 2)
                    {
                        int newQ = hex.q;
                        int newR = hex.r + 1;
                        Hex adjacentHex = (new Hex(newQ, newR, -newQ - newR)).WrapHex();
                        if (Global.gameManager.game.mainGameBoard.gameHexDict[adjacentHex].ownedBy == city.teamNum)
                        {
                            mesh.QueueFree();
                        }
                    }
                    if (index == 3)
                    {
                        int newQ = hex.q - 1;
                        int newR = hex.r + 1;
                        Hex adjacentHex = (new Hex(newQ, newR, -newQ - newR)).WrapHex();
                        if (Global.gameManager.game.mainGameBoard.gameHexDict[adjacentHex].ownedBy == city.teamNum)
                        {
                            mesh.QueueFree();
                        }
                    }
                    if (index == 4)
                    {
                        int newQ = hex.q - 1;
                        int newR = hex.r;
                        Hex adjacentHex = (new Hex(newQ, newR, -newQ - newR)).WrapHex();
                        if (Global.gameManager.game.mainGameBoard.gameHexDict[adjacentHex].ownedBy == city.teamNum)
                        {
                            mesh.QueueFree();
                        }
                    }
                    if (index == 5)
                    {
                        int newQ = hex.q;
                        int newR = hex.r - 1;
                        Hex adjacentHex = (new Hex(newQ, newR, -newQ - newR)).WrapHex();
                        if (Global.gameManager.game.mainGameBoard.gameHexDict[adjacentHex].ownedBy == city.teamNum)
                        {
                            mesh.QueueFree();
                        }
                    }

                    StandardMaterial3D material = new StandardMaterial3D();
                    //material.AlbedoColor = Global.menuManager.lobby.PlayerColors[teamNum];

                    Gradient gradient = new Gradient();
                    //gradient.AddPoint(0.0f, new Color(1, 0, 0, 1));
                    //gradient.AddPoint(1.0f, new Color(0, 1, 0, 1)); //.menuManager.lobby.PlayerColors[teamNum]

                    // Clear any existing points if needed
                    /*                    while (gradient.GetPointCount() > 0)
                                            gradient.RemovePoint(0);*/

                    GD.Print(gradient.GetPointCount());
                    // Add white at the start (offset 0.0)
                    //gradient.AddPoint(0.0f, new Color(1, 1, 1)); // White

                    // Add red at the end (offset 1.0)
                    //gradient.AddPoint(1.0f, new Color(1, 0, 0)); // Red

                    gradient.SetColor(0, new Color(teamColor.R, teamColor.G, teamColor.B, 0.2f));
                    gradient.SetColor(1, teamColor);

                    GradientTexture1D gradTex = new GradientTexture1D();
                    gradTex.Gradient = gradient;
                    gradTex.Width = 256;
                    material.AlbedoTexture = gradTex;
                    material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
                    mesh.SetSurfaceOverrideMaterial(0, material);

                    index++;
                }
                territoryLinesNode.Name = "TerritoryLines" + hex;
                ggb.chunkList[ggb.hexToChunkDictionary[hex]].multiMeshInstance.AddChild(territoryLinesNode);
            }
        }
    }

    public void SelectResearch(String researchType)
    {
        HashSet<String> visited = new();
        List<ResearchQueueType> queue = new();
        if(queuedResearch.Any())
        {
            partialResearchDictionary[queuedResearch[0].researchType] = queuedResearch[0];
        }
        queuedResearch.Clear();
        void TopologicalSort(String researchType)
        {
            if (visited.Contains(researchType) || completedResearches.Contains(researchType))
                return; 


            visited.Add(researchType);

            if (ResearchLoader.researchesDict.ContainsKey(researchType))
            {
                foreach (String requirement in ResearchLoader.researchesDict[researchType].Requirements)
                {
                    TopologicalSort(requirement);
                }
            }
            if(partialResearchDictionary.ContainsKey(researchType))
            {
                queuedResearch.Add(partialResearchDictionary[researchType]);
            }
            else
            {
                queuedResearch.Add(new ResearchQueueType(researchType, ResearchLoader.tierCostDict[ResearchLoader.researchesDict[researchType].Tier], ResearchLoader.tierCostDict[ResearchLoader.researchesDict[researchType].Tier])); //apply cost mod TODO
            }
        }

        TopologicalSort(researchType);
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager2))
        {
            manager2.Update2DUI(UIElement.endTurnButton);
            manager2.Update2DUI(UIElement.researchTree);
        }
    }

    public void OnResearchComplete(String researchType)
    {
        completedResearches.Add(researchType);
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
        {
            manager.Update2DUI(UIElement.researchTree);
        }
        foreach (String unitType in ResearchLoader.researchesDict[researchType].UnitUnlocks)
        {
            allowedUnits.Add(unitType);
        }
        foreach(String buildingType in ResearchLoader.researchesDict[researchType].BuildingUnlocks)
        {
            allowedBuildings.Add(buildingType);
            allowedDistricts.Add(BuildingLoader.buildingsDict[buildingType].DistrictType);
        }
        foreach(String effect in ResearchLoader.researchesDict[researchType].Effects)
        {
            ResearchLoader.ProcessFunctionString(effect, this);
        }
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager2)) manager2.Update2DUI(UIElement.endTurnButton);
    }

    public void SelectCultureResearch(String researchType)
    {
        HashSet<String> visited = new();
        List<ResearchQueueType> queue = new();
        if (queuedCultureResearch.Any())
        {
            partialCultureResearchDictionary[queuedCultureResearch[0].researchType] = queuedCultureResearch[0];
        }
        queuedCultureResearch.Clear();
        void TopologicalSort(String researchType)
        {
            if (visited.Contains(researchType) || completedCultureResearches.Contains(researchType))
                return;


            visited.Add(researchType);

            if (CultureResearchLoader.researchesDict.ContainsKey(researchType))
            {
                foreach (String requirement in CultureResearchLoader.researchesDict[researchType].Requirements)
                {
                    TopologicalSort(requirement);
                }
            }
            if (partialCultureResearchDictionary.ContainsKey(researchType))
            {
                queuedCultureResearch.Add(partialCultureResearchDictionary[researchType]);
            }
            else
            {
                queuedCultureResearch.Add(new ResearchQueueType(researchType, CultureResearchLoader.tierCostDict[CultureResearchLoader.researchesDict[researchType].Tier], CultureResearchLoader.tierCostDict[CultureResearchLoader.researchesDict[researchType].Tier])); //apply cost mod TODO
            }
        }

        TopologicalSort(researchType);
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager2))
        {
            manager2.Update2DUI(UIElement.endTurnButton);
            manager2.Update2DUI(UIElement.researchTree);
        }
    }

    public void OnCultureResearchComplete(String researchType)
    {
        completedCultureResearches.Add(researchType);
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
        {
            manager.Update2DUI(UIElement.researchTree);
        }
        foreach (String unitType in CultureResearchLoader.researchesDict[researchType].UnitUnlocks)
        {
            allowedUnits.Add(unitType);
        }
        foreach (String buildingType in CultureResearchLoader.researchesDict[researchType].BuildingUnlocks)
        {
            allowedBuildings.Add(buildingType);
            allowedDistricts.Add(BuildingLoader.buildingsDict[buildingType].DistrictType);
        }
        foreach (String effect in CultureResearchLoader.researchesDict[researchType].Effects)
        {
            CultureResearchLoader.ProcessFunctionString(effect, this);
        }
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager2)) manager2.Update2DUI(UIElement.endTurnButton);
    }

    public bool AddResource(Hex hex, ResourceType resourceType, City targetCity)
    {
        if(targetCity.heldResources.Count < targetCity.maxResourcesHeld)
        {
            targetCity.heldResources.Add(hex, resourceType);
            targetCity.RecalculateYields();
            unassignedResources.Remove(hex);
            return true;
        }
        return false;
    }



    public bool RemoveResource(Hex hex)
    {
        foreach (int cityID in cityList)
        {
            City city = Global.gameManager.game.cityDictionary[cityID];
            if (city.heldResources.Keys.Contains(hex))
            {
                ResourceType temp = city.heldResources[hex];
                city.heldResources.Remove(hex);
                city.RecalculateYields();
                unassignedResources.Add(hex, temp);
                return true;
            }
        }
        return false;
    }
    
    
    public bool RemoveLostResource(Hex hex)
    {
        foreach (int cityID in cityList)
        {
            City city = Global.gameManager.game.cityDictionary[cityID];
            if (city.heldResources.Remove(hex))
            {
                return true;
            }
        }
        globalResources.Remove(hex);
        unassignedResources.Remove(hex);
        return false;
    }

    public void AddGold(float gold)
    {
        SetGoldTotal(GetGoldTotal() + gold);
    }
    public void AddScience(float science)
    {
        SetScienceTotal(GetScienceTotal() + science);
    }
    public void AddCulture(float culture)
    {
        SetCultureTotal(GetCultureTotal() + culture);
    }
    public void AddHappiness(float happiness)
    {
        SetHappinessTotal(GetHappinessTotal() + happiness);
    }
    public void AddInfluence(float influence)
    {
        SetInfluenceTotal(GetInfluenceTotal() + influence);
    }

    internal int GetNextUniqueID()
    {
        string teamID = ""; //teamnum with minimum three digits (1 = 100, 3 = 300, 74 = 740, 600 = 600, 999 = 999)
        if (teamNum>999)
        {
           throw new Exception("Team number exceeds 999, cannot generate unique ID.");
        }

        if (teamNum <0)
        {             
            throw new Exception("Team number cannot be negative, cannot generate unique ID.");
        }

        if (idCounter > 999999)
        {
            throw new Exception("IDCounter exceeds 999999, cannot generate unique ID.");
        }

        if (teamNum < 10)
        {
            teamID = "00" + teamNum.ToString();
        }
        else if (teamNum < 100)
        {
            teamID = "0" + teamNum.ToString();
        }
        else
        {
            teamID = teamNum.ToString();
        }
        string idString = idCounter.ToString() + teamID;
        idCounter++;
        return int.Parse(idString);
    }
}

public class ResearchQueueType
{

    public ResearchQueueType(String researchType, int researchCost, int researchLeft)
    {
        this.researchType = researchType;
        this.researchCost = researchCost;
        this.researchLeft = researchLeft;
    }
    public String researchType { get; set; }
    public int researchCost { get; set; }
    public int researchLeft { get; set; }

}

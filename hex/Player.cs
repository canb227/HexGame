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
    public Player(float goldTotal, int teamNum, Godot.Color teamColor, bool isAI)
    {
        this.teamColor = teamColor;
        this.teamNum = teamNum;
        this.goldTotal = goldTotal;
        this.isAI = isAI;
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

        theme = new Theme();

        playerTerritoryMaterial = new StandardMaterial3D();

        Gradient gradient = new Gradient();
        gradient.SetColor(0, new Color(teamColor.R, teamColor.G, teamColor.B, 0.2f));
        gradient.SetColor(1, teamColor);

        GradientTexture1D gradTex = new GradientTexture1D();
        gradTex.Gradient = gradient;
        gradTex.Width = 256;

        playerTerritoryMaterial.AlbedoTexture = gradTex;
        playerTerritoryMaterial.Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;

        //default diplomatic actions
        diplomaticActionHashSet.Add(new DiplomacyAction(teamNum, "Give Gold", true, false));
        diplomaticActionHashSet.Add(new DiplomacyAction(teamNum, "Give Gold Per Turn", true, true));
        diplomaticActionHashSet.Add(new DiplomacyAction(teamNum, "Make Peace", false, false));
    }

    public Player()
    {
        //used for loading
    }
    public bool isAI { get; set; } = false;
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
    public HashSet<DiplomacyAction> diplomaticActionHashSet { get; set; } = new();

    public GovernmentType government { get; set; } = new();
    public float strongestUnitBuilt { get; set; } = 0.0f;

    public float goldTotal { get; set; }
    public float scienceTotal { get; set; }
    public float cultureTotal { get; set; }
    public float happinessTotal { get; set; }
    public float influenceTotal { get; set; }
    public int settlerCount = 0;
    //exports and trade
    public List<ExportRoute> exportRouteList { get; set; } = new();
    public List<TradeRoute> tradeRouteList { get; set; } = new();
    public List<TradeRoute> outgoingTradeRouteList { get; set; } = new();
    public int exportCount { get; set; }
    public int exportCap { get; set; } = 2;
    public int maxTradeCount { get; set; } = 2;
    public int tradeRouteCount { get; set; }
    private int idCounter { get; set; } = 0;

    public float foodDifficultyModifier = 1.0f;
    public float productionDifficultyModifier = 1.0f;
    public float goldDifficultyModifier = 1.0f;
    public float scienceDifficultyModifier = 1.0f;
    public float cultureDifficultyModifier = 1.0f;
    public float happinessDifficultyModifier = 1.0f;
    public float influenceDifficultyModifier = 1.0f;
    public float combatPowerDifficultyModifier = 0.0f;

    public Godot.Color teamColor;
    public StandardMaterial3D playerTerritoryMaterial;
    public Theme theme;

    public void SetGoldTotal(float goldTotal)
    {
        this.goldTotal = goldTotal;
        if(teamNum == Global.gameManager.game.localPlayerTeamNum)
        {
            if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager)) manager.CallDeferred("Update2DUI", (int)UIElement.gold);
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
            if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager)) manager.CallDeferred("Update2DUI", (int)UIElement.science);
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
            if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager)) manager.CallDeferred("Update2DUI", (int)UIElement.culture);
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
            if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager)) manager.CallDeferred("Update2DUI", (int)UIElement.happiness);
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
            if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager)) manager.CallDeferred("Update2DUI", (int)UIElement.influence);
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
        if(exportCount > exportCap)
        {
            foreach (int cityID in cityList)
            {
                foreach(ExportRoute route in exportRouteList.AsEnumerable().Reverse().ToList())
                {
                    if (route.sourceCityID == cityID)
                    {
                        RemoveExportRoute(route.sourceCityID, route.targetCityID, route.exportType);
                        break;
                    }
                }
            }
        }
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
        {
            manager.CallDeferred("Update2DUI", (int)UIElement.gold);
            manager.CallDeferred("Update2DUI", (int)UIElement.happiness);
            manager.CallDeferred("Update2DUI", (int)UIElement.influence);
            manager.CallDeferred("Update2DUI", (int)UIElement.researchTree);
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

    public List<string> AvaliableResearches()
    {
        List<string> researchNames = new();
        foreach(string research in ResearchLoader.researchesDict.Keys)
        {
            bool canResearch = true;
            ResearchInfo info = ResearchLoader.researchesDict[research];
            foreach (string requirement in info.Requirements)
            {
                if (!completedResearches.Contains(requirement))
                {
                    canResearch = false;
                    break;
                }
            }
            if(canResearch)
            {
                researchNames.Add(research);
            }
        }
        return researchNames;
    }

    public List<string> AvaliableCultureResearches()
    {
        List<string> researchNames = new();
        foreach (string research in CultureResearchLoader.researchesDict.Keys)
        {
            bool canResearch = true;
            ResearchInfo info = CultureResearchLoader.researchesDict[research];
            foreach (string requirement in info.Requirements)
            {
                if (!completedCultureResearches.Contains(requirement))
                {
                    canResearch = false;
                    break;
                }
            }
            if (canResearch)
            {
                researchNames.Add(research);
            }
        }
        return researchNames;
    }

    public void IncreaseAllSettlerCost()
    {
        settlerCount += 1;
        foreach (int cityID in cityList)
        {
            City city = Global.gameManager.game.cityDictionary[cityID];
            city.IncreaseSettlerCost();
        }
    }

    public void DecreaseAllSettlerCost()
    {
        settlerCount -= 1;
        foreach (int cityID in cityList)
        {
            City city = Global.gameManager.game.cityDictionary[cityID];
            city.DecreaseSettlerCost();
        }
    }

    public void UpdateTerritoryGraphic()
    {
        GraphicGameBoard ggb = ((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);
        ggb.CallDeferred("UpdateTerritoryGraphic", teamNum);
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
            manager2.CallDeferred("Update2DUI", (int)UIElement.endTurnButton);
            manager2.CallDeferred("Update2DUI", (int)UIElement.researchTree);
        }
    }

    public void OnResearchComplete(String researchType)
    {
        completedResearches.Add(researchType);
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
        {
            manager.CallDeferred("Update2DUI", (int)UIElement.researchTree);
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
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager2)) manager2.CallDeferred("Update2DUI", (int)UIElement.endTurnButton);
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
            manager2.CallDeferred("Update2DUI", (int)UIElement.endTurnButton);
            manager2.CallDeferred("Update2DUI", (int)UIElement.researchTree);
        }
    }

    public void OnCultureResearchComplete(String researchType)
    {
        completedCultureResearches.Add(researchType);
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
        {
            manager.CallDeferred("Update2DUI", (int)UIElement.researchTree);
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
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager2)) manager2.CallDeferred("Update2DUI", (int)UIElement.endTurnButton);
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
                //check all outgoing traderoutes for this player to remove the resource from all trade routes using this city
                foreach (TradeRoute route in outgoingTradeRouteList)
                {
                    if (route.targetCityID == cityID)
                    {
                        Global.gameManager.game.playerDictionary[Global.gameManager.game.cityDictionary[route.homeCityID].teamNum].RemoveLostResource(hex);
                    }
                }
                break;
            }
        }
        globalResources.Remove(hex);
        unassignedResources.Remove(hex);
        return true;
    }

    public void RecalculateExportsFromCity(int sourceCity)
    {
        foreach (ExportRoute export in exportRouteList)
        {
            if (export.sourceCityID == sourceCity)
            {
                Global.gameManager.game.cityDictionary[export.targetCityID].RecalculateYields();
            }
        }
    }
    public void NewExportRoute(int city, int targetCity, YieldType exportType)
    {
        exportCount++;
        Global.gameManager.game.cityDictionary[city].NewExport(exportType);
        exportRouteList.Add(new ExportRoute(city, targetCity, exportType));
        Global.gameManager.game.cityDictionary[city].RecalculateYields();
        Global.gameManager.game.cityDictionary[targetCity].RecalculateYields();
    }

    public void RemoveExportRoute(int city, int targetCity, YieldType exportType)
    {
        exportCount--;
        Global.gameManager.game.cityDictionary[city].RemoveExport(exportType);
        exportRouteList.Remove(new ExportRoute(city, targetCity, exportType));
        Global.gameManager.game.cityDictionary[city].RecalculateYields();
        Global.gameManager.game.cityDictionary[targetCity].RecalculateYields();
    }

    public void NewTradeRoute(int homeCity, int targetCity)
    {
        TradeRoute route = new TradeRoute(homeCity, targetCity);
        tradeRouteList.Add(route);
        Global.gameManager.game.playerDictionary[Global.gameManager.game.cityDictionary[targetCity].teamNum].outgoingTradeRouteList.Add(route);
        List<ResourceType> resources = new();
        foreach (District district in Global.gameManager.game.cityDictionary[targetCity].districts)
        {
            if (Global.gameManager.game.mainGameBoard.gameHexDict[district.hex].resourceType != ResourceType.None)
            {
                unassignedResources.Add(district.hex, Global.gameManager.game.mainGameBoard.gameHexDict[district.hex].resourceType);
            }
        }
        tradeRouteCount++;
    }
    public void RemoveTradeRoute(int homeCity, int targetCity)
    {
        TradeRoute route = new TradeRoute(homeCity, targetCity);
        tradeRouteList.Remove(route);
        Global.gameManager.game.playerDictionary[Global.gameManager.game.cityDictionary[targetCity].teamNum].outgoingTradeRouteList.Remove(route);
        foreach (District district in Global.gameManager.game.cityDictionary[targetCity].districts)
        {
            if (Global.gameManager.game.mainGameBoard.gameHexDict[district.hex].resourceType != ResourceType.None)
            {
                RemoveLostResource(district.hex);
            }
        }
        tradeRouteCount--;
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

public enum GovernmentType
{
    Tribal,

    Autocracy,
    ClassicalRepublic,
    Oligarchy,

    MerchantRepublic,
    Monarchy,
    Theocracy,

    Communism,
    Democracy,
    Facism
}

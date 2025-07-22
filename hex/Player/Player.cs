using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data;
using Godot;
using System.IO;
using NetworkMessages;
using System.Drawing;

[Serializable]
public class Player : BasePlayer
{
    public Player(float goldTotal, int teamNum, FactionType faction, Godot.Color teamColor, bool isAI, bool isEncampment) : base(teamNum, teamColor, isAI, isEncampment)
    {
        this.goldTotal = goldTotal;
        administrativeCityCost = 20;
        administrativePopulationCost = 2;

        SelectResearch("Agriculture");
        SelectCultureResearch("TribalDominion");

        //default diplomatic actions
        diplomaticActionHashSet.Add(new DiplomacyAction(teamNum, "Give Gold", true, false));
        diplomaticActionHashSet.Add(new DiplomacyAction(teamNum, "Give Gold Per Turn", true, true));

        //default policy cards
        //unassignedPolicyCards.Add(PolicyCardLoader.policyCardDictionary[0]); //sample card

        //default yields
        SetBaseHexYields();

        this.faction = faction;

        Global.gameManager.game.playerDictionary.Add(teamNum, this);
        if (teamNum == Global.gameManager.game.localPlayerTeamNum)
        {
            Global.gameManager.game.localPlayerRef = this;
        }

        foreach (Hex hex in Global.gameManager.game.mainGameBoard.gameHexDict.Keys)
        {
            Global.gameManager.game.mainGameBoard.gameHexDict[hex].RecalculateYields();
            if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
            {
                var data = new Godot.Collections.Dictionary
                {
                    { "q", hex.q },
                    { "r", hex.r },
                    { "s", hex.s }
                };
                manager.CallDeferred("UpdateHex", data);
            }
        }

    }

    public Player()
    {
        //used for loading
    }
    public Dictionary<Hex, int> visibleGameHexDict { get; set; } = new();
    public Dictionary<Hex, bool> seenGameHexDict { get; set; } = new();
    public Dictionary<Hex, int> personalVisibleGameHexDict { get; set;} = new();
    public List<Hex> visibilityChangedList { get; set; } = new();
    public List<ResearchQueueType> queuedResearch { get; set; } = new();
    public Dictionary<String, ResearchQueueType> partialResearchDictionary { get; set; } = new();
    public HashSet<String> completedResearches { get; set; } = new();
    public List<ResearchQueueType> queuedCultureResearch { get; set; } = new();
    public Dictionary<String, ResearchQueueType> partialCultureResearchDictionary { get; set; } = new();
    public HashSet<String> completedCultureResearches { get; set; } = new();
    public float goldTotal { get; set; }
    public float scienceTotal { get; set; }
    public float cultureTotal { get; set; }


    // happinessTotal reaches administrativeupkeep you get a golden age, golden age locks happinessTotal at 100? 
    //provides a boost, if happinessTotal reaches some negative number enter a dark age lock happinessTotal at 0 provides some effect, bad but someway to help resolve happiness deficit happinessTotal
    public float happinessTotal { get; set; }
    public int goldenAgeTurnsLeft { get; set; } = 0;
    public int darkAgeTurnsLeft { get; set; } = 0;
    public float influenceTotal { get; set; }
    public float administrativeUpkeep { get; set; } = 100; //administrativeUpkeep is increased by each city and each population
    public float administrativeCityCost { get; set; }
    public float administrativePopulationCost { get; set; } 
    public int settlerCount { get; set; } = 0;
    //exports and trade
    public List<ExportRoute> exportRouteList { get; set; } = new();
    public List<TradeRoute> tradeRouteList { get; set; } = new();
    public List<TradeRoute> outgoingTradeRouteList { get; set; } = new();
    public int exportCount { get; set; }
    public int exportCap { get; set; } = 0;
    public int baseMaxTradeRoutes { get; set; } = 2;
    public int tradeRouteCount { get; set; }

    //encampments
    public List<int> occupiedEncampments { get; set; } = new();
    public List<int> alliedEncampments { get; set; } = new();


    //policy card helpers
    public int goldPerTradeRoute { get; set; }
    public int sciencePerTradeRoute { get; set; }
    public int culturePerTradeRoute { get; set; }
    public int sciencePerEncampment { get; set; }
    public int culturePerEncampment { get; set; }
    public int influencePerEncampment { get; set; }
    public int cityCombatStrengthMod { get; set; }
    public Dictionary<string, (DistrictType, float)> districtTypeProductionBoosts { get; set; } = new();
    public Dictionary<string, (UnitClass, float)> unitClassProductionBoosts { get; set; } = new();

    //era seperator research and culture counts
    public int industrialInsightCulturalResearchCount { get; set; } = 0;
    public int industrialInsightResearchCount { get; set; } = 0;




    private void SetBaseHexYields()
    {
        flatYields.food = 1;
        roughYields.production = 1;
        //mountainYields.production += 0;
        coastalYields.food = 1;
        //oceanYields.gold = 1;

        desertYields.gold = 1;
        plainsYields.production = 1;
        grasslandYields.food = 1;
        tundraYields.happiness = 2;
        //arcticYields

        forestYields.production = 1;
        coralYields.production = 1;
        wetlandYields.food = 1;
    }


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

    public new float GetGoldPerTurn()
    {
        float goldPerTurn = base.GetGoldPerTurn();
        goldPerTurn += goldPerTradeRoute * (tradeRouteCount + outgoingTradeRouteList.Count);
        return goldPerTurn;
    }

    public new float GetSciencePerTurn()
    {
        float sciencePerTurn = base.GetSciencePerTurn();
        //trade
        sciencePerTurn += sciencePerTradeRoute * (tradeRouteCount + outgoingTradeRouteList.Count);
        //encampments
        sciencePerTurn += sciencePerEncampment * (occupiedEncampments.Count + alliedEncampments.Count);
        return sciencePerTurn;
    }

    public new float GetCulturePerTurn()
    {
        float culturePerTurn = base.GetCulturePerTurn();
        //trade
        culturePerTurn += culturePerTradeRoute * (tradeRouteCount + outgoingTradeRouteList.Count);
        //encampments
        culturePerTurn += culturePerEncampment * (occupiedEncampments.Count + alliedEncampments.Count);

        return culturePerTurn;
    }

    public new float GetInfluencePerTurn()
    {
        float influencePerTurn = base.GetInfluencePerTurn();
        influencePerTurn += influencePerEncampment * (occupiedEncampments.Count + alliedEncampments.Count);
        return influencePerTurn;
    }

    public override void OnTurnStarted(int turnNumber)
    {
        base.OnTurnStarted(turnNumber);
        goldPerTurnFromTrade = 0;
        foreach (var tradeDeal in Global.gameManager.game.teamManager.goldTradeDealDict)
        {
            if (tradeDeal.Key.reciever == teamNum)
            {
                goldPerTurnFromTrade += tradeDeal.Value.amount;
            }
            else if (tradeDeal.Key.sender == teamNum)
            {
                goldPerTurnFromTrade -= tradeDeal.Value.amount;
            }
        }
        AddGold(goldPerTurnFromTrade);
        administrativeUpkeep = 100;
        if (FactionLoader.IsFactionMinor(faction))
        {
            AddCheatCultureForTurn();
            AddCheatScienceForTurn();
        }
        foreach (int cityID in cityList)
        {
            City city = Global.gameManager.game.cityDictionary[cityID];
            administrativeUpkeep += city.naturalPopulation * administrativePopulationCost;
            administrativeUpkeep += administrativeCityCost;
        }
        if(queuedResearch.Any() && cityList.Any())
        {
            float cost = queuedResearch[0].researchLeft;
            queuedResearch[0].researchLeft -= (int)Math.Round(scienceTotal);
            scienceTotal -= cost;
            scienceTotal = Math.Max(0.0f, scienceTotal);
            if(queuedResearch[0].researchLeft <= 0)
            {
                OnResearchComplete(queuedResearch[0].researchType);
            }
        }
        if (queuedCultureResearch.Any() && cityList.Any())
        {
            float cost = queuedCultureResearch[0].researchLeft;
            queuedCultureResearch[0].researchLeft -= (int)Math.Round(cultureTotal);
            cultureTotal -= cost;
            cultureTotal = Math.Max(0.0f, cultureTotal);
            if (queuedCultureResearch[0].researchLeft <= 0)
            {
                OnCultureResearchComplete(queuedCultureResearch[0].researchType);
            }
        }
        
        // happinessTotal reaches administrativeupkeep you get a golden age, golden age locks happinessTotal at 100? 
        //provides a boost, if happinessTotal reaches some negative number enter a dark age lock happinessTotal at 0 provides some effect, bad but someway to help resolve happiness deficit happinessTotal
        if(happinessTotal > administrativeUpkeep)
        {
            if (Global.gameManager.game.localPlayerTeamNum == teamNum && Global.gameManager.TryGetGraphicManager(out GraphicManager manager2))
            {
                manager2.uiManager.CallDeferred("SetTopBarColor", Godot.Colors.Goldenrod);
            }
            goldenAgeTurnsLeft = 30;
            darkAgeTurnsLeft = 0;
            happinessTotal = 0;
        }
        else if(happinessTotal < -100)
        {
            if (Global.gameManager.game.localPlayerTeamNum == teamNum && Global.gameManager.TryGetGraphicManager(out GraphicManager manager2))
            {
                manager2.uiManager.CallDeferred("SetTopBarColor", Godot.Colors.Gray);
            }
            darkAgeTurnsLeft = 30;
            goldenAgeTurnsLeft = 0;
            happinessTotal = 0;
        }
        if(goldenAgeTurnsLeft > 0)
        {
            goldenAgeTurnsLeft -= 1;
        }
        if(darkAgeTurnsLeft > 0)
        {
            darkAgeTurnsLeft -= 1;
        }
        if (exportCount > exportCap)
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
            if (teamNum == Global.gameManager.game.localPlayerTeamNum)
            {
                manager.uiManager.CallDeferred("NewTurnStarted");
                manager.uiManager.CallDeferred("UpdateAll");
                manager.CallDeferred("Update2DUI", (int)UIElement.researchTree);
                manager.uiManager.UpdateResearchUI();
            }
        }
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
        queuedResearch.RemoveAt(0);
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
        foreach(ResourceType resourceType in ResearchLoader.researchesDict[researchType].ResourceUnlocks)
        {
            hiddenResources.Remove(resourceType);
            foreach (var (hex, resource) in hiddenGlobalResources)
            {
                if(resource == resourceType)
                {
                    hiddenGlobalResources.Remove(hex);
                    globalResources.Add(hex, resource);
                }
            }
            foreach (Hex hex in Global.gameManager.game.mainGameBoard.gameHexDict.Keys)
            {
                var data = new Godot.Collections.Dictionary
                {
                    { "q", hex.q },
                    { "r", hex.r },
                    { "s", hex.s }
                };
                if(manager != null) manager.CallDeferred("UpdateHex", data);
            }
        }
        if (ResearchLoader.researchesDict[researchType].PolicyCardUnlocks != null)
        {
            foreach (String policyCard in ResearchLoader.researchesDict[researchType].PolicyCardUnlocks)
            {
                unassignedPolicyCards.Add(PolicyCardLoader.GetPolicyCard(policyCard));
            }
        }
        if (ResearchLoader.researchesDict[researchType].GovernmentUnlocks != null)
        {
            foreach (GovernmentType governmentType in ResearchLoader.researchesDict[researchType].GovernmentUnlocks)
            {
                avaliableGovernments.Add(governmentType);
            }
        }
        foreach (String effect in ResearchLoader.researchesDict[researchType].Effects)
        {
            ResearchLoader.ProcessFunctionString(effect, this);
        }
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager2))
        {
            manager2.CallDeferred("Update2DUI", (int)UIElement.endTurnButton);
            manager2.uiManager.CallDeferred("UpdateResearchUI");

            
        }
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
        queuedCultureResearch.RemoveAt(0);
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
        if(CultureResearchLoader.researchesDict[researchType].ResourceUnlocks != null)
        {
            foreach (ResourceType resourceType in CultureResearchLoader.researchesDict[researchType].ResourceUnlocks)
            {
                hiddenResources.Remove(resourceType);
                foreach (var (hex, resource) in hiddenGlobalResources)
                {
                    if (resource == resourceType)
                    {
                        hiddenGlobalResources.Remove(hex);
                        globalResources.Add(hex, resource);
                    }
                }
                foreach (Hex hex in Global.gameManager.game.mainGameBoard.gameHexDict.Keys)
                {
                    var data = new Godot.Collections.Dictionary
                {
                    { "q", hex.q },
                    { "r", hex.r },
                    { "s", hex.s }
                };
                    if (manager != null) manager.CallDeferred("UpdateHex", data);
                }
            }
        }
        if (CultureResearchLoader.researchesDict[researchType].PolicyCardUnlocks != null)
        {
            foreach (String policyCard in CultureResearchLoader.researchesDict[researchType].PolicyCardUnlocks)
            {
                unassignedPolicyCards.Add(PolicyCardLoader.GetPolicyCard(policyCard));
            }
        }
        if (CultureResearchLoader.researchesDict[researchType].GovernmentUnlocks != null)
        {
            foreach (GovernmentType governmentType in CultureResearchLoader.researchesDict[researchType].GovernmentUnlocks)
            {
                avaliableGovernments.Add(governmentType);
            }
        }
        foreach (String effect in CultureResearchLoader.researchesDict[researchType].Effects)
        {
            CultureResearchLoader.ProcessFunctionString(effect, this);
        }
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager2))
        {
            manager2.CallDeferred("Update2DUI", (int)UIElement.endTurnButton);
            manager2.uiManager.CallDeferred("UpdateResearchUI");
        }
    }
    
    
    public override bool RemoveLostResource(Hex hex)
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
        hiddenGlobalResources.Remove(hex);
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


    public int GetMaxTradeRoutes()
    {
        int maxTradeRoutes = baseMaxTradeRoutes;
        foreach (int cityID in cityList)
        {
            City city = Global.gameManager.game.cityDictionary[cityID];
            maxTradeRoutes += city.additionalTradeRoutes;
        }
        return maxTradeRoutes;
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

    public void AssignPolicyCard(int policyCardID)
    {
        PolicyCard policyCard = PolicyCardLoader.GetPolicyCard(policyCardID);
        Global.gameManager.game.playerDictionary[teamNum].activePolicyCards.Remove(policyCard);
        Global.gameManager.game.playerDictionary[teamNum].activePolicyCards.Add(policyCard);
        Global.gameManager.game.playerDictionary[teamNum].unassignedPolicyCards.Remove(policyCard);

        policyCard.AddEffect(this.teamNum);
    }

    public void UnassignPolicyCard(int policyCardID)
    {
        PolicyCard policyCard = PolicyCardLoader.GetPolicyCard(policyCardID);
        Global.gameManager.game.playerDictionary[teamNum].activePolicyCards.Remove(policyCard);
        Global.gameManager.game.playerDictionary[teamNum].unassignedPolicyCards.Add(policyCard);
    }

    public void AddGold(float gold)
    {
        SetGoldTotal(GetGoldTotal() + gold);
    }
    public void AddScience(float science)
    {
        SetScienceTotal(GetScienceTotal() + science);
    }

    public void AddCheatScienceForTurn()
    {
        int playerCount = 0;
        int globalScienceTotal = 0;
        foreach (Player player in Global.gameManager.game.playerDictionary.Values)
        {
            if (!FactionLoader.IsFactionMinor(player.faction))
            {
                playerCount++;
                foreach (int cityID in player.cityList)
                {
                    globalScienceTotal += (int)Math.Round(Global.gameManager.game.cityDictionary[cityID].yields.science);
                }
            }
        }
        globalScienceTotal /= playerCount;
        SetScienceTotal(GetScienceTotal() + globalScienceTotal);
    }
    public void AddCulture(float culture)
    {
        SetCultureTotal(GetCultureTotal() + culture);
    }

    public void AddCheatCultureForTurn()
    {
        int playerCount = 0;
        int globalCultureTotal = 0;
        foreach (Player player in Global.gameManager.game.playerDictionary.Values)
        {
            if (!FactionLoader.IsFactionMinor(player.faction))
            {
                playerCount++;
                foreach (int cityID in player.cityList)
                {
                    globalCultureTotal += (int)Math.Round(Global.gameManager.game.cityDictionary[cityID].yields.science);
                }
            }
        }
        globalCultureTotal /= playerCount;
        SetCultureTotal(GetCultureTotal() + globalCultureTotal);
    }
    public void AddHappiness(float happiness)
    {
        SetHappinessTotal(GetHappinessTotal() + happiness);
    }
    public void AddInfluence(float influence)
    {
        SetInfluenceTotal(GetInfluenceTotal() + influence);
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
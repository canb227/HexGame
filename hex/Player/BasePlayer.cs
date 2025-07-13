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
public class BasePlayer
{
    public BasePlayer(int teamNum, Godot.Color teamColor, bool isAI)
    {
        this.teamColor = teamColor;
        this.teamNum = teamNum;
        this.isAI = isAI;

        hiddenResources.Add(ResourceType.Horses);
        hiddenResources.Add(ResourceType.Iron);
        hiddenResources.Add(ResourceType.Niter);
        hiddenResources.Add(ResourceType.Coal);
        hiddenResources.Add(ResourceType.Oil);
        hiddenResources.Add(ResourceType.Uranium);
        hiddenResources.Add(ResourceType.Lithium);

        SetBaseHexYields();
        avaliableGovernments.Add(GovernmentType.Tribal);
        //testing
        avaliableGovernments.Add(GovernmentType.Democracy);
        //
        SetGovernment(GovernmentType.Tribal);
        PlayerEffect.ProcessFunctionString("AddTribalGovernmentEffect", this); //you should only apply government effects once and they are permanent

        Global.gameManager.game.teamManager.AddTeam(teamNum, 50);

        theme = new Theme();

        playerTerritoryMaterial = new StandardMaterial3D();

        Gradient gradient = new Gradient();
        gradient.SetColor(0, new Godot.Color(teamColor.R, teamColor.G, teamColor.B, 0.2f));
        gradient.SetColor(1, teamColor);

        GradientTexture1D gradTex = new GradientTexture1D();
        gradTex.Gradient = gradient;
        gradTex.Width = 256;

        playerTerritoryMaterial.AlbedoTexture = gradTex;
        playerTerritoryMaterial.Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
    }

    public BasePlayer()
    {
        //used for loading
    }
    public bool isAI { get; set; } = false;
    public int teamNum { get; set; }
    public FactionType faction { get; set; } = FactionType.Human;
    public bool turnFinished { get; set; }
    public List<int> unitList { get; set; } = new();
    public List<int> cityList { get; set; } = new();
    public List<(string, UnitEffect, UnitClass)> unitPlayerEffects { get; set; } = new();
    public List<(string, BuildingEffect, String)> buildingPlayerEffects { get; set; } = new();
    public HashSet<String> allowedBuildings { get; set; } = new();
    public HashSet<DistrictType> allowedDistricts { get; set; } = new();
    public HashSet<String> allowedUnits { get; set; } = new();
    public HashSet<ResourceType> hiddenResources { get; set; } = new();
    public Dictionary<Hex, ResourceType> unassignedResources { get; set; } = new();
    public Dictionary<Hex, ResourceType> globalResources { get; set; } = new();
    public Dictionary<Hex, ResourceType> hiddenGlobalResources { get; set;} = new();
    public GovernmentType government { get; set; }
    public HashSet<GovernmentType> avaliableGovernments { get; set;} = new();
    public float strongestUnitBuilt { get; set; } = 0.0f;
    private int idCounter { get; set; } = 1;
    public Yields flatYields { get; set; } = new();
    public Yields roughYields { get; set; } = new();
    public Yields mountainYields { get; set; } = new();
    public Yields coastalYields { get; set; } = new();
    public Yields oceanYields { get; set; } = new();
    public Yields desertYields { get; set; } = new();
    public Yields plainsYields { get; set; } = new();
    public Yields grasslandYields { get; set; } = new();
    public Yields tundraYields { get; set; } = new();
    public Yields arcticYields { get; set; } = new();

    public List<PolicyCard> unassignedPolicyCards { get; set; } = new();
    public List<PolicyCard> activePolicyCards { get; set; } = new();
    public int militaryPolicySlots { get; set; } = 0;
    public int economicPolicySlots { get; set; } = 0;
    public int diplomaticPolicySlots { get; set; } = 0;
    public int heroicPolicySlots { get; set; } = 0;

    public int baseFlat;
    public int baseRough;
    public int baseMountain;
    public int baseCoastal;
    public int baseOcean;

    public int baseDesert;
    public int basePlains;
    public int baseGrassland;
    public int baseTundra;
    public int baseArctic;

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

    private void SetBaseHexYields()
    {
        //flatYields += baseFlat;
        roughYields.production = 1;
        //mountainYields.production += 0;
        coastalYields.food = 1;
        oceanYields.gold = 1;

        desertYields.gold = 1;
        plainsYields.production = 1;
        grasslandYields.food = 1;
        tundraYields.happiness = 1;
        //arcticYields

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


    internal int GetNextUniqueID()
    {
        string teamID = ""; //teamnum with minimum three digits (1 = 100, 3 = 300, 74 = 740, 600 = 600, 999 = 999)
        if (teamNum > 999)
        {
            throw new Exception("Team number exceeds 999, cannot generate unique ID.");
        }

        if (teamNum < 0)
        {
            throw new Exception("Team number cannot be negative, cannot generate unique ID.");
        }

        if (idCounter > 999999)
        {
            throw new Exception("IDCounter exceeds 999999, cannot generate unique ID.");
        }

        if (teamNum < 10)
        {
            teamID = teamNum.ToString() + "00";
        }
        else if (teamNum < 100)
        {
            teamID = teamNum.ToString() + "0";
        }
        else
        {
            teamID = teamNum.ToString();
        }
        string idString = teamID + idCounter.ToString();
        idCounter++;
        return int.Parse(idString);

    }

    public virtual void OnTurnStarted(int turnNumber, bool updateUI)
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

        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager) && updateUI)
        {

            manager.uiManager.CallDeferred("NotWaitingOnLocalPlayer");
            manager.uiManager.CallDeferred("UpdateAll");
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
        List<int> waitingPlayers = Global.gameManager.game.turnManager.CheckTurnStatus();
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
        {
            if (waitingPlayers.Count() == 1)
            {
                if (waitingPlayers[0] == Global.gameManager.game.localPlayerTeamNum)
                {
                    manager.uiManager.CallDeferred("GameWaitingOnLocalPlayer");
                    Global.gameManager.audioManager.CallDeferred("PlayAudio", "res://audio/soundeffects/LastPlayer.wav");
                }
            }
        }
    }

    public void UpdateTerritoryGraphic()
    {
        //GraphicGameBoard ggb = ((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);
        //ggb.CallDeferred("UpdateTerritoryGraphic", teamNum);
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
    
    
    public virtual bool RemoveLostResource(Hex hex)
    {

        foreach (int cityID in cityList)
        {
            City city = Global.gameManager.game.cityDictionary[cityID];
            if (city.heldResources.Remove(hex))
            {
                break;
            }
        }
        globalResources.Remove(hex);
        hiddenGlobalResources.Remove(hex);
        unassignedResources.Remove(hex);
        return true;
    }

    public void SetGovernment(GovernmentType governmentType)
    {
        if(government != null)
        {
            if(government == GovernmentType.Tribal)
            {
                PlayerEffect.RemoveTribalGovernmentEffect(this);
            }
        }
        government = governmentType;
    }


}

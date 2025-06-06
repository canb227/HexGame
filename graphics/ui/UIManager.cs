﻿using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public enum UIElement
{
    gold,
    science,
    culture,
    happiness,
    influence,
    turnNumber,
    goldPerTurn,
    sciencePerTurn,
    culturePerTurn,
    happinessPerTurn,
    influencePerTurn,
    unitDisplay,
    endTurnButton,
    researchTree
}

public partial class UIManager : Node3D
{
    public Button endTurnButton;

    private Layout layout;
    private Control screenUI;
    public Label goldLabel { get; set; }
    public Label goldPerTurnLabel { get; set; }
    public Label sciencePerTurnLabel;
    public Label culturePerTurnLabel;
    public Label happinessLabel;
    public Label happinessPerTurnLabel;
    public Label influenceLabel;
    public Label influencePerTurnLabel;
    public Label turnNumberLabel;

    public Button scienceButton;
    public Label scienceButtonLabel;
    public TextureRect scienceButtonIcon;
    public HBoxContainer scienceButtonResults;
    public Label scienceButtonTurnsLeft;


    public Button cultureButton;
    public Label cultureButtonLabel;
    public TextureRect cultureButtonIcon;
    public HBoxContainer cultureButtonResults;
    public Label cultureButtonTurnsLeft;

    public UnitInfoPanel unitInfoPanel;
    public CityInfoPanel cityInfoPanel;
    public ResearchTreePanel researchTreePanel;
    public ResearchTreePanel cultureResearchTreePanel;

    public City targetCity;
    public Unit targetUnit;

    public bool pickScience;
    public bool pickCulture;

    public bool readyToGrow;
    public bool cityNeedsProduction;

    public bool waitingForOrders;

    public UIManager(Layout layout)
    {
        this.layout = layout;
        screenUI = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/gameui.tscn").Instantiate<Control>();

        goldLabel = screenUI.GetNode<Label>("PanelContainer/TopBar/Resources/GoldLabel");
        goldPerTurnLabel = screenUI.GetNode<Label>("PanelContainer/TopBar/Resources/GoldPerTurnLabel");
        sciencePerTurnLabel = screenUI.GetNode<Label>("PanelContainer/TopBar/Resources/SciencePerTurnLabel");
        culturePerTurnLabel = screenUI.GetNode<Label>("PanelContainer/TopBar/Resources/CulturePerTurnLabel");
        happinessLabel = screenUI.GetNode<Label>("PanelContainer/TopBar/Resources/HappinessLabel");
        happinessPerTurnLabel = screenUI.GetNode<Label>("PanelContainer/TopBar/Resources/HappinessPerTurnLabel");
        influenceLabel = screenUI.GetNode<Label>("PanelContainer/TopBar/Resources/InfluenceLabel");
        influencePerTurnLabel = screenUI.GetNode<Label>("PanelContainer/TopBar/Resources/InfluencePerTurnLabel");
        turnNumberLabel = screenUI.GetNode<Label>("PanelContainer/TopBar/GameInfo/TurnLabel");

        scienceButton = screenUI.GetNode<Button>("ScienceTree");
        scienceButtonLabel = scienceButton.GetNode<Label>("ResearchLabel");
        scienceButtonIcon = scienceButton.GetNode<TextureRect>("ScienceTreeIcon");
        scienceButtonResults = scienceButton.GetNode<HBoxContainer>("ResearchResultBox");
        scienceButtonTurnsLeft = scienceButton.GetNode<Label>("TurnsLeft");

        cultureButton = screenUI.GetNode<Button>("CultureTree");
        cultureButtonLabel = cultureButton.GetNode<Label>("ResearchLabel");
        cultureButtonIcon = cultureButton.GetNode<TextureRect>("CultureTreeIcon");
        cultureButtonResults = cultureButton.GetNode<HBoxContainer>("ResearchResultBox");
        cultureButtonTurnsLeft = scienceButton.GetNode<Label>("TurnsLeft");

        scienceButton.Pressed += () => ScienceTreeButtonPressed();
        cultureButton.Pressed += () => CultureTreeButtonPressed();

        goldLabel.Text = "0 ";
        goldPerTurnLabel.Text = "(+0) ";
        sciencePerTurnLabel.Text = "+0 ";
        culturePerTurnLabel.Text = "+0 ";
        happinessLabel.Text = "0 ";
        happinessPerTurnLabel.Text = "(+0) ";
        influenceLabel.Text = "0 ";
        influencePerTurnLabel.Text = "(+0) ";
        SetupTurnUI();
        
        unitInfoPanel = new UnitInfoPanel();
        unitInfoPanel.Name = "UnitInfoPanel";
        AddChild(unitInfoPanel);
        unitInfoPanel.Visible = false;

        cityInfoPanel = new CityInfoPanel();
        cityInfoPanel.Name = "CityInfoPanel";
        AddChild(cityInfoPanel);
        cityInfoPanel.cityInfoPanel.Visible = false;

        researchTreePanel = new ResearchTreePanel(ResearchLoader.researchesDict, false);
        researchTreePanel.Name = "ResearchTreePanel";
        researchTreePanel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        AddChild(researchTreePanel);
        researchTreePanel.Visible = false;

        cultureResearchTreePanel = new ResearchTreePanel(CultureResearchLoader.researchesDict, true);
        cultureResearchTreePanel.Name = "CultureResearchTreePanel";
        cultureResearchTreePanel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        AddChild(cultureResearchTreePanel);
        cultureResearchTreePanel.Visible = false;

        UpdateAll();
        AddChild(screenUI);
    }

    private void SetupTurnUI()
    {
        endTurnButton = screenUI.GetNode<Button>("EndTurnButton");
        endTurnButton.Pressed += endTurnButtonPressed;
    }

    public void HideGenericUIForTargeting()
    {
        endTurnButton.Visible = false;
        scienceButton.Visible = false;
        cultureButton.Visible = false;
    }

    public void ShowGenericUIAfterTargeting()
    {
        endTurnButton.Visible = true;
        scienceButton.Visible = true;
        cultureButton.Visible = true;
    }

    public void UpdateAll()
    {
        goldLabel.Text = Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].GetGoldTotal().ToString() + " ";
        goldPerTurnLabel.Text = "(+" + Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].GetGoldPerTurn().ToString() + ")  ";
        sciencePerTurnLabel.Text = " +" + Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].GetSciencePerTurn().ToString() + "  ";
        culturePerTurnLabel.Text = " +" + Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].GetCulturePerTurn().ToString() + "  ";
        happinessLabel.Text = Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].GetHappinessTotal().ToString() + " ";
        happinessPerTurnLabel.Text = "(+" + Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].GetHappinessPerTurn().ToString() + ")  ";
        influenceLabel.Text = Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].GetInfluenceTotal().ToString() + " ";
        influencePerTurnLabel.Text = "(+" + Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].GetInfluencePerTurn().ToString() + ")  ";
        turnNumberLabel.Text = " " + Global.gameManager.game.turnManager.currentTurn;

        UpdateUnitUIDisplay();
        UpdateEndTurnButton();
        researchTreePanel.UpdateResearchUI();
        cultureResearchTreePanel.UpdateResearchUI();
        UpdateResearchUI();
    }

    public void Update(UIElement element)
    {
        if (element == UIElement.gold)
        {
            goldLabel.Text = Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].GetGoldTotal().ToString() + " ";
        }
        else if (element == UIElement.goldPerTurn)
        {
            goldPerTurnLabel.Text = "(+" + Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].GetGoldPerTurn().ToString() + ")  ";
        }
        else if (element == UIElement.sciencePerTurn)
        {
            sciencePerTurnLabel.Text = " +" + Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].GetSciencePerTurn().ToString() + "  ";
        }
        else if (element == UIElement.culturePerTurn)
        {
            culturePerTurnLabel.Text = " +" + Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].GetCulturePerTurn().ToString() + "  ";
        }
        else if (element == UIElement.happiness)
        {
            happinessLabel.Text = Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].GetHappinessTotal().ToString() + " ";
        }
        else if (element == UIElement.happinessPerTurn)
        {
            happinessPerTurnLabel.Text = "(+" + Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].GetHappinessPerTurn().ToString() + ")  ";
        }
        else if (element == UIElement.influence)
        {
            influenceLabel.Text = Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].GetInfluenceTotal().ToString() + " ";
        }
        else if (element == UIElement.influencePerTurn)
        {
            influencePerTurnLabel.Text = "(+" + Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].GetInfluencePerTurn().ToString() + ")  ";
        }
        else if (element == UIElement.turnNumber)
        {
            turnNumberLabel.Text = " " + Global.gameManager.game.turnManager.currentTurn;
        }
        else if (element == UIElement.unitDisplay)
        {
            UpdateUnitUIDisplay();
        }
        else if (element == UIElement.endTurnButton)
        {
            UpdateEndTurnButton();
        }
        else if (element == UIElement.researchTree)
        {
            researchTreePanel.UpdateResearchUI();
            cultureResearchTreePanel.UpdateResearchUI();
            UpdateResearchUI();
        }
    }

    public void UpdateResearchUI()
    {
        if (Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].queuedResearch.Any())
        {
            ResearchInfo info = ResearchLoader.researchesDict[Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].queuedResearch.First().researchType];
            scienceButtonLabel.Text = Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].queuedResearch.First().researchType;
            scienceButtonIcon.Texture = Godot.ResourceLoader.Load<Texture2D>("res://" + info.IconPath);
            foreach (Node child in scienceButtonResults.GetChildren())
            {
                child.QueueFree();
            }
            foreach (String unitName in info.UnitUnlocks)
            {
                TextureRect unitIcon = researchTreePanel.researchEffectScene.Instantiate<TextureRect>();
                unitIcon.Texture = Godot.ResourceLoader.Load<Texture2D>("res://" + UnitLoader.unitsDict[unitName].IconPath);
                scienceButtonResults.AddChild(unitIcon);
            }
            foreach (String buildingName in info.BuildingUnlocks)
            {
                TextureRect buildingIcon = researchTreePanel.researchEffectScene.Instantiate<TextureRect>();
                buildingIcon.Texture = Godot.ResourceLoader.Load<Texture2D>("res://" + BuildingLoader.buildingsDict[buildingName].IconPath);
                scienceButtonResults.AddChild(buildingIcon);
            }
            GD.Print(Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].queuedResearch.First().researchLeft);
            GD.Print(Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].GetSciencePerTurn());
            scienceButtonTurnsLeft.Text = (Math.Ceiling(Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].queuedResearch.First().researchLeft / Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].GetScienceTotal())).ToString();
            cultureButtonTurnsLeft.Text = (Math.Ceiling(Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].queuedCultureResearch.First().researchLeft / Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].GetCultureTotal())).ToString();
        }


        if (Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].queuedCultureResearch.Any())
        {
            ResearchInfo info = CultureResearchLoader.researchesDict[Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].queuedCultureResearch.First().researchType];
            cultureButtonLabel.Text = Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].queuedCultureResearch.First().researchType;
            cultureButtonIcon.Texture = Godot.ResourceLoader.Load<Texture2D>("res://" + info.IconPath);
            foreach (Node child in cultureButtonResults.GetChildren())
            {
                child.QueueFree();
            }
            foreach (String unitName in info.UnitUnlocks)
            {
                TextureRect unitIcon = cultureResearchTreePanel.researchEffectScene.Instantiate<TextureRect>();
                unitIcon.Texture = Godot.ResourceLoader.Load<Texture2D>("res://" + UnitLoader.unitsDict[unitName].IconPath);
                cultureButtonResults.AddChild(unitIcon);
            }
            foreach (String buildingName in info.BuildingUnlocks)
            {
                TextureRect buildingIcon = cultureResearchTreePanel.researchEffectScene.Instantiate<TextureRect>();
                buildingIcon.Texture = Godot.ResourceLoader.Load<Texture2D>("res://" + BuildingLoader.buildingsDict[buildingName].IconPath);
                cultureButtonResults.AddChild(buildingIcon);
            }
            cultureButtonTurnsLeft.Text = (Math.Ceiling(Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].queuedCultureResearch.First().researchLeft / Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].GetCulturePerTurn())).ToString();
        }
    }

    public void UnitSelected(Unit unit)
    {
        unitInfoPanel.UnitSelected(unit);
    }

    public void UnitUnselected(Unit unit)
    {
        unitInfoPanel.UnitUnselected(unit);
    }

    public void UpdateUnitUIDisplay()
    {
        unitInfoPanel.UpdateUnitPanelInfo();
    }

    private void endTurnButtonPressed()
    {
        researchTreePanel.Visible = false;
        if (pickScience)
        {
            ScienceTreeButtonPressed();
            return;
        }
        if (pickCulture)
        {
            CultureTreeButtonPressed();
            return;
        }
        if (readyToGrow)
        {
            ((GraphicCity)Global.gameManager.graphicManager.graphicObjectDictionary[targetCity.id]).GenerateGrowthTargetingPrompt();
            return;
        }
        else if(cityNeedsProduction)
        {
            GD.Print("target"+targetCity);
            GD.Print("graphic"+(GraphicCity)Global.gameManager.graphicManager.graphicObjectDictionary[targetCity.id]);
            Global.gameManager.graphicManager.ChangeSelectedObject(targetCity.id, (GraphicCity)Global.gameManager.graphicManager.graphicObjectDictionary[targetCity.id]);
            return;
        }
        else if(waitingForOrders)
        {
            //move camera to foundUnit.hex TODO
            Global.gameManager.graphicManager.ChangeSelectedObject(targetUnit.id , Global.gameManager.graphicManager.graphicObjectDictionary[targetUnit.id]);
            return;
        }
        else
        {
            Global.gameManager.game.turnManager.EndCurrentTurn(Global.gameManager.game.localPlayerTeamNum);
            return;
        }
    }
    
    private void UpdateEndTurnButton()
    {
        if(Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].queuedResearch.Count == 0)
        {
            pickScience = true;
            endTurnButton.Icon = Godot.ResourceLoader.Load<Texture2D>("res://graphics/ui/icons/science.png");
            return;
        }
        else
        {
            pickScience = false;
        }

        if (Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].queuedCultureResearch.Count == 0)
        {
            pickCulture = true;
            endTurnButton.Icon = Godot.ResourceLoader.Load<Texture2D>("res://graphics/ui/icons/culture.png");
            return;
        }
        else
        {
            pickCulture = false;
        }

        bool cityReadyToGrow = false;
        City foundCity = null;
        foreach(int cityID in Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].cityList)
        {
            City city = Global.gameManager.game.cityDictionary[cityID];
            if (city.readyToExpand > 0)
            {
                foundCity = city;
                endTurnButton.Icon = Godot.ResourceLoader.Load<Texture2D>("res://graphics/ui/icons/house.png");
                readyToGrow = true;
                targetCity = city;
                waitingForOrders = false;
                cityNeedsProduction = false;
                return;
            }
            if(city.productionQueue.Count == 0)
            {
                endTurnButton.Icon = Godot.ResourceLoader.Load<Texture2D>("res://graphics/ui/icons/gears.png");
                readyToGrow = false;
                cityNeedsProduction = true;
                targetCity = city;
                waitingForOrders = false;
                return;
            }
        }

        bool unitNeedsOrders = false;
        Unit foundUnit = null;
        foreach (int unitID in Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].unitList)
        {
            Unit unit = Global.gameManager.game.unitDictionary[unitID];
            if (unit.remainingMovement > 0 && unit.currentPath.Count == 0 && !unit.isSleeping)
            {
                endTurnButton.Icon = Godot.ResourceLoader.Load<Texture2D>("res://graphics/ui/icons/moveicon.png");
                readyToGrow = false;
                cityNeedsProduction = false;
                targetCity = null;
                waitingForOrders = true;
                targetUnit = unit;
                return;
            }
        }
        if (unitNeedsOrders)
        {

        }

        endTurnButton.Icon = Godot.ResourceLoader.Load<Texture2D>("res://graphics/ui/icons/skipturn.png");
        readyToGrow = false;
        cityNeedsProduction = false;
        targetCity = null;
        waitingForOrders = false;
    }

    public void ScienceTreeButtonPressed()
    {
        researchTreePanel.Visible = true;
        var timer = new Timer();
        timer.WaitTime = 0.01; // Delay for 0.1 seconds (adjust as needed)
        timer.OneShot = true;
        AddChild(timer);
        timer.Start();

        timer.Timeout += () => researchTreePanel.AddLines();
    }

    public void CultureTreeButtonPressed()
    {
        cultureResearchTreePanel.Visible = true;
        var timer = new Timer();
        timer.WaitTime = 0.01; // Delay for 0.1 seconds (adjust as needed)
        timer.OneShot = true;
        AddChild(timer);
        timer.Start();

        timer.Timeout += () => cultureResearchTreePanel.AddLines();
    }
}

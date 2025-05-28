using Godot;
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
    endTurnButton
}

public partial class UIManager : Node3D
{
    public Button endTurnButton;
    private Game game;
    private Layout layout;
    private GraphicManager graphicManager;
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
    public Button cultureButton;

    public UnitInfoPanel unitInfoPanel;
    public CityInfoPanel cityInfoPanel;

    public City targetCity;
    public Unit targetUnit;

    public bool readyToGrow;

    public bool waitingForOrders;

    public UIManager(GraphicManager graphicManager, Game game, Layout layout)
    {
        this.game = game;
        this.layout = layout;
        this.graphicManager = graphicManager;
        screenUI = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/gameui.tscn").Instantiate<Control>();

        goldLabel = screenUI.GetNode<Label>("VBoxContainer/PanelContainer/TopBar/Resources/GoldLabel");
        goldPerTurnLabel = screenUI.GetNode<Label>("VBoxContainer/PanelContainer/TopBar/Resources/GoldPerTurnLabel");
        sciencePerTurnLabel = screenUI.GetNode<Label>("VBoxContainer/PanelContainer/TopBar/Resources/SciencePerTurnLabel");
        culturePerTurnLabel = screenUI.GetNode<Label>("VBoxContainer/PanelContainer/TopBar/Resources/CulturePerTurnLabel");
        happinessLabel = screenUI.GetNode<Label>("VBoxContainer/PanelContainer/TopBar/Resources/HappinessLabel");
        happinessPerTurnLabel = screenUI.GetNode<Label>("VBoxContainer/PanelContainer/TopBar/Resources/HappinessPerTurnLabel");
        influenceLabel = screenUI.GetNode<Label>("VBoxContainer/PanelContainer/TopBar/Resources/InfluenceLabel");
        influencePerTurnLabel = screenUI.GetNode<Label>("VBoxContainer/PanelContainer/TopBar/Resources/InfluencePerTurnLabel");
        turnNumberLabel = screenUI.GetNode<Label>("VBoxContainer/PanelContainer/TopBar/GameInfo/TurnLabel");

        scienceButton = screenUI.GetNode<Button>("VBoxContainer/HBoxContainer/ScienceTree");
        cultureButton = screenUI.GetNode<Button>("VBoxContainer/HBoxContainer/CultureTree");

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
        
        unitInfoPanel = new UnitInfoPanel(graphicManager, game);
        unitInfoPanel.Name = "UnitInfoPanel";
        AddChild(unitInfoPanel);
        unitInfoPanel.Visible = false;

        cityInfoPanel = new CityInfoPanel(graphicManager, game);
        cityInfoPanel.Name = "CityInfoPanel";
        AddChild(cityInfoPanel);
        cityInfoPanel.cityInfoPanel.Visible = false;

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
        goldLabel.Text = game.playerDictionary[game.localPlayerTeamNum].GetGoldTotal().ToString() + " ";
        goldPerTurnLabel.Text = "(+" + game.playerDictionary[game.localPlayerTeamNum].GetGoldPerTurn().ToString() + ")  ";
        sciencePerTurnLabel.Text = " +" + game.playerDictionary[game.localPlayerTeamNum].GetSciencePerTurn().ToString() + "  ";
        culturePerTurnLabel.Text = " +" + game.playerDictionary[game.localPlayerTeamNum].GetCulturePerTurn().ToString() + "  ";
        happinessLabel.Text = game.playerDictionary[game.localPlayerTeamNum].GetHappinessTotal().ToString() + " ";
        happinessPerTurnLabel.Text = "(+" + game.playerDictionary[game.localPlayerTeamNum].GetHappinessPerTurn().ToString() + ")  ";
        influenceLabel.Text = game.playerDictionary[game.localPlayerTeamNum].GetInfluenceTotal().ToString() + " ";
        influencePerTurnLabel.Text = "(+" + game.playerDictionary[game.localPlayerTeamNum].GetInfluencePerTurn().ToString() + ")  ";
        turnNumberLabel.Text = " " + game.turnManager.currentTurn;

        UpdateUnitUIDisplay();
        UpdateEndTurnButton();
    }

    public void Update(UIElement element)
    {
        if (element == UIElement.gold)
        {
            goldLabel.Text = game.playerDictionary[game.localPlayerTeamNum].GetGoldTotal().ToString() + " ";
        }
        else if (element == UIElement.goldPerTurn)
        {
            goldPerTurnLabel.Text = "(+" + game.playerDictionary[game.localPlayerTeamNum].GetGoldPerTurn().ToString() + ")  ";
        }
        else if (element == UIElement.sciencePerTurn)
        {
            sciencePerTurnLabel.Text = " +" + game.playerDictionary[game.localPlayerTeamNum].GetSciencePerTurn().ToString() + "  ";
        }
        else if (element == UIElement.culturePerTurn)
        {
            culturePerTurnLabel.Text = " +" + game.playerDictionary[game.localPlayerTeamNum].GetCulturePerTurn().ToString() + "  ";
        }
        else if (element == UIElement.happiness)
        {
            happinessLabel.Text = game.playerDictionary[game.localPlayerTeamNum].GetHappinessTotal().ToString() + " ";
        }
        else if (element == UIElement.happinessPerTurn)
        {
            happinessPerTurnLabel.Text = "(+" + game.playerDictionary[game.localPlayerTeamNum].GetHappinessPerTurn().ToString() + ")  ";
        }
        else if (element == UIElement.influence)
        {
            influenceLabel.Text = game.playerDictionary[game.localPlayerTeamNum].GetInfluenceTotal().ToString() + " ";
        }
        else if (element == UIElement.influencePerTurn)
        {
            influencePerTurnLabel.Text = "(+" + game.playerDictionary[game.localPlayerTeamNum].GetInfluencePerTurn().ToString() + ")  ";
        }
        else if (element == UIElement.turnNumber)
        {
            turnNumberLabel.Text = " " + game.turnManager.currentTurn;
        }
        else if (element == UIElement.unitDisplay)
        {
            UpdateUnitUIDisplay();
        }
        else if (element == UIElement.endTurnButton)
        {
            UpdateEndTurnButton();
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
        if (readyToGrow)
        {
            ((GraphicCity)graphicManager.graphicObjectDictionary[targetCity.id]).GenerateGrowthTargetingPrompt();
        }
        else if(waitingForOrders)
        {
            //move camera to foundUnit.hex
            graphicManager.ChangeSelectedObject(targetUnit.id , graphicManager.graphicObjectDictionary[targetUnit.id]);
        }
        else
        {
            game.turnManager.EndCurrentTurn(game.localPlayerTeamNum);
        }
    }
    
    private void UpdateEndTurnButton()
    {
        bool cityReadyToGrow = false;
        City foundCity = null;
        foreach(int cityID in game.playerDictionary[game.localPlayerTeamNum].cityList)
        {
            City city = Global.gameManager.game.cityDictionary[cityID];
            if (city.readyToExpand > 0)
            {
                foundCity = city;
                cityReadyToGrow = true;
                break;
            }
        }
        if (cityReadyToGrow)
        {
            endTurnButton.Icon = Godot.ResourceLoader.Load<Texture2D>("res://graphics/ui/icons/house.png");
            readyToGrow = true;
            targetCity = foundCity;
            waitingForOrders = false;
            return;
        }
        bool unitNeedsOrders = false;
        Unit foundUnit = null;
        foreach (int unitID in game.playerDictionary[game.localPlayerTeamNum].unitList)
        {
            Unit unit = Global.gameManager.game.unitDictionary[unitID];
            if (unit.remainingMovement > 0 && unit.currentPath.Count == 0)
            {
                foundUnit = unit;
                unitNeedsOrders = true;
                break;
            }
        }
        if (unitNeedsOrders)
        {
            endTurnButton.Icon = Godot.ResourceLoader.Load<Texture2D>("res://graphics/ui/icons/moveicon.png");
            readyToGrow = false;
            targetCity = null;
            waitingForOrders = true;
            targetUnit = foundUnit;
            return;
        }

        endTurnButton.Icon = Godot.ResourceLoader.Load<Texture2D>("res://graphics/ui/icons/skipturn.png");
        readyToGrow = false;
        targetCity = null;
        waitingForOrders = false;
    }

    public void ScienceTreeButtonPressed()
    {

    }

    public void CultureTreeButtonPressed()
    {

    }
}

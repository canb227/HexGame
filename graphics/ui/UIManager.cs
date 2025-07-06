using Godot;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static NetworkPeer;

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
    researchTree,
    resourcePanel
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

    public Button menuButton;

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

    public Button resourceButton;

    public Button tradeExportButton;

    public HBoxContainer playerList;

    public UnitInfoPanel unitInfoPanel;
    public CityInfoPanel cityInfoPanel;
    public ResearchTreePanel researchTreePanel;
    public ResearchTreePanel cultureResearchTreePanel;

    public ResourcePanel resourcePanel;

    public TradeExportPanel tradeExportPanel;

    public TradeRoutePickerPanel tradeRoutePickerPanel;

    public DiplomacyPanel diplomacyPanel;

    public VBoxContainer actionQueue;

    public PanelContainer topBarPanel;
    public Label goldenAgeLabel;

    public City targetCity;
    public Unit targetUnit;

    public bool pickScience;
    public bool pickCulture;

    public bool assignResource;

    public bool readyToGrow;
    public bool cityNeedsProduction;

    public bool waitingForOrders = true;

    public bool windowOpen = false;

    public UIManager(Layout layout)
    {
        this.layout = layout;
        screenUI = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/gameui.tscn").Instantiate<Control>();

        goldLabel = screenUI.GetNode<Label>("LayerHelper/PanelContainer/TopBar/Resources/GoldLabel");
        goldPerTurnLabel = screenUI.GetNode<Label>("LayerHelper/PanelContainer/TopBar/Resources/GoldPerTurnLabel");
        sciencePerTurnLabel = screenUI.GetNode<Label>("LayerHelper/PanelContainer/TopBar/Resources/SciencePerTurnLabel");
        culturePerTurnLabel = screenUI.GetNode<Label>("LayerHelper/PanelContainer/TopBar/Resources/CulturePerTurnLabel");
        happinessLabel = screenUI.GetNode<Label>("LayerHelper/PanelContainer/TopBar/Resources/HappinessLabel");
        happinessPerTurnLabel = screenUI.GetNode<Label>("LayerHelper/PanelContainer/TopBar/Resources/HappinessPerTurnLabel");
        influenceLabel = screenUI.GetNode<Label>("LayerHelper/PanelContainer/TopBar/Resources/InfluenceLabel");
        influencePerTurnLabel = screenUI.GetNode<Label>("LayerHelper/PanelContainer/TopBar/Resources/InfluencePerTurnLabel");
        turnNumberLabel = screenUI.GetNode<Label>("LayerHelper/PanelContainer/TopBar/GameInfo/TurnLabel");
        topBarPanel = screenUI.GetNode<PanelContainer>("LayerHelper/PanelContainer");
        goldenAgeLabel = screenUI.GetNode<Label>("LayerHelper/PanelContainer/TopBar/Resources/HappinessNeededForGoldenAge");

        menuButton = screenUI.GetNode<Button>("LayerHelper/PanelContainer/TopBar/GameInfo/MenuButton");

        menuButton.Pressed += () => Global.menuManager.ChangeMenu(MenuManager.UI_Pause);

        scienceButton = screenUI.GetNode<Button>("LayerHelper/ScienceTree");
        scienceButtonLabel = scienceButton.GetNode<Label>("ResearchLabel");
        scienceButtonIcon = scienceButton.GetNode<TextureRect>("ScienceTreeIcon");
        scienceButtonResults = scienceButton.GetNode<HBoxContainer>("ResearchResultBox");
        scienceButtonTurnsLeft = scienceButton.GetNode<Label>("TurnsLeft");

        cultureButton = screenUI.GetNode<Button>("LayerHelper/CultureTree");
        cultureButtonLabel = cultureButton.GetNode<Label>("ResearchLabel");
        cultureButtonIcon = cultureButton.GetNode<TextureRect>("CultureTreeIcon");
        cultureButtonResults = cultureButton.GetNode<HBoxContainer>("ResearchResultBox");
        cultureButtonTurnsLeft = cultureButton.GetNode<Label>("TurnsLeft");

        scienceButton.Pressed += () => ScienceTreeButtonPressed();
        cultureButton.Pressed += () => CultureTreeButtonPressed();

        resourceButton = screenUI.GetNode<Button>("LayerHelper/ResourcePanel");
        resourceButton.Pressed += () => ResourcePanelButtonPressed();

        tradeExportButton = screenUI.GetNode<Button>("LayerHelper/TradeExportButton");
        tradeExportButton.Pressed += () => TradeExportPanelButtonPressed();

        actionQueue = screenUI.GetNode<VBoxContainer>("ActionQueueScrollBox/ActionQueue");

        playerList = screenUI.GetNode<HBoxContainer>("PlayerList");
        foreach(Player player in Global.gameManager.game.playerDictionary.Values)
        {
            if(player.teamNum != 0)
            {
                Button icon = new();
                if (player.isAI)
                {
                    icon.Icon = GD.Load<CompressedTexture2D>("res://graphics/ui/icons/blankperson.png");
                }
                else
                {
                    icon.Icon = Global.GetMediumSteamAvatar(Global.gameManager.teamNumToPlayerID[player.teamNum]);
                }
                icon.Pressed += () => DiplomacyButtonPressed(player.teamNum, null);
                playerList.AddChild(icon);
            }
        }

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

        resourcePanel = new ResourcePanel();
        resourcePanel.Name = "ResourcePanel";
        resourcePanel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        AddChild(resourcePanel);
        resourcePanel.Visible = false;

        tradeExportPanel = new TradeExportPanel();
        tradeExportPanel.Name = "TradeExportPanel";
        tradeExportPanel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        AddChild(tradeExportPanel);
        tradeExportPanel.Visible = false;

        tradeRoutePickerPanel = new TradeRoutePickerPanel();
        tradeRoutePickerPanel.Name = "TradeRoutePickerPanel";
        tradeRoutePickerPanel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        AddChild(tradeRoutePickerPanel);
        tradeRoutePickerPanel.Visible = false;

        diplomacyPanel = new DiplomacyPanel();
        diplomacyPanel.Name = "DiplomacyPanel";
        diplomacyPanel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        AddChild(diplomacyPanel);
        diplomacyPanel.Visible = false;


        UpdateAll();
        AddChild(screenUI);
    }

    private void SetupTurnUI()
    {
        endTurnButton = screenUI.GetNode<Button>("LayerHelper/EndTurnButton");
        endTurnButton.Pressed += endTurnButtonPressed;
    }

    public void HideGenericUIForTargeting()
    {
        endTurnButton.Visible = false;
        HideGenericUI();
    }

    public void ShowGenericUIAfterTargeting()
    {
        endTurnButton.Visible = true;
        ShowGenericUI();
    }

    public void UpdateAll()
    {
        goldLabel.Text = Math.Round(Global.gameManager.game.localPlayerRef.GetGoldTotal()).ToString() + " ";
        goldPerTurnLabel.Text = "(+" + Math.Round(Global.gameManager.game.localPlayerRef.GetGoldPerTurn()).ToString() + ")  ";
        sciencePerTurnLabel.Text = " +" + Math.Round(Global.gameManager.game.localPlayerRef.GetSciencePerTurn()).ToString() + "  ";
        culturePerTurnLabel.Text = " +" + Math.Round(Global.gameManager.game.localPlayerRef.GetCulturePerTurn()).ToString() + "  ";
        happinessLabel.Text = Math.Round(Global.gameManager.game.localPlayerRef.GetHappinessTotal()).ToString() + " ";
        happinessPerTurnLabel.Text = "(+" + Math.Round(Global.gameManager.game.localPlayerRef.GetHappinessPerTurn()).ToString() + ")";
        goldenAgeLabel.Text = "/" + Math.Round(Global.gameManager.game.localPlayerRef.administrativeUpkeep).ToString() + "  ";
        influenceLabel.Text = Math.Round(Global.gameManager.game.localPlayerRef.GetInfluenceTotal()).ToString() + " ";
        influencePerTurnLabel.Text = "(+" + Math.Round(Global.gameManager.game.localPlayerRef.GetInfluencePerTurn()).ToString() + ")  ";
        turnNumberLabel.Text = " " + Global.gameManager.game.turnManager.currentTurn;

        UpdateUnitUIDisplay();
        UpdateEndTurnButton();
        researchTreePanel.UpdateResearchUI();
        cultureResearchTreePanel.UpdateResearchUI();
        resourcePanel.UpdateResourcePanel();
        UpdateResearchUI();
    }

    public void Update(UIElement element)
    {
        if (element == UIElement.gold)
        {
            goldLabel.Text = Math.Round(Global.gameManager.game.localPlayerRef.GetGoldTotal()).ToString() + " ";
        }
        else if (element == UIElement.goldPerTurn)
        {
            goldPerTurnLabel.Text = "(+" + Math.Round(Global.gameManager.game.localPlayerRef.GetGoldPerTurn()).ToString() + ")  ";
        }
        else if (element == UIElement.sciencePerTurn)
        {
            sciencePerTurnLabel.Text = " +" + Math.Round(Global.gameManager.game.localPlayerRef.GetSciencePerTurn()).ToString() + "  ";
        }
        else if (element == UIElement.culturePerTurn)
        {
            culturePerTurnLabel.Text = " +" + Math.Round(Global.gameManager.game.localPlayerRef.GetCulturePerTurn()).ToString() + "  ";
        }
        else if (element == UIElement.happiness)
        {
            happinessLabel.Text = Math.Round(Global.gameManager.game.localPlayerRef.GetHappinessTotal()).ToString() + " ";
        }
        else if (element == UIElement.happinessPerTurn)
        {
            happinessPerTurnLabel.Text = "(+" + Math.Round(Global.gameManager.game.localPlayerRef.GetHappinessPerTurn()).ToString() + ")";
        }
        else if (element == UIElement.influence)
        {
            influenceLabel.Text = Math.Round(Global.gameManager.game.localPlayerRef.GetInfluenceTotal()).ToString() + " ";
        }
        else if (element == UIElement.influencePerTurn)
        {
            influencePerTurnLabel.Text = "(+" + Math.Round(Global.gameManager.game.localPlayerRef.GetInfluencePerTurn()).ToString() + ")  ";
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
        else if(element == UIElement.resourcePanel)
        {
            resourcePanel.UpdateResourcePanel();
        }
    }

    public void SetTopBarColor(Godot.Color color)
    {
        StyleBoxFlat styleBox = new StyleBoxFlat();
        styleBox.BgColor = color;
        topBarPanel.AddThemeStyleboxOverride("panel", styleBox);
    }

    public void UpdateResearchUI()
    {
        Player localPlayer = Global.gameManager.game.localPlayerRef;

        if (localPlayer.queuedResearch.Any())
        {
            GD.Print(localPlayer.queuedResearch[0].researchType);
            ResearchInfo info = ResearchLoader.researchesDict[localPlayer.queuedResearch.First().researchType];
            scienceButtonLabel.Text = Global.gameManager.game.localPlayerRef.queuedResearch.First().researchType;
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
            scienceButtonTurnsLeft.Text = (Math.Ceiling(localPlayer.queuedResearch[0].researchLeft / (localPlayer.GetSciencePerTurn() + localPlayer.GetScienceTotal()))).ToString();
        }
        else
        {
            scienceButtonLabel.Text = "Select a Research";
            scienceButtonIcon.Texture = Godot.ResourceLoader.Load<Texture2D>("res://graphics/ui/icons/science.png");
            scienceButtonTurnsLeft.Text = "-";
            foreach (Node child in scienceButtonResults.GetChildren())
            {
                child.QueueFree();
            }
        }


        if (localPlayer.queuedCultureResearch.Any())
        {
            ResearchInfo info = CultureResearchLoader.researchesDict[localPlayer.queuedCultureResearch.First().researchType];
            cultureButtonLabel.Text = localPlayer.queuedCultureResearch.First().researchType;
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
            cultureButtonTurnsLeft.Text = Math.Ceiling(localPlayer.queuedCultureResearch[0].researchLeft / (localPlayer.GetCulturePerTurn() + localPlayer.GetCultureTotal())).ToString();
        }
        else
        {
            cultureButtonLabel.Text = "Select a Research";
            cultureButtonIcon.Texture = Godot.ResourceLoader.Load<Texture2D>("res://graphics/ui/icons/culture.png");
            cultureButtonTurnsLeft.Text = "-";
            foreach (Node child in cultureButtonResults.GetChildren())
            {
                child.QueueFree();
            }
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

    public void CloseCurrentWindow()
    {
        windowOpen = false;
        researchTreePanel.Visible = false;
        cultureResearchTreePanel.Visible = false;
        resourcePanel.Visible = false;
        tradeExportPanel.Visible = false;
        tradeRoutePickerPanel.Visible = false;
        diplomacyPanel.Visible = false;
        ShowGenericUI();
    }
    private void endTurnButtonPressed()
    {
        windowOpen = false;
        researchTreePanel.Visible = false;
        cultureResearchTreePanel.Visible = false;
        resourcePanel.Visible = false;
        ShowGenericUI();
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
        if (assignResource)
        {
            ResourcePanelButtonPressed();
            return;
        }
        if (readyToGrow)
        {
            researchTreePanel.Visible = false;
            cultureResearchTreePanel.Visible = false;
            resourcePanel.Visible = false;
            HideGenericUI();
            ((GraphicCity)Global.gameManager.graphicManager.graphicObjectDictionary[targetCity.id]).GenerateGrowthTargetingPrompt();
            Global.camera.SetHexTarget(targetCity.hex);
            return;
        }
        else if(cityNeedsProduction)
        {
            researchTreePanel.Visible = false;
            cultureResearchTreePanel.Visible = false;
            resourcePanel.Visible = false;
            HideGenericUI();
            Global.gameManager.graphicManager.ChangeSelectedObject(targetCity.id, (GraphicCity)Global.gameManager.graphicManager.graphicObjectDictionary[targetCity.id]);
            Global.camera.SetHexTarget(targetCity.hex);
            return;
        }
        else if(waitingForOrders)
        {
            Global.gameManager.graphicManager.ChangeSelectedObject(targetUnit.id , Global.gameManager.graphicManager.graphicObjectDictionary[targetUnit.id]);
            Global.camera.SetHexTarget(targetUnit.hex);
            return;
        }
        else
        {
            Global.gameManager.EndTurn(Global.gameManager.game.localPlayerTeamNum);
            CloseCurrentWindow();
            //Global.gameManager.game.turnManager.EndCurrentTurn(Global.gameManager.game.localPlayerTeamNum);
            return;
        }
    }
    
    private void UpdateEndTurnButton()
    {
        if(Global.gameManager.game.localPlayerRef.queuedResearch.Count == 0)
        {
            pickScience = true;
            endTurnButton.Icon = Godot.ResourceLoader.Load<Texture2D>("res://graphics/ui/icons/science.png");
            return;
        }
        else
        {
            pickScience = false;
        }

        if (Global.gameManager.game.localPlayerRef.queuedCultureResearch.Count == 0)
        {
            pickCulture = true;
            endTurnButton.Icon = Godot.ResourceLoader.Load<Texture2D>("res://graphics/ui/icons/culture.png");
            return;
        }
        else
        {
            pickCulture = false;
        }

        if (assignResource)
        {
            endTurnButton.Icon = Godot.ResourceLoader.Load<Texture2D>("res://graphics/ui/icons/star.png");
            readyToGrow = false;
            cityNeedsProduction = false;
            targetCity = null;
            waitingForOrders = false;
            return;
        }

        bool cityReadyToGrow = false;
        City foundCity = null;
        foreach(int cityID in Global.gameManager.game.localPlayerRef.cityList)
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
        foreach (int unitID in Global.gameManager.game.localPlayerRef.unitList)
        {
            Unit unit = Global.gameManager.game.unitDictionary[unitID];
            if (unit.remainingMovement > 0 && unit.currentPath.Count == 0 && !unit.isSleeping && !unit.isSkipping)
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
        Global.gameManager.graphicManager.UnselectObject();
        windowOpen = true;
        researchTreePanel.Visible = true;
        HideGenericUI();
        var timer = new Timer();
        timer.WaitTime = 0.01; // Delay for 0.1 seconds (adjust as needed)
        timer.OneShot = true;
        AddChild(timer);
        timer.Start();

        timer.Timeout += () => researchTreePanel.AddLines();
    }

    public void CultureTreeButtonPressed()
    {
        Global.gameManager.graphicManager.UnselectObject();
        windowOpen = true;
        cultureResearchTreePanel.Visible = true;
        HideGenericUI();
        var timer = new Timer();
        timer.WaitTime = 0.01; // Delay for 0.1 seconds (adjust as needed)
        timer.OneShot = true;
        AddChild(timer);
        timer.Start();

        timer.Timeout += () => cultureResearchTreePanel.AddLines();
    }

    public void ResourcePanelButtonPressed()
    {
        Global.gameManager.graphicManager.UnselectObject();
        windowOpen = true;
        assignResource = false;
        resourcePanel.UpdateResourcePanel();
        resourcePanel.Visible = true;
        HideGenericUI();
        Global.gameManager.graphicManager.uiManager.Update(UIElement.endTurnButton);
    }

    public void TradeExportPanelButtonPressed()
    {
        Global.gameManager.graphicManager.UnselectObject();
        windowOpen = true;
        tradeExportPanel.Visible = true;
        tradeExportPanel.UpdateTradeExportPanel();
        HideGenericUI();
    }

    public void DiplomacyActionButtonPressed(Button dealButton, int targetTeamNum, DiplomacyDeal deal)
    {
        if (Global.gameManager.game.localPlayerRef.turnFinished)
        {
            return;
        }
        dealButton.QueueFree();
        DiplomacyButtonPressed(targetTeamNum, deal);
    }

    public void DiplomacyButtonPressed(int targetTeamNum, DiplomacyDeal deal)
    {
        if (Global.gameManager.game.localPlayerRef.turnFinished)
        {
            return;
        }
        if (deal == null && Global.gameManager.game.teamManager.pendingDeals.Any())
        {
            foreach(DiplomacyDeal pendingDeal in Global.gameManager.game.teamManager.pendingDeals)
            {
                if(pendingDeal.sendingTeamNum == targetTeamNum && pendingDeal.receivingTeamNum == Global.gameManager.game.localPlayerTeamNum)
                {
                    deal = pendingDeal;
                    break;
                }
            }
        }
        Global.gameManager.graphicManager.UnselectObject();
        windowOpen = true;
        diplomacyPanel.Visible = true;
        diplomacyPanel.UpdateDiplomacyPanel(targetTeamNum, deal);
        HideGenericUI();
    }

    public void OpenTradeMenu(Unit unit)
    {
        windowOpen = true;
        tradeRoutePickerPanel.Visible = true;
        tradeRoutePickerPanel.UpdateTradeRoutePickerPanel(unit);
        HideGenericUI();
    }

    public void HideGenericUI()
    {
        scienceButton.Visible = false;
        cultureButton.Visible = false;
        resourceButton.Visible = false;
        tradeExportButton.Visible = false;
        playerList.Visible = false;
    }
    public void ShowGenericUI()
    {
        scienceButton.Visible = true;
        cultureButton.Visible = true;
        resourceButton.Visible = true;
        tradeExportButton.Visible = true;
        playerList.Visible = true;
    }

    public void NewDiplomaticDeal(DiplomacyDeal deal)
    {
        Button dealButton = new Button();
        dealButton.Name = deal.sendingTeamNum.ToString();
        dealButton.CustomMinimumSize = new Vector2(64, 64);
        dealButton.ExpandIcon = true;
        dealButton.Icon = Godot.ResourceLoader.Load<Texture2D>("res://graphics/ui/icons/diplomacy.png");
        dealButton.Pressed += () => DiplomacyActionButtonPressed(dealButton, deal.sendingTeamNum, deal);
        actionQueue.AddChild(dealButton);
        actionQueue.MoveChild(dealButton, 0);
    }

    public void RemoveDiplomaticDeal(DiplomacyDeal deal)
    {
        GD.Print("Check against:" + deal.sendingTeamNum.ToString());
        foreach (Node child in actionQueue.GetChildren())
        {
            GD.Print("WE are:" + child.Name.ToString());
            if (child.Name.ToString().Contains(deal.sendingTeamNum.ToString()))
            {
                child.QueueFree();
                break;
            }
        }
    }
}

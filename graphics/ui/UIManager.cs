using Godot;
using Microsoft.Win32.SafeHandles;
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

    public Button governmentButton;

    public HBoxContainer playerList;

    public Label expandBuildingLabel;

    public UnitInfoPanel unitInfoPanel;
    public HeroInfoPanel heroInfoPanel;
    public CityInfoPanel cityInfoPanel;
    public ResearchTreePanel researchTreePanel;
    public ResearchTreePanel cultureResearchTreePanel;

    public ResourcePanel resourcePanel;

    public TradeExportPanel tradeExportPanel;

    public PolicyPanel policyPanel;

    public TradeRoutePickerPanel tradeRoutePickerPanel;

    public DiplomacyPanel diplomacyPanel;

    public EncampementTakenPopUp encampementTakenPopUp;

    public CityTakenPopUp cityTakenPopUp;

    public EventSelectionPanel eventSelectionPanel;

    public VBoxContainer heroContainer;
    public Button heroButton;
    public TextureProgressBar heroRespawnBar;
    public Label heroRespawnLabel;
    public ProgressBar healthBar;
    public ProgressBar manaBar;
    public Label totalHealthLabel;
    public Label totalManaLabel;

    public HBoxContainer resourcesContainer;

    public PanelContainer combatPreviewPanel;
    public Label yourName;
    public Label yourStrengthLabel;
    public VBoxContainer yourCombatStrengthEffectBox;
    public ColorRect yourLifeBackground;
    public ColorRect yourDamageTaken;
    public ColorRect yourLifeRemaining;
    public Label theirName;
    public Label theirStrengthLabel;
    public VBoxContainer theirCombatStrengthEffectBox;
    public ColorRect theirLifeBackground;
    public ColorRect theirDamageTaken;
    public ColorRect theirLifeRemaining;
    public Label battleResultLabel;

    public bool combatPreviewVisible;


    public VBoxContainer actionQueue;

    public PanelContainer topBarPanel;
    public Label goldenAgeLabel;
    public PanelContainer waitingOnYouPanel;
    public bool waitingOnLocalPlayer;

    public City targetCity;
    public Unit targetUnit;

    public bool pickScience;
    public bool pickCulture;

    public bool assignResource;
    public bool assignGovernment;

    public bool readyToGrow;
    public bool readyToUrbanize;
    public bool cityNeedsProduction;

    public bool waitingForOrders = true;

    public bool windowOpen = false;
    public bool pauseMenuOpen = false;

    public UIManager(Layout layout)
    {
        this.layout = layout;
        screenUI = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/gameui.tscn").Instantiate<Control>();

        goldLabel = screenUI.GetNode<Label>("LayerHelper/PanelContainer/TopBar/Yields/GoldLabel");
        goldPerTurnLabel = screenUI.GetNode<Label>("LayerHelper/PanelContainer/TopBar/Yields/GoldPerTurnLabel");
        sciencePerTurnLabel = screenUI.GetNode<Label>("LayerHelper/PanelContainer/TopBar/Yields/SciencePerTurnLabel");
        culturePerTurnLabel = screenUI.GetNode<Label>("LayerHelper/PanelContainer/TopBar/Yields/CulturePerTurnLabel");
        happinessLabel = screenUI.GetNode<Label>("LayerHelper/PanelContainer/TopBar/Yields/HappinessLabel");
        happinessPerTurnLabel = screenUI.GetNode<Label>("LayerHelper/PanelContainer/TopBar/Yields/HappinessPerTurnLabel");
        influenceLabel = screenUI.GetNode<Label>("LayerHelper/PanelContainer/TopBar/Yields/InfluenceLabel");
        influencePerTurnLabel = screenUI.GetNode<Label>("LayerHelper/PanelContainer/TopBar/Yields/InfluencePerTurnLabel");
        turnNumberLabel = screenUI.GetNode<Label>("LayerHelper/PanelContainer/TopBar/GameInfo/TurnLabel");
        topBarPanel = screenUI.GetNode<PanelContainer>("LayerHelper/PanelContainer");
        goldenAgeLabel = screenUI.GetNode<Label>("LayerHelper/PanelContainer/TopBar/Yields/HappinessNeededForGoldenAge");

        resourcesContainer = screenUI.GetNode<HBoxContainer>("LayerHelper/PanelContainer/TopBar/Resources");

        menuButton = screenUI.GetNode<Button>("LayerHelper/PanelContainer/TopBar/GameInfo/MenuButton");

        menuButton.Pressed += () => ChangeMenuManagerMenu(MenuManager.UI_Pause);

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

        governmentButton = screenUI.GetNode<Button>("LayerHelper/GovernmentButton");
        governmentButton.Pressed += () => GovernmentButtonPressed();

        actionQueue = screenUI.GetNode<VBoxContainer>("ActionQueueScrollBox/ActionQueue");

        expandBuildingLabel = screenUI.GetNode<Label>("LayerHelper/ExpandBuildingLabel");
        expandBuildingLabel.Visible = false;

        waitingOnYouPanel = screenUI.GetNode<PanelContainer>("LayerHelper/EndTurnButton/WaitingOnPlayerPanel");
        waitingOnYouPanel.Visible = false;


        heroContainer = screenUI.GetNode<VBoxContainer>("HeroContainer");
        heroButton = heroContainer.GetNode<Button>("SelectHeroButton");
        heroButton.Pressed += () => SelectHero();
        heroRespawnBar = heroButton.GetNode<TextureProgressBar>("Cooldown");
        heroRespawnLabel = heroButton.GetNode<Label>("CooldownLabel");
        healthBar = heroContainer.GetNode<ProgressBar>("HealthBar");
        manaBar = heroContainer.GetNode<ProgressBar>("ManaBar");
        totalHealthLabel = heroContainer.GetNode<Label>("HealthBar/TotalHealth");
        totalManaLabel = heroContainer.GetNode<Label>("ManaBar/TotalMana");
        heroContainer.Visible = false;

        combatPreviewPanel = screenUI.GetNode<PanelContainer>("CombatPreviewPanel");
        combatPreviewPanel.Visible = false;
        yourName = combatPreviewPanel.GetNode<Label>("VBoxContainer/HBoxContainer/VBoxContainer/YourUnitName");
        theirName = combatPreviewPanel.GetNode<Label>("VBoxContainer/HBoxContainer/VBoxContainer2/TheirName");
        yourStrengthLabel = combatPreviewPanel.GetNode<Label>("VBoxContainer/HBoxContainer/VBoxContainer/YourStrengthLabel");
        yourCombatStrengthEffectBox = combatPreviewPanel.GetNode<VBoxContainer>("VBoxContainer/HBoxContainer/VBoxContainer/YourCombatStrengthEffectBox");
        yourLifeBackground = combatPreviewPanel.GetNode<ColorRect>("VBoxContainer/HBoxContainer/YourHealthBar/LifeBackground");
        yourDamageTaken = combatPreviewPanel.GetNode<ColorRect>("VBoxContainer/HBoxContainer/YourHealthBar/DamageTaken");
        yourLifeRemaining = combatPreviewPanel.GetNode<ColorRect>("VBoxContainer/HBoxContainer/YourHealthBar/RemainingHealth");
        theirStrengthLabel = combatPreviewPanel.GetNode<Label>("VBoxContainer/HBoxContainer/VBoxContainer2/TheirStrengthLabel");
        theirCombatStrengthEffectBox = combatPreviewPanel.GetNode<VBoxContainer>("VBoxContainer/HBoxContainer/VBoxContainer2/TheirCombatStrengthEffectBox");
        theirLifeBackground = combatPreviewPanel.GetNode<ColorRect>("VBoxContainer/HBoxContainer/TheirLifeBar/LifeBackground");
        theirDamageTaken = combatPreviewPanel.GetNode<ColorRect>("VBoxContainer/HBoxContainer/TheirLifeBar/DamageTaken");
        theirLifeRemaining = combatPreviewPanel.GetNode<ColorRect>("VBoxContainer/HBoxContainer/TheirLifeBar/RemainingHealth");
        battleResultLabel = combatPreviewPanel.GetNode<Label>("VBoxContainer/BattleResultLabel");

        playerList = screenUI.GetNode<HBoxContainer>("PlayerList");
        foreach (Player player in Global.gameManager.game.playerDictionary.Values)
        {
            if (player.teamNum != 0 && !FactionLoader.IsFactionMinor(player.faction))
            {
                Button icon = new();
                if (player.isAI)
                {
                    icon.Icon = GD.Load<CompressedTexture2D>("res://graphics/ui/icons/blankperson.png");
                }
                else
                {
                    icon.Icon = Global.GetMediumSteamAvatar(Global.gameManager.game.teamNumToPlayerID[player.teamNum]);
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

        heroInfoPanel = new HeroInfoPanel(healthBar, manaBar, totalHealthLabel, totalManaLabel);
        heroInfoPanel.Name = "HeroInfoPanel";
        AddChild(heroInfoPanel);
        heroInfoPanel.Visible = false;

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

        policyPanel = new PolicyPanel();
        policyPanel.Name = "PolicyPanel";
        policyPanel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        AddChild(policyPanel);
        policyPanel.Visible = false;

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


        encampementTakenPopUp = new EncampementTakenPopUp();
        encampementTakenPopUp.Name = "EncampementTakenPopUp";
        encampementTakenPopUp.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        AddChild(encampementTakenPopUp);
        encampementTakenPopUp.Visible = false;


        cityTakenPopUp = new CityTakenPopUp();
        cityTakenPopUp.Name = "CityTakenPopUp";
        cityTakenPopUp.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        AddChild(cityTakenPopUp);
        cityTakenPopUp.Visible = false;

        eventSelectionPanel = new EventSelectionPanel();
        eventSelectionPanel.Name = "EventSelectionPanel";
        eventSelectionPanel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        AddChild(eventSelectionPanel);
        eventSelectionPanel.Visible = false;


        UpdateAll();
        AddChild(screenUI);
    }

    public void SelectHero()
    {
        if (Global.gameManager.graphicManager.GetWaitForTargeting())
        {
            Global.gameManager.graphicManager.ClearWaitForTarget();
        }
        Global.camera.SetHexTarget(Global.gameManager.game.unitDictionary[Global.gameManager.game.localPlayerRef.ourHeroID].hex);
        Global.gameManager.graphicManager.ChangeSelectedObject(Global.gameManager.game.localPlayerRef.ourHeroID, Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.localPlayerRef.ourHeroID]);
    }

    public void ChangeMenuManagerMenu(string menu)
    {
        CloseCurrentWindow();
        Global.menuManager.ChangeMenu(menu);
        pauseMenuOpen = true;
    }

    private void SetupTurnUI()
    {
        endTurnButton = screenUI.GetNode<Button>("LayerHelper/EndTurnButton");
        endTurnButton.Pressed += endTurnButtonPressed;
    }

    public void HideGenericUIForTargeting()
    {
        endTurnButton.Visible = false;
        waitingOnYouPanel.Visible = false;
        HideGenericUI();
    }

    public void ShowGenericUIAfterTargeting()
    {
        endTurnButton.Visible = true;
        if (waitingOnLocalPlayer)
        {
            waitingOnYouPanel.Visible = true;
        }
        ShowGenericUI();
    }

    public void ShowAndUpdateCombatPreview(GraphicUnit graphicUnit, Unit targetUnit, District targetDistrict, int bonusCombatStrength=0, UnitAbility ability=null, int level=0)
    {
        combatPreviewPanel.Visible = true;
        combatPreviewVisible = true;
        yourName.Text = graphicUnit.unit.name;
        if (targetDistrict != null && targetDistrict.health > 0)
        {
            //yours
            yourStrengthLabel.Text = graphicUnit.unit.CalculateCombatStrength(bonusCombatStrength, null, Global.gameManager.game.cityDictionary[targetDistrict.cityID].teamNum).ToString();
            foreach (var child in yourCombatStrengthEffectBox.GetChildren())
            {
                child.Free();
            }
            //
            //anti-cavalry check
            if ((graphicUnit.unit.unitClass & UnitClass.AntiCavalry) != 0 && (graphicUnit.unit.unitClass & UnitClass.Cavalry) != 0)
            {
                Label antiCavalry = new();
                antiCavalry.Text = "+7 vs Cavalry";
                yourCombatStrengthEffectBox.AddChild(antiCavalry);
            }

            //anti-encampment bonus check
            if (Global.gameManager.game.playerDictionary[Global.gameManager.game.cityDictionary[targetDistrict.cityID].teamNum].isEncampment && Global.gameManager.game.playerDictionary[graphicUnit.unit.teamNum].bonusAgainstEncampments > 0)
            {
                Label antiEncampment = new();
                antiEncampment.Text = "+"+ Global.gameManager.game.playerDictionary[graphicUnit.unit.teamNum].bonusAgainstEncampments+" vs Encampments";
                yourCombatStrengthEffectBox.AddChild(antiEncampment);
            }


            yourLifeBackground.CustomMinimumSize = new Vector2(16, 180); //tempUnit.waitingAbility.name
            float damageTaken = graphicUnit.unit.CalculateDamage(graphicUnit.unit.CalculateCombatStrength(bonusCombatStrength, null, Global.gameManager.game.cityDictionary[targetDistrict.cityID].teamNum), targetDistrict.GetCombatStrength(), 1.0f);
            float unitHealthPercentage = (graphicUnit.unit.health) / 100.0f;
            float unitHealthRemainigPercentage = (graphicUnit.unit.health - damageTaken) / 100.0f;
            if (ability != null)
            {
                yourDamageTaken.CustomMinimumSize = new Vector2(16, unitHealthPercentage * 180f);
                yourLifeRemaining.CustomMinimumSize = new Vector2(16, unitHealthPercentage * 180f);
            }
            else
            {
                yourDamageTaken.CustomMinimumSize = new Vector2(16, unitHealthPercentage * 180f);
                yourLifeRemaining.CustomMinimumSize = new Vector2(16, unitHealthRemainigPercentage * 180f);
            }

            //theirs
            theirName.Text = Global.gameManager.game.cityDictionary[targetDistrict.cityID].name;
            theirStrengthLabel.Text = targetDistrict.GetCombatStrength().ToString();
            foreach (var child in theirCombatStrengthEffectBox.GetChildren())
            {
                child.Free();
            }
            theirLifeBackground.CustomMinimumSize = new Vector2(16, 180);
            float damageTaken2 = 0.0f;
            if (ability != null && ability.combatPower.Any())
            {
                float abilityPower = ability.combatPower[level];
                if(ability.name == "BombardAttack")
                {
                    Label bombardLabel = new();
                    bombardLabel.Text = "+10 From Bombard Attack";
                    yourCombatStrengthEffectBox.AddChild(bombardLabel);
                    abilityPower += 10;
                }
                yourStrengthLabel.Text = abilityPower.ToString();
                damageTaken2 = graphicUnit.unit.CalculateDamage(targetDistrict.GetCombatStrength(), abilityPower, 1.0f);
            }
            else
            {
                damageTaken2 = graphicUnit.unit.CalculateDamage(targetDistrict.GetCombatStrength(), graphicUnit.unit.CalculateCombatStrength(bonusCombatStrength, null, Global.gameManager.game.cityDictionary[targetDistrict.cityID].teamNum), 1.0f);
            }
            unitHealthPercentage = (targetDistrict.health) / targetDistrict.maxHealth;
            theirDamageTaken.CustomMinimumSize = new Vector2(16, unitHealthPercentage * 180f);
            unitHealthRemainigPercentage = (targetDistrict.health - damageTaken2) / targetDistrict.maxHealth;
            theirLifeRemaining.CustomMinimumSize = new Vector2(16, unitHealthRemainigPercentage * 180f);
            //result evaluation
            if (ability != null)
            {
                battleResultLabel.Text = "Ranged Attack";
                battleResultLabel.LabelSettings.FontColor = Godot.Colors.Yellow;
            }
            else if (targetDistrict.health - damageTaken2 <= 0 && graphicUnit.unit.health - damageTaken <= 0)
            {
                battleResultLabel.Text = "Draw";
                battleResultLabel.LabelSettings.FontColor = Godot.Colors.Yellow;
            }
            else if (targetDistrict.health - damageTaken2 > 0 && graphicUnit.unit.health - damageTaken <= 0)
            {
                battleResultLabel.Text = "Defeat";
                battleResultLabel.LabelSettings.FontColor = Godot.Colors.Red;
            }
            else if (targetDistrict.health - damageTaken2 <= 0 && graphicUnit.unit.health - damageTaken > 0)
            {
                battleResultLabel.Text = "Victory";
                battleResultLabel.LabelSettings.FontColor = Godot.Colors.Green;
            }
            else
            {
                float differenceRatio = (damageTaken2 - damageTaken) / damageTaken;

                if (differenceRatio >= 0.5)
                {
                    battleResultLabel.Text = "Victory";
                    battleResultLabel.LabelSettings.FontColor = Godot.Colors.Green;
                }
                else if (differenceRatio >= 0.2)
                {
                    battleResultLabel.Text = "Minor Victory";
                    battleResultLabel.LabelSettings.FontColor = Godot.Colors.GreenYellow;
                }
                else if (differenceRatio >= -0.2)
                {
                    battleResultLabel.Text = "Draw";
                    battleResultLabel.LabelSettings.FontColor = Godot.Colors.Yellow;
                }
                else if (differenceRatio >= -0.5)
                {
                    battleResultLabel.Text = "Minor Defeat";
                    battleResultLabel.LabelSettings.FontColor = Godot.Colors.Orange;
                }
                else
                {
                    battleResultLabel.Text = "Defeat";
                    battleResultLabel.LabelSettings.FontColor = Godot.Colors.Red;
                }
            }
        }
        else if (targetUnit != null)
        {
            //yours
            yourStrengthLabel.Text = graphicUnit.unit.CalculateCombatStrength(bonusCombatStrength, targetUnit, targetUnit.teamNum).ToString();
            foreach (var child in yourCombatStrengthEffectBox.GetChildren())
            {
                child.Free();
            }
            yourLifeBackground.CustomMinimumSize = new Vector2(16, 180);
            float damageTaken = graphicUnit.unit.CalculateDamage(graphicUnit.unit.CalculateCombatStrength(bonusCombatStrength, targetUnit, targetUnit.teamNum), targetUnit.CalculateCombatStrength(0, graphicUnit.unit, graphicUnit.unit.teamNum), 1.0f);
            float unitHealthPercentage = (graphicUnit.unit.health) / 100.0f;
            float unitHealthRemainigPercentage = (graphicUnit.unit.health - damageTaken) / 100.0f;
            if (ability != null)
            {
                yourDamageTaken.CustomMinimumSize = new Vector2(16, unitHealthPercentage * 180f);
                yourLifeRemaining.CustomMinimumSize = new Vector2(16, unitHealthPercentage * 180f);

            }
            else
            {
                yourDamageTaken.CustomMinimumSize = new Vector2(16, unitHealthPercentage * 180f);
                yourLifeRemaining.CustomMinimumSize = new Vector2(16, unitHealthRemainigPercentage * 180f);
            }

            //theirs
            theirName.Text = targetUnit.name;
            theirStrengthLabel.Text = targetUnit.CalculateCombatStrength(0, graphicUnit.unit, graphicUnit.unit.teamNum).ToString();
            foreach (var child in theirCombatStrengthEffectBox.GetChildren())
            {
                child.Free();
            }
            theirLifeBackground.CustomMinimumSize = new Vector2(16, 180);
            float theirUnitHealthPercentage = (targetUnit.health) / 100.0f;
            theirDamageTaken.CustomMinimumSize = new Vector2(16, theirUnitHealthPercentage * 180f);
            float theirDamageTaken2 = 0.0f;
            if (ability != null && ability.combatPower.Any())
            {
                float abilityPower = ability.combatPower[level];
                theirDamageTaken2 = targetUnit.CalculateDamage(targetUnit.CalculateCombatStrength(0, graphicUnit.unit, graphicUnit.unit.teamNum), abilityPower, 1.0f);
                yourStrengthLabel.Text = abilityPower.ToString();

            }
            else
            {
                theirDamageTaken2 = targetUnit.CalculateDamage(targetUnit.CalculateCombatStrength(0, graphicUnit.unit, graphicUnit.unit.teamNum), graphicUnit.unit.CalculateCombatStrength(bonusCombatStrength, targetUnit, targetUnit.teamNum), 1.0f);
            }
            float theirUnitHealthRemainigPercentage = (targetUnit.health - theirDamageTaken2) / 100.0f;
            theirLifeRemaining.CustomMinimumSize = new Vector2(16, theirUnitHealthRemainigPercentage * 180f);

            //result evaluation
            if(ability != null)
            {
                battleResultLabel.Text = "Ranged Attack";
                battleResultLabel.LabelSettings.FontColor = Godot.Colors.Yellow;
            }
            else if (targetUnit.health - theirDamageTaken2 <= 0 && graphicUnit.unit.health - damageTaken <= 0)
            {
                battleResultLabel.Text = "Draw";
                battleResultLabel.LabelSettings.FontColor = Godot.Colors.Yellow;
            }
            else if (targetUnit.health - theirDamageTaken2 > 0 && graphicUnit.unit.health - damageTaken <= 0)
            {
                battleResultLabel.Text = "Defeat";
                battleResultLabel.LabelSettings.FontColor = Godot.Colors.Red;
            }
            else if (targetUnit.health - theirDamageTaken2 <= 0 && graphicUnit.unit.health - damageTaken > 0)
            {
                battleResultLabel.Text = "Victory";
                battleResultLabel.LabelSettings.FontColor = Godot.Colors.Green;
            }
            else
            {
                float differenceRatio = (theirDamageTaken2 - damageTaken) / damageTaken;


                if (differenceRatio >= 0.5)
                {
                    battleResultLabel.Text = "Victory";
                    battleResultLabel.LabelSettings.FontColor = Godot.Colors.Green;
                }
                else if (differenceRatio >= 0.2)
                {
                    battleResultLabel.Text = "Minor Victory";
                    battleResultLabel.LabelSettings.FontColor = Godot.Colors.GreenYellow;
                }
                else if (differenceRatio >= -0.2)
                {
                    battleResultLabel.Text = "Draw";
                    battleResultLabel.LabelSettings.FontColor = Godot.Colors.Yellow;
                }
                else if (differenceRatio >= -0.5)
                {
                    battleResultLabel.Text = "Minor Defeat";
                    battleResultLabel.LabelSettings.FontColor = Godot.Colors.Orange;
                }
                else
                {
                    battleResultLabel.Text = "Defeat";
                    battleResultLabel.LabelSettings.FontColor = Godot.Colors.Red;
                }
            }
        }


    }

    public void HideCombatPreview()
    {
        combatPreviewPanel.Visible = false;
        combatPreviewVisible = false;
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


        //TODO this shouldnt clear them each time
        foreach(var child in resourcesContainer.GetChildren())
        {
            child.QueueFree();
        }

        foreach(ResourceType resourceType in Global.gameManager.game.localPlayerRef.resourceStockpiles.Keys)
        {
            if (resourceType != ResourceType.None)
            {
                TextureRect resourceIcon = new();
                resourceIcon.ExpandMode = TextureRect.ExpandModeEnum.FitWidth;
                resourceIcon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
                resourceIcon.Texture = Godot.ResourceLoader.Load<Texture2D>("res://" + ResourceLoader.resources[resourceType].IconPath); ;
                Label resourceAmount = new();
                resourceAmount.Text = Global.gameManager.game.localPlayerRef.resourceStockpiles[resourceType].ToString();
                resourcesContainer.AddChild(resourceIcon);
                resourcesContainer.AddChild(resourceAmount);
            }
        }


        UpdateHeroUIDisplay();
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
        else if (element == UIElement.resourcePanel)
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

    public void UpdateHeroRespawn()
    {
        if(Global.gameManager.game.localPlayerRef.ourHeroID != 0)
        {
            Hero hero = (Hero)(Global.gameManager.game.unitDictionary[Global.gameManager.game.localPlayerRef.ourHeroID]);
            heroRespawnBar.Value = hero.respawnCountdown;
            heroRespawnBar.MaxValue = hero.maxRespawnCountdown;
            if(hero.respawnCountdown <= 0)
            {
                heroRespawnLabel.Text = "";
            }
            else
            {
                heroRespawnLabel.Text = hero.respawnCountdown.ToString();
            }
        }
    }

    public void UnitSelected(Unit unit)
    {
        unitInfoPanel.UnitSelected(unit);
    }

    public void HeroSelected(Hero hero)
    {
        heroInfoPanel.HeroSelected(hero);
    }

    public void ShowHeroUI()
    {
        Hero hero = (Hero) Global.gameManager.game.unitDictionary[Global.gameManager.game.localPlayerRef.ourHeroID];
        heroContainer.Visible = true;
        heroButton.Icon = Godot.ResourceLoader.Load<Texture2D>("res://" + hero.heroImagePath);
        healthBar.Value = Math.Round(hero.health);
        healthBar.MaxValue = 100;
        manaBar.Value = hero.mana;
        manaBar.MaxValue = hero.maxMana;
        totalHealthLabel.Text = Math.Round(hero.health) + "/" + "100";
        totalManaLabel.Text = hero.mana + "/" + hero.maxMana;
    }
    public void HeroUnselected(Hero hero)
    {
        heroInfoPanel.HeroUnselected(hero);
    }

    public void UnitUnselected(Unit unit)
    {
        unitInfoPanel.UnitUnselected(unit);
    }

    public void UpdateHeroUIDisplay()
    {
        heroInfoPanel.UpdateHeroPanelInfo();
    }

    public void UpdateUnitUIDisplay()
    {
        unitInfoPanel.UpdateUnitPanelInfo();
        heroInfoPanel.UpdateHeroPanelInfo();
        UpdateHeroUIDisplay();
    }

    public void CloseCurrentWindow()
    {
        if(pauseMenuOpen)
        {
            Global.menuManager.ClearMenus();
        }
        else
        {
            if (policyPanel.governmentPickerOpen)
            {
                policyPanel.CloseGovernmentSwitchPanel();
                windowOpen = true;
            }
            else
            {
                policyPanel.Visible = false;
                windowOpen = false;
                researchTreePanel.Visible = false;
                cultureResearchTreePanel.Visible = false;
                resourcePanel.Visible = false;
                tradeExportPanel.Visible = false;
                tradeRoutePickerPanel.Visible = false;
                diplomacyPanel.Visible = false;
                encampementTakenPopUp.Visible = false;
                cityTakenPopUp.Visible = false;
                eventSelectionPanel.Visible = false;
                ShowGenericUI();
            }

        }
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
        if(assignGovernment)
        {
            GovernmentButtonPressed();
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
        else if (readyToUrbanize)
        {
            researchTreePanel.Visible = false;
            cultureResearchTreePanel.Visible = false;
            resourcePanel.Visible = false;
            HideGenericUI();
            ((GraphicCity)Global.gameManager.graphicManager.graphicObjectDictionary[targetCity.id]).GenerateUrbanizeTargetingPrompt();
            Global.camera.SetHexTarget(targetCity.hex);
            return;
            
        }
        else if (cityNeedsProduction)
        {
            researchTreePanel.Visible = false;
            cultureResearchTreePanel.Visible = false;
            resourcePanel.Visible = false;
            HideGenericUI();
            Global.gameManager.graphicManager.ChangeSelectedObject(targetCity.id, (GraphicCity)Global.gameManager.graphicManager.graphicObjectDictionary[targetCity.id]);
            Global.camera.SetHexTarget(targetCity.hex);
            return;
        }
        else if (waitingForOrders)
        {
            Global.gameManager.graphicManager.ChangeSelectedObject(targetUnit.id, Global.gameManager.graphicManager.graphicObjectDictionary[targetUnit.id]);
            Global.camera.SetHexTarget(targetUnit.hex);
            return;
        }
        else
        {
            endTurnButton.Icon = Godot.ResourceLoader.Load<Texture2D>("res://graphics/ui/icons/sleep.png");
            endTurnButton.Disabled = true;

            Global.gameManager.graphicManager.UnselectObject();
            if (Global.gameManager.graphicManager.GetWaitForTargeting())
            {
                Global.gameManager.graphicManager.ClearWaitForTarget();
            }
            if (Global.gameManager.graphicManager.uiManager.windowOpen || Global.gameManager.graphicManager.uiManager.pauseMenuOpen)
            {
                Global.gameManager.graphicManager.uiManager.CloseCurrentWindow();
                Global.gameManager.graphicManager.uiManager.ShowGenericUIAfterTargeting();
            }

            Global.gameManager.EndTurn(Global.gameManager.game.localPlayerTeamNum);
            CloseCurrentWindow();
            //Global.gameManager.game.turnManager.EndCurrentTurn(Global.gameManager.game.localPlayerTeamNum);
            return;
        }
    }

    public void SetAndShowExpandBuildingLabel(string text)
    {
        expandBuildingLabel.Visible = true;
        expandBuildingLabel.Text = text;
    }

    public void HideExpandBuildingLabel()
    {
        expandBuildingLabel.Visible = false;
    }

    public void UpdateEndTurnButton()
    {
        if (Global.gameManager.game.localPlayerRef.queuedResearch.Count == 0)
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
            readyToUrbanize = false;
            cityNeedsProduction = false;
            targetCity = null;
            waitingForOrders = false;
            return;
        }

        if(assignGovernment)
        {
            endTurnButton.Icon = Godot.ResourceLoader.Load<Texture2D>("res://graphics/ui/icons/government.png");
            readyToGrow = false;
            readyToUrbanize = false;
            cityNeedsProduction = false;
            targetCity = null;
            waitingForOrders = false;
            assignResource = false;
            return;
        }

        bool cityReadyToGrow = false;
        City foundCity = null;
        foreach (int cityID in Global.gameManager.game.localPlayerRef.cityList)
        {
            City city = Global.gameManager.game.cityDictionary[cityID];
            if (city.readyToExpand > 0)
            {
                foundCity = city;
                endTurnButton.Icon = Godot.ResourceLoader.Load<Texture2D>("res://graphics/ui/icons/house.png");
                readyToGrow = true;
                readyToUrbanize = false;
                targetCity = city;
                waitingForOrders = false;
                cityNeedsProduction = false;
                return;
            }
            if (city.readyToUrbanize > 0)
            {
                foundCity = city;
                endTurnButton.Icon = Godot.ResourceLoader.Load<Texture2D>("res://graphics/ui/icons/production.png");
                readyToGrow = false;
                readyToUrbanize = true;
                targetCity = city;
                waitingForOrders = false;
                cityNeedsProduction = false;
                return;
            }
            if (city.productionQueue.Count == 0)
            {
                endTurnButton.Icon = Godot.ResourceLoader.Load<Texture2D>("res://graphics/ui/icons/gears.png");
                readyToGrow = false;
                readyToUrbanize = false;
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
                readyToUrbanize = false;
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
        readyToUrbanize = false;
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

    public void GovernmentButtonPressed()
    {
        Global.gameManager.graphicManager.UnselectObject();
        windowOpen = true;
        policyPanel.Visible = true;
        policyPanel.UpdatePolicyPanel();
        HideGenericUI();
        assignGovernment = false;
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
            foreach (DiplomacyDeal pendingDeal in Global.gameManager.game.teamManager.pendingDeals.Values)
            {
                if (pendingDeal.fromTeamNum == targetTeamNum && pendingDeal.toTeamNum == Global.gameManager.game.localPlayerTeamNum)
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

    public void EncampmentTakenPopUp(Encampment encampment, int takerTeamNum)
    {
        CloseCurrentWindow();
        windowOpen = true;
        encampementTakenPopUp.Visible = true;
        encampementTakenPopUp.UpdateEncampementTakenPopUp(encampment, takerTeamNum);
        HideGenericUI();
    }

    public void CityTakenPopUp(City city, int takerTeamNum)
    {
        CloseCurrentWindow();
        windowOpen = true;
        cityTakenPopUp.Visible = true;
        cityTakenPopUp.UpdateCityTakenPopUp(city, takerTeamNum);
        HideGenericUI();
    }

    public void EventSelectionPopUp(AncientRuins ancientRuins)
    {
        CloseCurrentWindow();
        windowOpen = true;
        eventSelectionPanel.Visible = true;
        eventSelectionPanel.UpdateEventSelectionPanel(ancientRuins);
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
        governmentButton.Visible = false;
        playerList.Visible = false;
        heroContainer.Visible = false;
    }
    public void ShowGenericUI()
    {
        scienceButton.Visible = true;
        cultureButton.Visible = true;
        resourceButton.Visible = true;
        tradeExportButton.Visible = true;
        governmentButton.Visible = true;
        playerList.Visible = true;
        heroContainer.Visible = true;
    }

    public void NewTurnStarted()
    {
        NotWaitingOnLocalPlayer();
        UpdateEndTurnButton();
        endTurnButton.Disabled = false;
    }

    public void GameWaitingOnLocalPlayer()
    {
        waitingOnYouPanel.Visible = true;
        waitingOnLocalPlayer = true;
    }

    public void NotWaitingOnLocalPlayer()
    {
        waitingOnLocalPlayer = false;
        waitingOnYouPanel.Visible = false;
    }

    public void NewDiplomaticDeal(DiplomacyDeal deal)
    {
        Button dealButton = new Button();
        dealButton.Name = deal.fromTeamNum.ToString();
        dealButton.CustomMinimumSize = new Vector2(64, 64);
        dealButton.ExpandIcon = true;
        dealButton.Icon = Godot.ResourceLoader.Load<Texture2D>("res://graphics/ui/icons/diplomacy.png");
        dealButton.Pressed += () => DiplomacyActionButtonPressed(dealButton, deal.fromTeamNum, deal);
        actionQueue.AddChild(dealButton);
        actionQueue.MoveChild(dealButton, 0);
    }

    public void RemoveDiplomaticDealUI(DiplomacyDeal deal)
    {
        foreach (Node child in actionQueue.GetChildren())
        {
            if (child.Name.ToString().Contains(deal.fromTeamNum.ToString()))
            {
                child.QueueFree();
                break;
            }
        }
    }
}

using Godot;
using NetworkMessages;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static AIUtils;

public partial class HeroInfoPanel : Node3D
{
    public Hero hero;

    public PanelContainer heroInfoPanel;

    public TextureRect unitImage;
    public ProgressBar healthProgressBar;

    public TextureRect healthIcon;
    public Label healthLabel;

    public HBoxContainer movementContainer;
    public TextureRect movementIcon;
    public Label movementLabel;

    public HBoxContainer combatStrengthContainer;
    public TextureRect combatStrengthIcon;
    public Label combatStrengthLabel;

    public HBoxContainer rangedStrengthContainer;
    public TextureRect rangedStrengthIcon;
    public Label rangedStrengthLabel;

    public HBoxContainer rangeContainer;
    public TextureRect rangeIcon;
    public Label rangeLabel;

    public Label currentLevelLabel;
    public Label totalHealthLabel;
    public Label totalManaLabel;
    public Label perTurnHealthLabel;
    public Label perTurnManaLabel;
    public ProgressBar healthBar;
    public ProgressBar manaBar;
    public ProgressBar experienceBar;

    public FlowContainer abilityFlowContainer;
    public FlowContainer heroAbilityFlowContainer;

    private PackedScene heroAbilityButtonScene;

    public HeroInfoPanel()
    {
        heroInfoPanel = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/HeroInfoPanel.tscn").Instantiate<PanelContainer>();

        unitImage = heroInfoPanel.GetNode<TextureRect>("UnitHFlow/HBoxContainer/UnitImage");
        healthProgressBar = heroInfoPanel.GetNode<ProgressBar>("UnitHFlow/HBoxContainer/UnitImage/HealthProgressBar");

        healthLabel = heroInfoPanel.GetNode<Label>("UnitHFlow/HBoxContainer/UnitStatContainer/HealthContainer/HealthLabel");

        movementLabel = heroInfoPanel.GetNode<Label>("UnitHFlow/HBoxContainer/UnitStatContainer/MovementContainer/MovementLabel");

        combatStrengthContainer = heroInfoPanel.GetNode<HBoxContainer>("UnitHFlow/HBoxContainer/UnitStatContainer/CombatStrengthContainer");
        combatStrengthLabel = heroInfoPanel.GetNode<Label>("UnitHFlow/HBoxContainer/UnitStatContainer/CombatStrengthContainer/CombatStrengthLabel");

        rangedStrengthContainer = heroInfoPanel.GetNode<HBoxContainer>("UnitHFlow/HBoxContainer/UnitStatContainer/RangedStrengthContainer");
        rangedStrengthLabel = heroInfoPanel.GetNode<Label>("UnitHFlow/HBoxContainer/UnitStatContainer/RangedStrengthContainer/RangedStrengthLabel");

        rangeContainer = heroInfoPanel.GetNode<HBoxContainer>("UnitHFlow/HBoxContainer/UnitStatContainer/RangeContainer");
        rangeLabel = heroInfoPanel.GetNode<Label>("UnitHFlow/HBoxContainer/UnitStatContainer/RangeContainer/RangeLabel");

        abilityFlowContainer = heroInfoPanel.GetNode<FlowContainer>("UnitHFlow/AbilityFlowContainer");
        heroAbilityFlowContainer = heroInfoPanel.GetNode<FlowContainer>("UnitHFlow/VBoxContainer/HeroAbilityFlowContainer");

        currentLevelLabel = heroInfoPanel.GetNode<Label>("UnitHFlow/HBoxContainer/UnitImage/TextureRect/CurrentLevel");
        totalHealthLabel = heroInfoPanel.GetNode<Label>("UnitHFlow/VBoxContainer/HealthBar/TotalHealth");
        totalManaLabel = heroInfoPanel.GetNode<Label>("UnitHFlow/VBoxContainer/ManaBar/TotalMana");
        perTurnHealthLabel = heroInfoPanel.GetNode<Label>("UnitHFlow/VBoxContainer/HealthBar/PerTurnHealth");
        perTurnManaLabel = heroInfoPanel.GetNode<Label>("UnitHFlow/VBoxContainer/ManaBar/PerTurnMana");
        healthBar = heroInfoPanel.GetNode<ProgressBar>("UnitHFlow/VBoxContainer/HealthBar");
        manaBar = heroInfoPanel.GetNode<ProgressBar>("UnitHFlow/VBoxContainer/ManaBar");
        experienceBar = heroInfoPanel.GetNode<ProgressBar>("UnitHFlow/VBoxContainer/ExperienceBar");

        heroAbilityButtonScene = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/HeroAbilityButton.tscn");

        AddChild(heroInfoPanel);
    }


    public void Update(UIElement element)
    {

    }

    public void HeroSelected(Hero hero)
    {
        this.hero = hero;
        heroInfoPanel.Visible = true;
        UpdateHeroPanelInfo();
    }

    public void HeroUnselected(Hero hero)
    {
        this.hero = null;
        heroInfoPanel.Visible = false;
    }

    public void UpdateHeroPanelInfo()
    {
        foreach (var child in abilityFlowContainer.GetChildren())
        {
            child.QueueFree();
        }
        foreach (var child in heroAbilityFlowContainer.GetChildren())
        {
            child.QueueFree();
        }
        if (heroInfoPanel.Visible && hero != null)
        {
            healthProgressBar.Value = Math.Round(hero.health);
            healthLabel.Text = Math.Round(hero.health).ToString() + "/100";
            movementLabel.Text = hero.remainingMovement.ToString() + "/" + hero.movementSpeed.ToString();
            if (hero.combatStrength > 0)
            {
                combatStrengthContainer.Visible = true;
                combatStrengthLabel.Text = hero.combatStrength.ToString() + "(" + hero.attacksLeft.ToString() + "*)";
            }
            else
            {
                combatStrengthContainer.Visible = false;
            }

            rangeContainer.Visible = false;
            rangedStrengthContainer.Visible = false;

            foreach (UnitAbility ability in hero.abilities)
            {
                if(ability.name == "RangedAttack" || ability.name == "BombardAttack")
                {
                    rangedStrengthContainer.Visible = true;
                    rangedStrengthLabel.Text = ability.combatPower.ToString();

                    rangeContainer.Visible = true;
                    rangedStrengthLabel.Visible = true;
                }
                Button abilityButton = new Button();
                abilityButton.Icon = Godot.ResourceLoader.Load<Texture2D>("res://"+ability.iconPath);
                abilityButton.IconAlignment = HorizontalAlignment.Center;
                abilityButton.ExpandIcon = true;
                abilityButton.CustomMinimumSize = new Vector2(64, 64);
                abilityButton.Pressed += () => AbilityButtonPressed(ability);
                abilityFlowContainer.AddChild(abilityButton);
                if(ability.currentCharges <= 0)
                {
                    abilityButton.Disabled = true;
                }
                else
                {
                    abilityButton.Disabled = false;
                }
                if (hero.teamNum != Global.gameManager.game.localPlayerTeamNum)
                {
                    abilityButton.Disabled = true;
                }
                //check if there are any valid targets
                List<Hex> hexes = new List<Hex>();
                foreach (Hex hex in hero.hex.WrappingRange(ability.range, Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.top, Global.gameManager.game.mainGameBoard.bottom))
                {
                    if (ability.validTargetTypes.IsHexValidTarget(Global.gameManager.game.mainGameBoard.gameHexDict[hex], hero))
                    {
                        hexes.Add(hex);
                    }
                }
                if(ability.name == "SettleCityAbility" || ability.name == "SettleCapitalAbility")
                {
                    if(hero.CanSettleHere(hero.hex, 3, new List<TerrainType>(){ TerrainType.Flat, TerrainType.Rough}, false))
                    {
                        abilityButton.Disabled = false;
                    }
                    else
                    {
                        abilityButton.Disabled = true;
                    }
                }

                if (hexes.Count <= 0)
                {
                    abilityButton.Disabled = true;
                }
            }
            foreach (HeroAbility heroAbility in hero.heroAbilities)
            {
                Button abilityButton = heroAbilityButtonScene.Instantiate<Button>();
                abilityButton.Icon = Godot.ResourceLoader.Load<Texture2D>("res://" + heroAbility.ability.iconPath);
                abilityButton.IconAlignment = HorizontalAlignment.Center;
                abilityButton.ExpandIcon = true;
                abilityButton.CustomMinimumSize = new Vector2(64, 64);
                abilityButton.Pressed += () => HeroAbilityButtonPressed(heroAbility);
                Button levelupButton = abilityButton.GetNode<Button>("LevelUpButton");
                TextureProgressBar cooldownBar = abilityButton.GetNode<TextureProgressBar>("Cooldown");
                cooldownBar.Value = heroAbility.currentCooldown;
                cooldownBar.MaxValue = heroAbility.cooldown[heroAbility.level];
                Label cooldownLabel = abilityButton.GetNode<Label>("CooldownLabel");
                cooldownLabel.Text = heroAbility.currentCooldown.ToString();
                if(heroAbility.currentCooldown <= 0)
                {
                    cooldownLabel.Visible = false;
                }
                else
                {
                    cooldownLabel.Visible = true;
                }
                HBoxContainer levelupPips = abilityButton.GetNode<HBoxContainer>("LevelUpPips");

                for (int i = 0; i < heroAbility.maxLevel; i++)
                {
                    TextureRect levelPip = new();
                    GradientTexture1D temp = new GradientTexture1D();
                    temp.Gradient = new();
                    if (i+1 <= heroAbility.level)
                    {
                        temp.Gradient.Colors = new Godot.Color[] { Godot.Colors.Gold };
                    }
                    else
                    {
                        temp.Gradient.Colors = new Godot.Color[] { Godot.Colors.DarkGray };
                    }
                    levelPip.Texture = temp;
                    levelPip.ExpandMode = TextureRect.ExpandModeEnum.FitWidth;
                    levelupPips.AddChild(levelPip);
                }
                levelupButton.Pressed += () => LevelUpButtonPressed(heroAbility);
                if (heroAbility.CanLevelUp(hero))
                {
                    levelupButton.Visible = true;
                    levelupButton.Disabled = false;
                }
                else
                {
                    levelupButton.Visible = false;
                    levelupButton.Disabled = true;
                }
                Control tempControl = new Control();
                tempControl.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill; 
                heroAbilityFlowContainer.AddChild(tempControl);
                heroAbilityFlowContainer.AddChild(abilityButton);
                if (heroAbility.ability.currentCharges <= 0 || heroAbility.manaCost[heroAbility.level] > hero.mana || heroAbility.currentCooldown > 0 || heroAbility.level <= 0)
                {
                    abilityButton.Disabled = true;
                }
                else
                {
                    abilityButton.Disabled = false;
                }
                if (hero.teamNum != Global.gameManager.game.localPlayerTeamNum)
                {
                    abilityButton.Disabled = true;
                }
                //check if there are any valid targets
                List<Hex> hexes = new List<Hex>();
                foreach (Hex hex in hero.hex.WrappingRange(heroAbility.ability.range, Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.top, Global.gameManager.game.mainGameBoard.bottom))
                {
                    if (heroAbility.ability.validTargetTypes.IsHexValidTarget(Global.gameManager.game.mainGameBoard.gameHexDict[hex], hero))
                    {
                        hexes.Add(hex);
                    }
                }
                //if it is a settle ability check the parameters special
                /*if ()
                {
                    if (hero.CanSettleHere(hero.hex, 3, new List<TerrainType>() { TerrainType.Flat, TerrainType.Rough }, false))
                    {
                        abilityButton.Disabled = false;
                    }
                    else
                    {
                        abilityButton.Disabled = true;
                    }
                }*/

                if (hexes.Count <= 0)
                {
                    abilityButton.Disabled = true;
                }
            }
            Control tempControl2 = new Control();
            tempControl2.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            heroAbilityFlowContainer.AddChild(tempControl2);
            UpdateHealthAndMana();
        }
    }

    public void UpdateHealthAndMana()
    {
        currentLevelLabel.Text = hero.level.ToString();
        totalHealthLabel.Text = Math.Round(hero.health) + "/" + "100";
        totalManaLabel.Text = hero.mana + "/" + hero.maxMana ;
        perTurnHealthLabel.Text = "(+"+hero.healingFactor+")";
        perTurnManaLabel.Text = "(+" + hero.manaRegeneration + ")";
        healthBar.Value = Math.Round(hero.health);
        healthBar.MaxValue = 100;
        manaBar.Value = hero.mana;
        manaBar.MaxValue = hero.maxMana;
        experienceBar.Value = hero.experience;
        experienceBar.MaxValue = hero.experienceToLevelUp[hero.level];
    }
    private void AbilityButtonPressed(UnitAbility ability)
    {
        if (Global.gameManager.game.localPlayerRef.turnFinished)
        {
            return;
        }
        if (ability.validTargetTypes.TargetUnits || ability.validTargetTypes.TargetRuralBuildings || ability.validTargetTypes.TargetUrbanBuildings ||ability.validTargetTypes.TargetTiles)
        {
            ((GraphicUnit)Global.gameManager.graphicManager.graphicObjectDictionary[hero.id]).GenerateTargetingPrompt(ability);
        }
        else if (ability.validTargetTypes.TargetSelf)
        {
            if (ability.validTargetTypes.IsHexValidTarget(Global.gameManager.game.mainGameBoard.gameHexDict[Global.gameManager.game.unitDictionary[ability.usingUnitID].hex], Global.gameManager.game.unitDictionary[ability.usingUnitID]))
            {
                Global.gameManager.ActivateAbility(hero.id, ability.name, hero.hex); //networked command
            }
        }
        return;
    }

    private void HeroAbilityButtonPressed(HeroAbility heroAbility)
    {
        if (Global.gameManager.game.localPlayerRef.turnFinished || heroAbility.currentCooldown > 0 && hero.mana < heroAbility.manaCost[heroAbility.level])
        {
            return;
        }
        AbilityButtonPressed(heroAbility.ability);
        return;
    }

    private void LevelUpButtonPressed(HeroAbility ability)
    {
        if (Global.gameManager.game.localPlayerRef.turnFinished)
        {
            return;
        }
        //ability.LevelUpAbility(hero);
        Global.gameManager.LevelUpAbility(hero.id, ability.ability.name);
        UpdateHeroPanelInfo();
        return;
    }

}

using Godot;
using NetworkMessages;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

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

    public FlowContainer abilityFlowContainer;

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
        if (heroInfoPanel.Visible && hero != null)
        {
            healthProgressBar.Value = hero.health;
            healthLabel.Text = hero.health.ToString() + "/100";
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
                abilityButton.Pressed += () => AbilityButtonPressed(ability, abilityButton);
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
                Button abilityButton = new Button();
                abilityButton.Icon = Godot.ResourceLoader.Load<Texture2D>("res://" + heroAbility.ability.iconPath);
                abilityButton.IconAlignment = HorizontalAlignment.Center;
                abilityButton.ExpandIcon = true;
                abilityButton.CustomMinimumSize = new Vector2(64, 64);
                abilityButton.Pressed += () => AbilityButtonPressed(heroAbility.ability, abilityButton);
                abilityFlowContainer.AddChild(abilityButton);
                if (heroAbility.ability.currentCharges <= 0 || heroAbility.manaCost[heroAbility.level] > hero.mana || heroAbility.currentCooldown > 0)
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
        }
    }

    private void AbilityButtonPressed(UnitAbility ability, Button sourceButton)
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

}

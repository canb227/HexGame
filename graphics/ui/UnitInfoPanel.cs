﻿using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public partial class UnitInfoPanel : Node3D
{
    private Game game;
    private GraphicManager graphicManager;
    public Unit unit;

    public PanelContainer unitInfoPanel;

    public TextureRect unitImage;
    public ProgressBar healthProgressBar;

    public HBoxContainer healthContainer;
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

    public UnitInfoPanel(GraphicManager graphicManager, Game game)
    {
        this.game = game;
        this.graphicManager = graphicManager;
        unitInfoPanel = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/UnitInfoPanel.tscn").Instantiate<PanelContainer>();

        //unitInfoPanel.Visible = true;

        unitImage = unitInfoPanel.GetNode<TextureRect>("UnitHFlow/UnitImageContainer/UnitImage");
        healthProgressBar = unitInfoPanel.GetNode<ProgressBar>("UnitHFlow/UnitImageContainer/UnitImage/HealthProgressBar");

        healthContainer = unitInfoPanel.GetNode<HBoxContainer>("UnitHFlow/UnitStatContainer/HealthContainer");
        healthIcon = unitInfoPanel.GetNode<TextureRect>("UnitHFlow/UnitStatContainer/HealthContainer/HealthIcon");
        healthLabel = unitInfoPanel.GetNode<Label>("UnitHFlow/UnitStatContainer/HealthContainer/HealthLabel");

        movementContainer = unitInfoPanel.GetNode<HBoxContainer>("UnitHFlow/UnitStatContainer/MovementContainer");
        movementIcon = unitInfoPanel.GetNode<TextureRect>("UnitHFlow/UnitStatContainer/MovementContainer/MovementIcon");
        movementLabel = unitInfoPanel.GetNode<Label>("UnitHFlow/UnitStatContainer/MovementContainer/MovementLabel");

        combatStrengthContainer = unitInfoPanel.GetNode<HBoxContainer>("UnitHFlow/UnitStatContainer/CombatStrengthContainer");
        combatStrengthIcon = unitInfoPanel.GetNode<TextureRect>("UnitHFlow/UnitStatContainer/CombatStrengthContainer/CombatStengthIcon");
        combatStrengthLabel = unitInfoPanel.GetNode<Label>("UnitHFlow/UnitStatContainer/CombatStrengthContainer/CombatStrengthLabel");

        rangedStrengthContainer = unitInfoPanel.GetNode<HBoxContainer>("UnitHFlow/UnitStatContainer/RangedStrengthContainer");
        rangedStrengthIcon = unitInfoPanel.GetNode<TextureRect>("UnitHFlow/UnitStatContainer/RangedStrengthContainer/RangedStrengthIcon");
        rangedStrengthLabel = unitInfoPanel.GetNode<Label>("UnitHFlow/UnitStatContainer/RangedStrengthContainer/RangedStrengthLabel");

        rangeContainer = unitInfoPanel.GetNode<HBoxContainer>("UnitHFlow/UnitStatContainer/RangeContainer");
        rangeIcon = unitInfoPanel.GetNode<TextureRect>("UnitHFlow/UnitStatContainer/RangeContainer/RangeIcon");
        rangeLabel = unitInfoPanel.GetNode<Label>("UnitHFlow/UnitStatContainer/RangeContainer/RangeLabel");

        abilityFlowContainer = unitInfoPanel.GetNode<FlowContainer>("UnitHFlow/AbilityFlowContainer");

        AddChild(unitInfoPanel);
    }


    public void Update(UIElement element)
    {

    }

    public void UnitSelected(Unit unit)
    {
        this.unit = unit;
        unitInfoPanel.Visible = true;
        UpdateUnitPanelInfo();
    }

    public void UnitUnselected(Unit unit)
    {
        this.unit = null;
        unitInfoPanel.Visible = false;
    }

    public void UpdateUnitPanelInfo()
    {
        foreach (var child in abilityFlowContainer.GetChildren())
        {
            child.QueueFree();
        }
        if (unitInfoPanel.Visible && unit != null)
        {
            healthProgressBar.Value = unit.health;
            healthLabel.Text = unit.health.ToString() + "/100";
            movementLabel.Text = unit.remainingMovement.ToString() + "/" + unit.movementSpeed.ToString();
            if (unit.combatStrength > 0)
            {
                combatStrengthContainer.Visible = true;
                combatStrengthLabel.Text = unit.combatStrength.ToString() + "(" + unit.attacksLeft.ToString() + "*)";
            }
            else
            {
                combatStrengthContainer.Visible = false;
            }

            rangeContainer.Visible = false;
            rangedStrengthContainer.Visible = false;

            foreach (UnitAbility ability in unit.abilities)
            {
                if(ability.name == "RangedAttack")
                {
                    rangedStrengthContainer.Visible = true;
                    rangedStrengthLabel.Text = ability.combatPower.ToString();

                    rangeContainer.Visible = true;
                    rangedStrengthLabel.Visible = true;
                }
                Button abilityButton = new Button();
                abilityButton.Icon = Godot.ResourceLoader.Load<Texture2D>("res://"+ability.iconPath);
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

                //check if there are any valid targets
                List<Hex> hexes = new List<Hex>();
                foreach (Hex hex in unit.gameHex.hex.WrappingRange(ability.range + 1, unit.gameHex.gameBoard.left, unit.gameHex.gameBoard.right, unit.gameHex.gameBoard.top, unit.gameHex.gameBoard.bottom))
                {
                    if (ability.validTargetTypes.IsHexValidTarget(unit.gameHex.gameBoard.gameHexDict[hex], unit))
                    {
                        hexes.Add(hex);
                    }
                }

                if (hexes.Count <= 0)
                {
                    abilityButton.Disabled = true;
                }
            }
        }
    }

    private void AbilityButtonPressed(UnitAbility ability, Button sourceButton)
    {
        if (ability.validTargetTypes.TargetUnits || ability.validTargetTypes.TargetRuralBuildings || ability.validTargetTypes.TargetUrbanBuildings ||ability.validTargetTypes.TargetTiles)
        {
            ((GraphicUnit)graphicManager.graphicObjectDictionary[unit.id]).GenerateTargetingPrompt(ability);
        }
        else if (ability.validTargetTypes.TargetSelf)
        {
            if (ability.validTargetTypes.IsHexValidTarget(ability.usingUnit.gameHex, ability.usingUnit))
            {
                ability.ActivateAbility();
            }
        }
        return;
    }

}

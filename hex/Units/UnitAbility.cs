using Godot;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Formats.Asn1;
using System.IO;
using System.Linq;
using static AIUtils;

[Serializable]
public class UnitAbility
{
    public String name { get; set; }
    public string description { get; set; }
    public bool isUnlocked { get; set; }
    private UnitEffect effect { get; set; }
    public int usingUnitID { get; set; }
    public List<float> combatPower { get; set; } = new();
    public int currentCharges { get; set; }
    public int maxChargesPerTurn { get; set; } //-1 means no reset... we use the charge then its gone I think is the idea
    public int range { get; set; }
    public String iconPath { get; set; }
    public TargetSpecification validTargetTypes { get; set; }

    public UnitAbility(int usingUnitID, string abilityName, string description="", bool isUnlocked=true, List<float> combatPower = null, int maxChargesPerTurn = 1, int range = 0, TargetSpecification validTargetTypes = null, String iconPath = "")
    {
        this.usingUnitID = usingUnitID;
        this.effect = new UnitEffect(abilityName);
        name = abilityName;
        this.description = description;
        this.isUnlocked = isUnlocked;
        this.iconPath = iconPath;
        this.combatPower = combatPower;
        this.maxChargesPerTurn = maxChargesPerTurn;
        this.currentCharges = maxChargesPerTurn;
        this.range = range;
        if(validTargetTypes == null)
        {
            validTargetTypes = new TargetSpecification();
        }
        this.validTargetTypes = validTargetTypes;
    }

    public UnitAbility()
    {

    }

    public void ResetAbilityUses()
    {
        if(maxChargesPerTurn > -1)
        {
            currentCharges = maxChargesPerTurn;
        }
    }

    public UnitEffect GetUnitEffect()
    {
        if(this.effect == null)
        {
            this.effect = new UnitEffect(name);
            name = effect.functionName;
        }
        return this.effect;
    }

    public UnitEffect GetUnitEffectWithLevel(int level)
    {
        this.effect = new UnitEffect(name, level);
        name = effect.functionName;
        return this.effect;
    }

    public bool ActivateAbility(GameHex abilityTarget, int level = 0)
    {
        if (this.effect == null)
        {
            this.effect = new UnitEffect(name);
            name = effect.functionName;
        }
        if (currentCharges > 0)
        {
            currentCharges -= 1;
            if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
            {
                manager.CallDeferred("Update2DUI", (int)UIElement.unitDisplay);
            }
            if(combatPower != null && combatPower.Any())
            {
                return effect.Apply(usingUnitID, level, combatPower[level], abilityTarget);
            }
            else
            {
                return effect.Apply(usingUnitID, level, 0, abilityTarget);
            }

        }
        return false;
    }

    // public List<Hex> ValidAbilityTargets(Unit unit)
    // {
    //     foreach(Hex hex in unit.hex.WrappingRange(range, Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.top, Global.gameManager.game.mainGameBoard.bottom))
    //     {
    //         IsValidTarget(UnitType? unitType, UnitClass? unitClass, String? buildingType, TerrainType? terrainType, bool isEnemy = false, bool isAlly = false)
    //         //TODO
    //     }
    // }
}

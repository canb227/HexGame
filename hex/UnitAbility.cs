using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data;
using System.Formats.Asn1;
using Godot;
using System.IO;

[Serializable]
public class UnitAbility
{
    public String name { get; set; }
    private UnitEffect effect { get; set; }
    public int usingUnitID { get; set; }
    public float combatPower { get; set; }
    public int currentCharges { get; set; }
    public int maxChargesPerTurn { get; set; } //-1 means no reset... we use the charge then its gone I think is the idea
    public int range { get; set; }
    public String iconPath { get; set; }
    public TargetSpecification validTargetTypes { get; set; }

    public UnitAbility(int usingUnitID, string abilityName, float combatPower = 0.0f, int maxChargesPerTurn = 1, int range = 0, TargetSpecification validTargetTypes = null, String iconPath = "")
    {
        this.usingUnitID = usingUnitID;
        this.effect = new UnitEffect(abilityName);
        name = abilityName;
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

    public bool ActivateAbility(GameHex abilityTarget)
    {
        if (this.effect == null)
        {
            this.effect = new UnitEffect(name);
            name = effect.functionName;
        }
        if (currentCharges > 0)
        {
            currentCharges -= 1;
            if(Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
            {
                manager.Update2DUI(UIElement.unitDisplay);
            }
            return effect.Apply(usingUnitID, combatPower, abilityTarget);
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

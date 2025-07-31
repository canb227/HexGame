using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data;
using System.Formats.Asn1;
using Godot;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime;

[Serializable]
public class Hero : Unit
{
    public List<HeroAbility> heroAbilities { get; set; } = new();
    public int maxMana { get; set; }
    public int mana { get; set; }
    public int manaRegeneration { get; set; }
    public int baseMaxMana { get; set; }
    public int baseMana { get; set; }
    public int baseManaRegeneration { get; set; }
    public int experience { get; set; }
    public int[] experienceToLevelUp { get; set; }
    public int level { get; set; }
    public int maxLevel { get; set; }

    public Hero(String heroName, int combatModifier, int id, int teamNum)
    {
        this.id = id;
        this.name = heroName;
        this.teamNum = teamNum;

        if (HeroLoader.heroDict.TryGetValue(heroName, out HeroInfo heroInfo))
        {
            this.maxMana = heroInfo.mana;
            this.baseMaxMana = heroInfo.mana;
            this.mana = heroInfo.mana;
            this.baseMana = heroInfo.mana;
            this.manaRegeneration = heroInfo.manaRegeneration;
            this.baseManaRegeneration = heroInfo.manaRegeneration;
            this.maxLevel = heroInfo.maxLevel;
            this.experienceToLevelUp = heroInfo.experienceToLevelUp;
            foreach (HeroAbility heroAbility in heroInfo.heroAbilities)
            {
                UnitAbility ability = new UnitAbility(id, heroAbility.ability.name, heroAbility.ability.combatPower, heroAbility.ability.maxChargesPerTurn, heroAbility.ability.range, heroAbility.ability.validTargetTypes, heroAbility.ability.iconPath);
                heroAbilities.Add(new HeroAbility(ability, heroAbility.manaCost, heroAbility.cooldown, heroAbility.level, heroAbility.maxLevel, heroAbility.minLevelToLearn));
            }

            Global.gameManager.game.unitDictionary.TryAdd(id, this);
            this.unitType = heroInfo.unitInfo.UnitName;
            this.unitClass = heroInfo.unitInfo.Class;
            this.movementCosts = heroInfo.unitInfo.MovementCosts;
            this.sightCosts = heroInfo.unitInfo.SightCosts;
            this.movementSpeed = heroInfo.unitInfo.MovementSpeed;
            this.remainingMovement = heroInfo.unitInfo.MovementSpeed;
            this.sightRange = heroInfo.unitInfo.SightRange;
            this.healingFactor = heroInfo.unitInfo.HealingFactor;
            this.combatStrength = heroInfo.unitInfo.CombatPower + combatModifier;
            this.baseCombatStrength = heroInfo.unitInfo.CombatPower + combatModifier;
            this.maintenanceCost = heroInfo.unitInfo.MaintenanceCost;
            this.baseMaintenanceCost = heroInfo.unitInfo.MaintenanceCost;
            this.baseZoneOfControl = heroInfo.unitInfo.ZoneOfControl;
            this.zoneOfControl = heroInfo.unitInfo.ZoneOfControl;
            this.baseIgnoreZoneOfControl = heroInfo.unitInfo.IgnoreZoneOfControl;
            this.ignoreZoneOfControl = heroInfo.unitInfo.IgnoreZoneOfControl;
            this.IconPath = heroInfo.unitInfo.IconPath;


            foreach (String effectName in heroInfo.unitInfo.Effects)
            {
                AddEffect(new UnitEffect(effectName));
            }

            foreach (String abilityName in heroInfo.unitInfo.Abilities.Keys)
            {
                AddAbility(abilityName, heroInfo.unitInfo);
            }
            //generic abilities
            AddGenericAbility("Sleep", "graphics/ui/icons/sleep.png");
            AddGenericAbility("Skip", "graphics/ui/icons/skipturn.png");
            RecalculateEffects();
        }
        else
        {
            throw new ArgumentException($"Hero '{name}' not found in hero data.");
        }
    }

    public override void SpawnSetup(GameHex targetGameHex)
    {
        spawnSetupFinished = true;
        targetGameHex.units.Add(id);
        hex = targetGameHex.hex;
        Global.gameManager.game.playerDictionary[teamNum].unitList.Add(this.id);
        RecalculateEffects();
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager)) manager.CallDeferred("NewHero", id);
    }

    public override void OnTurnStarted(int turnNumber)
    {
        base.OnTurnStarted(turnNumber);
        mana += manaRegeneration;
        if(mana > maxMana)
        {
            mana = maxMana;
        }
        foreach(HeroAbility heroAbility in heroAbilities)
        {
            if(heroAbility.currentCooldown > 0)
            {
                heroAbility.currentCooldown--;
            }
        }
    }
}
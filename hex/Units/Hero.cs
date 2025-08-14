using Godot;
using NetworkMessages;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Formats.Asn1;
using System.IO;
using System.Linq;
using System.Runtime;
using static System.Net.Mime.MediaTypeNames;

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
    public string heroImagePath { get; set; }
    public int avaliableSkillPoints { get; set; }
    public int respawnCountdown { get; set; }
    public int maxRespawnCountdown { get; set; } = 10;
    public bool isDead { get; set; }

    public Hero(String heroName, int combatModifier, int id, int teamNum)
    {
        this.id = id;
        this.name = heroName;
        this.teamNum = teamNum;

        this.level = 1;
        this.avaliableSkillPoints = 1;

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
            this.heroImagePath = heroInfo.heroImagePath;
            foreach (HeroAbility heroAbility in heroInfo.heroAbilities)
            {
                UnitAbility ability = new UnitAbility(id, heroAbility.ability.name, heroAbility.ability.combatPower, heroAbility.ability.maxChargesPerTurn, heroAbility.ability.range, heroAbility.ability.validTargetTypes, heroAbility.ability.iconPath);
                heroAbilities.Add(new HeroAbility(ability, heroAbility.manaCost, heroAbility.cooldown, heroAbility.level, heroAbility.maxLevel, heroAbility.minLevelToLearn, heroAbility.isUltimate));
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

    public Hero() { }
    public override void onDeathEffects()
    {
        Global.gameManager.game.mainGameBoard.gameHexDict[hex].units.Remove(this.id);
        Global.gameManager.game.playerDictionary[teamNum].unitList.Remove(this.id);
        isSleeping = true;
        isDead = true;
        this.respawnCountdown = maxRespawnCountdown;
        RemoveVision(true);
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
        {
            manager.CallDeferred("UpdateGraphic", id, (int)GraphicUpdateType.Remove);
            Global.gameManager.graphicManager.uiManager.CallDeferred("Update", (int)UIElement.endTurnButton);
        }
    }

    public override void SpawnSetup(GameHex targetGameHex, bool isRespawn=false)
    {
        Global.gameManager.game.playerDictionary[teamNum].ourHeroID = this.id;
        spawnSetupFinished = true;
        targetGameHex.units.Add(id);
        hex = targetGameHex.hex;
        Global.gameManager.game.playerDictionary[teamNum].unitList.Add(this.id);
        isSleeping = false;
        health = 100;
        RecalculateEffects();
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager)) manager.CallDeferred("NewHero", id);
    }

    public void RespawnHero()
    {
        foreach (int cityID in Global.gameManager.game.playerDictionary[teamNum].cityList)
        {
            if (Global.gameManager.game.cityDictionary[cityID].isCapital)
            {
                Global.gameManager.game.mainGameBoard.gameHexDict[Global.gameManager.game.cityDictionary[cityID].hex].SpawnUnit(this, false, true, true);
                isDead = false;
            }
        }
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
            heroAbility.ability.ResetAbilityUses();
        }
    }

    public void IncreaseExperience(int experienceToAdd)
    {
        experience += experienceToAdd;
        if (experience >= experienceToLevelUp[level])
        {
            experience -= experienceToLevelUp[level];
            level++;
            combatStrength = Mathf.Round(baseCombatStrength * (1 + (level / 8)));
            avaliableSkillPoints++;
            Global.gameManager.graphicManager.uiManager.UpdateHeroUIDisplay();
        }
    }

    public void ProcessPassiveEffect(HeroAbility heroAbility)
    {
        if(heroAbility.ability.name == "MysticRegeneration")
        {
            if (heroAbility.level == 1)
            {
                manaRegeneration += 5;
                baseManaRegeneration += 5;
                maxMana += 10;
                baseMaxMana += 10;
            }
            else if (heroAbility.level == 2)
            {
                manaRegeneration += 5;
                baseManaRegeneration += 5;
                maxMana += 10;
                baseMaxMana += 10;
            }
            else if (heroAbility.level == 3)
            {
                manaRegeneration += 5;
                baseManaRegeneration += 5;
                maxMana += 10;
                baseMaxMana += 10;
            }
            else if (heroAbility.level == 4)
            {
                manaRegeneration += 5;
                baseManaRegeneration += 5;
                maxMana += 10;
                baseMaxMana += 10;
            }
        }
        else if(heroAbility.ability.name == "Blood?")
        {
            if(heroAbility.level == 1)
            {
                healingOverTime += 3;
            }
            else if(heroAbility.level == 2)
            {
                healingOverTime += 3;
            }
            else if (heroAbility.level == 3)
            {
                healingOverTime += 3;
            }
            else if (heroAbility.level == 4)
            {
                healingOverTime += 3;
            }
        }
        else if(heroAbility.ability.name == "ForTheHorde")
        {
            if(heroAbility.level == 1)
            {
                onKillEffects.Add("ForTheHorde", heroAbility.ability.GetUnitEffectWithLevel(1));
            }
            else if(heroAbility.level == 2)
            {
                onKillEffects.Remove("ForTheHorde");
                onKillEffects.Add("ForTheHorde", heroAbility.ability.GetUnitEffectWithLevel(2));
            }
            else if(heroAbility.level == 3)
            {
                onKillEffects.Remove("ForTheHorde");
                onKillEffects.Add("ForTheHorde", heroAbility.ability.GetUnitEffectWithLevel(3));
            }
        }
    }


}
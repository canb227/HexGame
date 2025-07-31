using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data;
using System.Formats.Asn1;
using Godot;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;

[Serializable]
public class HeroAbility
{
    public UnitAbility ability;
    public int currentCooldown;
    public int[] manaCost;
    public int[] cooldown;
    public int level;
    public int maxLevel;
    public int minLevelToLearn;
    public HeroAbility(UnitAbility ability, int[] manaCost, int[] cooldown, int level, int maxLevel, int minLevelToLearn)
    {
        this.ability = ability;
        this.manaCost = manaCost;
        this.cooldown = cooldown;
        this.currentCooldown = 0;
        this.level = level;
        this.maxLevel = maxLevel;
        this.minLevelToLearn = minLevelToLearn;
    }

    public HeroAbility()
    {

    }

    public bool ActivateAbility(Hero hero, GameHex abilityTarget)
    {
        GD.Print("Activate Level " + level);
        if(currentCooldown <= 0 && hero.mana >= manaCost[level] && ability.ActivateAbility(abilityTarget, level))
        {
            hero.mana -= manaCost[level];
            currentCooldown = cooldown[level];
            return true;
        }
        return false;
    }

    public bool LevelUpAbility(Hero hero)
    {
        //must meet min level requirement
        if(hero.level < minLevelToLearn)
        {
            return false;
        }
        //if we were already max level fail
        if(level >= maxLevel)
        {
            level = maxLevel;
            return false;
        }

        //increase level
        level++;
        return true;
    }
}
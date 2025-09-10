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
    
    public string abilityName { get; set; }
    public int usageCount { get; set; }
    public int currentCooldown { get; set; }
    public int[] manaCost { get; set; }
    public int[] cooldown { get; set; }
    public int level { get; set; }
    public int maxLevel { get; set; }
    public int minLevelToLearn { get; set; }
    public bool isUltimate { get; set; }
    public HeroAbility(string abilityName, int usageCount, int[] manaCost, int[] cooldown, int level, int maxLevel, int minLevelToLearn, bool isUltimate)
    {
        this.abilityName = abilityName;
        this.usageCount = usageCount;
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


    public bool LevelUpAbility(Hero hero)
    {
        if(CanLevelUp(hero))
        {
            level++;
            hero.avaliableSkillPoints--;
            if (cooldown[level] == 0)
            {
                //ability is passive so add the passive effect
                hero.ProcessPassiveEffect(this);
            }
            return true;
        }
        return false;
    }

    public bool CanLevelUp(Hero hero)
    {
        //hero has a skill point ready
        if(hero.avaliableSkillPoints <= 0)
        {
            return false;
        }
        //must meet min level requirement
        if (hero.level < minLevelToLearn)
        {
            return false;
        }
        //if we were already max level fail
        if (level >= maxLevel)
        {
            level = maxLevel;
            return false;
        }
        if (isUltimate)
        {
            // Ultimate ability upgrade milestones: 6, 12, 18
            if ((level == 0 && hero.level >= 6) ||
                (level == 1 && hero.level >= 12) ||
                (level == 2 && hero.level >= 18))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            // Normal ability upgrade milestones: 1, 3, 5, 7
            if ((level == 0 && hero.level >= 1) ||
                (level == 1 && hero.level >= 3) ||
                (level == 2 && hero.level >= 5) ||
                (level == 3 && hero.level >= 7))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return true;
    }
}
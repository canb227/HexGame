using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data;
using System.Formats.Asn1;
using Godot;
using System.IO;
using static System.Net.Mime.MediaTypeNames;

[Serializable]
public class HeroAbility
{
    public UnitAbility ability;
    public int[] manaCost;
    public int[] cooldown;
    public int level;
    public int maxLevel;
    public int minLevelToLearn;
    public HeroAbility()
    {
    }
}
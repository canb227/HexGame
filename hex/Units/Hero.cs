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
public class Hero : Unit
{
    public List<HeroAbility> heroAbilities { get; set; } = new();
    public int mana { get; set; }
    public int manaRegeneration { get; set; }
    public int level { get; set; }
    public int maxLevel { get; set; }

    public Hero()
    {
    }
}
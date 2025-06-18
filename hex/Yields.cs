using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data;
using System.IO;

public enum YieldType
{
    food,
    production,
    gold,
    science,
    culture,
    happiness,
    influence
}
public class Yields
{
    public Yields()
    {
    }
    public Yields(Yields yields)
    {
        this.food = yields.food;
        this.production = yields.production;
        this.gold = yields.gold;
        this.science = yields.science;
        this.culture = yields.culture;
        this.happiness = yields.happiness;
        this.influence = yields.influence;
    }

    public Dictionary<YieldType, float> YieldsToDict()
    {
        Dictionary<YieldType, float> temp = new();
        temp.Add(YieldType.food, food);
        temp.Add(YieldType.production, production);
        temp.Add(YieldType.gold, gold);
        temp.Add(YieldType.science, science);
        temp.Add(YieldType.culture, culture);
        temp.Add(YieldType.happiness, happiness);
        temp.Add(YieldType.influence, influence);
        return temp;
    }
    public float food {get; set;}
    public float production {get; set;}
    public float gold {get; set;}
    public float science {get; set;}
    public float culture {get; set;}
    public float happiness {get; set;}
    public float influence {get; set;}
    
    // Overload the + operator
    public static Yields operator +(Yields a, Yields b)
    {
        return new Yields
        {
            food = a.food + b.food,
            production = a.production + b.production,
            gold = a.gold + b.gold,
            science = a.science + b.science,
            culture = a.culture + b.culture,
            happiness = a.happiness + b.happiness,
            influence = a.influence + b.influence
        };
    }
}

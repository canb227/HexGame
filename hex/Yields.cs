using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data;
using System.IO;

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

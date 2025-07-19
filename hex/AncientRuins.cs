using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data;
using System.IO;
using Godot;
using System.Diagnostics.Tracing;

public enum RuinType
{
    city,
    cave,
    anomaly
}

public enum RuinTier
{
    basic, //seen at start of game
    buried, //unlocked at end of classical culture
    hidden //unlocked at end of industrial culture
}
public static class RuinsEventLoader
{
    public static Dictionary<string, RuinsEvent> ruinsEventDict = new();
    public static List<RuinsEvent> eventStartPoints = new();
    
    //so we have an event dictionary
    static RuinsEventLoader()
    {
        RuinsEvent sample_final = new RuinsEvent()
        {
            eventID = "Sample_Final",
            description = "You leave the area moving on.",
        };
        RuinsEvent sample = new RuinsEvent
        {
            eventID = "Sample",
            description = "This is a sample event that leads to a few branching events to reach a unified conclusion.",
            options = new List<EventOption>
            {
                new EventOption
                {
                    optionText = "Do you go left?",
                    nextEvents = new List<RuinsEvent>
                    {
                        new RuinsEvent
                        {
                            eventID = "Sample_2_A",
                            description = "Now that you went left do you want to blow up the wall or leave?",
                            options = new List<EventOption>
                            {
                                new EventOption
                                {
                                    optionText = "Blow up the wall!",
                                    nextEvents = new List<RuinsEvent> {
                                        new RuinsEvent
                                        {
                                            eventID = "Sample_3_AA",
                                            description = "After destroying the wall you find an ancient trove of gems and shiny metals",
                                            eventEffects = (player) =>
                                            {
                                                player.AddGold(50);
                                            },
                                            options = new List<EventOption>
                                            {
                                                new EventOption
                                                {
                                                    nextEvents = new List<RuinsEvent>
                                                    {
                                                        sample_final
                                                    }
                                                }
                                            }
                                        }
                                    }
                                },
                                new EventOption
                                {
                                    optionText = "Run Away!",
                                    nextEvents = new List<RuinsEvent>
                                    {
                                        sample_final
                                    }
                                }
                            }
                        }
                    }
                },
                new EventOption
                {
                    optionText = "Or do you go right?",
                    nextEvents = new List<RuinsEvent>
                    {
                        new RuinsEvent
                        {
                            eventID = "Sample_2_B",
                            description = "You find a group of survivors do you invite them to join your civilization or leave?",
                            options = new List<EventOption>
                            {
                                new EventOption
                                {
                                    optionText = "Invite them to join your Civilization.",
                                    nextEvents = new List<RuinsEvent> {
                                        new RuinsEvent
                                        {
                                            eventID = "Sample_3_B1",
                                            description = "After a short discussion they decide to come live in your capital city.",
                                            eventEffects = (player) =>
                                            {
                                                Global.gameManager.game.cityDictionary[player.cityList[0]].GrowCity();
                                            },
                                            weight = 0.5f,
                                            options = new List<EventOption>
                                            {
                                                new EventOption
                                                {
                                                    nextEvents = new List<RuinsEvent>
                                                    {
                                                        sample_final
                                                    }
                                                }
                                            }
                                        },
                                        new RuinsEvent
                                        {
                                            eventID = "Sample_3_B2",
                                            description = "They decide they would rather risk it on their own.",
                                            weight = 0.5f,
                                            options = new List<EventOption>
                                            {
                                                new EventOption
                                                {
                                                    nextEvents = new List<RuinsEvent>
                                                    {
                                                        sample_final
                                                    }
                                                }
                                            }
                                        }
                                    }
                                },
                                new EventOption
                                {
                                    optionText = "Leave.",
                                    nextEvents = new List<RuinsEvent>
                                    {
                                        sample_final
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
        AddEventsRecursive(sample, ruinsEventDict);
        eventStartPoints.Add(sample);
    }

    static RuinsEvent PickWeightedEvent(List<RuinsEvent> candidates)
    {
        if (candidates == null || candidates.Count == 0) return null;
        if (candidates.Count == 1) return candidates[0];

        float totalWeight = candidates.Sum(e => e.weight);
        float roll = new Random().NextSingle() * totalWeight;

        float cumulative = 0f;
        foreach (var evt in candidates)
        {
            cumulative += evt.weight;
            if (roll <= cumulative)
                return evt;
        }

        return candidates.Last(); // fallback
    }

    static void AddEventsRecursive(RuinsEvent rootEvent, Dictionary<string, RuinsEvent> dict)
    {
        if (string.IsNullOrEmpty(rootEvent.eventID) || dict.ContainsKey(rootEvent.eventID))
            return;

        dict[rootEvent.eventID] = rootEvent;

        foreach (var option in rootEvent.options ?? new List<EventOption>())
        {
            foreach (var nextEvent in option.nextEvents ?? new List<RuinsEvent>())
            {
                AddEventsRecursive(nextEvent, dict);
            }
        }
    }
}
public class AncientRuins
{
    public Hex hex { get; set; }
    public RuinType ruinType { get; set; }
    public RuinTier ruinTier { get; set; }
    public string eventID { get; set; }
    public AncientRuins(Hex hex, RuinType ruinType, RuinTier ruinTier)
    {
        this.ruinType = ruinType;
        this.ruinTier = ruinTier;
        eventID = RuinsEventLoader.eventStartPoints[Random.Shared.Next(RuinsEventLoader.eventStartPoints.Count)].eventID;
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
        {
            //manager.CallDeferred("NewRuins", id); //how do we associate this ancient ruin, i guess pass everything?
        }
    }
    public AncientRuins()
    {
    }

}

public class RuinsEvent
{
    public string eventID { get; set; }
    public string description { get; set; }
    public Action<Player> eventEffects { get; set; }
    public List<EventOption> options { get; set; }
    public float weight { get; set; }

    public RuinsEvent(string eventID, string description, List<EventOption> options)
    {
        this.eventID = eventID;
        this.description = description;
        this.options = options;
    }

    public RuinsEvent() { }
}

public class EventOption
{
    public string optionText { get; set; }
    public List<RuinsEvent> nextEvents { get; set; } = new();

}
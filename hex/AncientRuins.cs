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

public enum RuinEffect
{

}
public static class AncientRuinsLoader
{
    public static Dictionary<string, RuinsEvent> ruinsEventDict = new();
    public static List<RuinsEvent> eventStartPoints = new();    
    static AncientRuinsLoader()
    {
        RuinsEvent sample = new RuinsEvent
        {
            eventID = "Sample",
            title = "Sample Title",
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
                            title = "Sample 2 A Title",
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
                                            title = "Sample 3 AA Title",
                                            description = "After destroying the wall you find an ancient trove of gems and shiny metals",
                                            options = new List<EventOption>
                                            {
                                                new EventOption
                                                {
                                                    optionText = "Done. +50 Gold. (Ends Ruin Event Chain)",
                                                    eventEffects = (player, ancientRuins) =>
                                                    {
                                                        player.AddGold(50);
                                                        ancientRuins.RemoveRuins();
                                                    },
                                                }
                                            }
                                        }
                                    }
                                },
                                new EventOption
                                {
                                    optionText = "Run Away! (Ends Ruin Event Chain)",
                                    eventEffects = (player, ancientRuins) =>
                                    {
                                        ancientRuins.RemoveRuins();
                                    },
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
                            title = "Sample 2 B Title",
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
                                            title = "Sample 3 B1 Title",
                                            description = "After a short discussion they decide to come live in your capital city.",

                                            weight = 0.4f,
                                            options = new List<EventOption>
                                            {
                                                new EventOption
                                                {
                                                    optionText = "Done. +1 Population in Capital City. (Ends Ruin Event Chain)",
                                                    eventEffects = (player, ancientRuins) =>
                                                    {
                                                        Global.gameManager.game.cityDictionary[player.cityList[0]].GrowCity();
                                                        ancientRuins.RemoveRuins();
                                                    },
                                                }
                                            }
                                        },
                                        new RuinsEvent
                                        {
                                            eventID = "Sample_3_B2",
                                            title = "Sample 3 B2 Title",
                                            description = "They turn out to be cannibals and attack your unit.",
                                            weight = 0.6f,
                                            options = new List<EventOption>
                                            {
                                                new EventOption
                                                {
                                                    optionText = "Done. (Ends Ruin Event Chain)",
                                                    eventEffects = (player, ancientRuins) =>
                                                    {
                                                        if(Global.gameManager.game.mainGameBoard.gameHexDict[ancientRuins.hex].units.Any())
                                                        {
                                                            Global.gameManager.game.unitDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[ancientRuins.hex].units[0]].decreaseHealth(75);
                                                        }
                                                        ancientRuins.RemoveRuins();
                                                    },
                                                }
                                            }
                                        }
                                    }
                                },
                                new EventOption
                                {
                                    optionText = "Leave. (Ends Ruin Event Chain)",
                                    eventEffects = (player, ancientRuins) =>
                                    {
                                        ancientRuins.RemoveRuins();
                                    },
                                }
                            }
                        }
                    }
                }
            }
        };
        AddEventsRecursive(sample, ruinsEventDict);
        eventStartPoints.Add(sample);

        //AI EVENTS PLACEHOLDER
        RuinsEvent mysteriousVault = new RuinsEvent
        {
            eventID = "Vault_1",
            title = "The Vault Entrance",
            description = "Deep within the ruins, you discover a sealed vault door etched with symbols no one in your civilization recognizes. A faint hum resonates from within.",
            options = new List<EventOption>
        {
            new EventOption
            {
                optionText = "Attempt to decipher the symbols.",
                nextEvents = new List<RuinsEvent>
                {
                    new RuinsEvent
                    {
                        eventID = "Vault_2_A",
                        title = "Symbol Analysis",
                        description = "Your scholars work tirelessly to decode the symbols. Some resemble constellations, others seem to depict rituals.",
                        options = new List<EventOption>
                        {
                            new EventOption
                            {
                                optionText = "Record findings for future study. +30 Science. (Ends Ruin Event Chain)",
                                eventEffects = (player, ancientRuins) =>
                                {
                                    player.AddScience(30);
                                    ancientRuins.RemoveRuins();
                                },
                            },
                            new EventOption
                            {
                                optionText = "Try activating the vault using the star pattern.",
                                nextEvents = new List<RuinsEvent>
                                {
                                    new RuinsEvent
                                    {
                                        eventID = "Vault_3_AA",
                                        title = "Vault Activation",
                                        description = "The vault opens with a low rumble. Inside lies a chamber filled with glowing tablets and relics.",
                                        options = new List<EventOption>
                                        {
                                            new EventOption
                                            {
                                                optionText = "Study the relics. +20 Culture, +20 Science. (Ends Ruin Event Chain)",
                                                eventEffects = (player, ancientRuins) =>
                                                {
                                                    player.AddCulture(20);
                                                    player.AddScience(20);
                                                    ancientRuins.RemoveRuins();
                                                },
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            },
            new EventOption
            {
                optionText = "Force the vault open.",
                nextEvents = new List<RuinsEvent>
                {
                    new RuinsEvent
                    {
                        eventID = "Vault_2_B",
                        title = "Forced Entry",
                        description = "Your hero uses brute strength and explosives to breach the vault. The explosion reveals a hidden chamber... and triggers a defense mechanism.",
                        options = new List<EventOption>
                        {
                            new EventOption
                            {
                                optionText = "Fight through the decrepit metal monsters.",
                                nextEvents = new List<RuinsEvent>
                                {
                                    new RuinsEvent
                                    {
                                        eventID = "Vault_3_B1",
                                        title = "Victory Over Drones",
                                        description = "Your hero defeats the drones and gains valuable combat experience.",
                                        weight = 0.5f,
                                        options = new List<EventOption>
                                        {
                                            new EventOption
                                            {
                                                optionText = "Done. +100 XP to Hero. (Ends Ruin Event Chain)",
                                                eventEffects = (player, ancientRuins) =>
                                                {
                                                    ((Hero)Global.gameManager.game.unitDictionary[player.ourHeroID]).IncreaseExperience(100);
                                                    ancientRuins.RemoveRuins();
                                                },
                                            }
                                        }
                                    },
                                    new RuinsEvent
                                    {
                                        eventID = "Vault_3_B2",
                                        title = "Ambushed!",
                                        description = "The metal monsters overwhelm your hero, causing severe injuries.",
                                        weight = 0.5f,
                                        options = new List<EventOption>
                                        {
                                            new EventOption
                                            {
                                                optionText = "Done. Hero loses 50 HP. (Ends Ruin Event Chain)",
                                                eventEffects = (player, ancientRuins) =>
                                                {
                                                    ((Hero)Global.gameManager.game.unitDictionary[player.ourHeroID]).decreaseHealth(50);
                                                    ancientRuins.RemoveRuins();
                                                },
                                            }
                                        }
                                    }
                                }
                            },
                            new EventOption
                            {
                                optionText = "Retreat and seal the vault. (Ends Ruin Event Chain)",
                                eventEffects = (player, ancientRuins) =>
                                {
                                    ancientRuins.RemoveRuins();
                                },
                            }
                        }
                    }
                }
            },
            new EventOption
            {
                optionText = "Ignore the vault and explore nearby chambers.",
                nextEvents = new List<RuinsEvent>
                {
                    new RuinsEvent
                    {
                        eventID = "Vault_2_C",
                        title = "Forgotten Chambers",
                        description = "You find a series of rooms filled with murals depicting a civilization worshipping celestial beings. One chamber contains preserved food and tools.",
                        options = new List<EventOption>
                        {
                            new EventOption
                            {
                                optionText = "Salvage supplies. +1 Population, +25 Gold. (Ends Ruin Event Chain)",
                                eventEffects = (player, ancientRuins) =>
                                {
                                    player.AddGold(25);
                                    Global.gameManager.game.cityDictionary[player.cityList[0]].GrowCity();
                                    ancientRuins.RemoveRuins();
                                },
                            },
                            new EventOption
                            {
                                optionText = "Leave the chambers untouched. (Ends Ruin Event Chain)",
                                eventEffects = (player, ancientRuins) =>
                                {
                                    ancientRuins.RemoveRuins();
                                },
                            }
                        }
                    }
                }
            }
        }
        };
        AddEventsRecursive(mysteriousVault, ruinsEventDict);
        eventStartPoints.Add(mysteriousVault);
        
        RuinsEvent dreamingObelisk = new RuinsEvent
        {
            eventID = "DreamObelisk_1",
            title = "The Obelisk of Echoes",
            description = "Your expedition stumbles upon a towering obsidian obelisk humming with energy. As your hero approaches, their vision blurs and a voice—not heard but felt—whispers: 'Choose the echo that defines you.'",
            options = new List<EventOption>
            {
                new EventOption
                {
                    optionText = "Touch the obelisk and embrace the Echo of Wisdom.",
                    nextEvents = new List<RuinsEvent>
                    {
                        new RuinsEvent
                        {
                            eventID = "DreamObelisk_2_A",
                            title = "Echo of Wisdom",
                            description = "Your hero sees visions of ancient scholars debating beneath twin moons. Knowledge floods their mind.",
                            options = new List<EventOption>
                            {
                                new EventOption
                                {
                                    optionText = "Done. Hero gains +50 XP and 3 Turns of Free Science for your Civilization. (Ends Ruin Event Chain)",
                                    eventEffects = (player, ancientRuins) =>
                                    {
                                        ((Hero)Global.gameManager.game.unitDictionary[player.ourHeroID]).IncreaseExperience(50);
                                        player.AddScience(player.GetSciencePerTurn()*3);
                                        ancientRuins.RemoveRuins();
                                    },
                                }
                            }
                        }
                    }
                },
                new EventOption
                {
                    optionText = "Touch the obelisk and embrace the Echo of Power.",
                    nextEvents = new List<RuinsEvent>
                    {
                        new RuinsEvent
                        {
                            eventID = "DreamObelisk_2_B",
                            title = "Echo of Power",
                            description = "Your hero is transported to a battlefield where time stands still. They feel every heartbeat of a long-dead warrior.",
                            options = new List<EventOption>
                            {
                                new EventOption
                                {
                                    optionText = "Done. Hero gains +50 XP and +1 Combat Strength Permanently. (Ends Ruin Event Chain)",
                                    eventEffects = (player, ancientRuins) =>
                                    {
                                        ((Hero)Global.gameManager.game.unitDictionary[player.ourHeroID]).IncreaseExperience(50);
                                        ((Hero)Global.gameManager.game.unitDictionary[player.ourHeroID]).baseCombatStrength += 1;
                                        ((Hero)Global.gameManager.game.unitDictionary[player.ourHeroID]).combatStrength += 1;
                                        ancientRuins.RemoveRuins();
                                    },
                                }
                            }
                        }
                    }
                },
                new EventOption
                {
                    optionText = "Touch the obelisk and embrace the Echo of Memory.",
                    nextEvents = new List<RuinsEvent>
                    {
                        new RuinsEvent
                        {
                            eventID = "DreamObelisk_2_C",
                            title = "Echo of Memory",
                            description = "Your hero relives moments from a civilization that vanished without a trace. A child’s laughter. A sudden silence. A door that never opens again.",
                            options = new List<EventOption>
                            {
                                new EventOption
                                {
                                    optionText = "Done. Hero gains +50 XP and 4 Turns of Culture for your Civilization. (Ends Ruin Event Chain)",
                                    eventEffects = (player, ancientRuins) =>
                                    {
                                        ((Hero)Global.gameManager.game.unitDictionary[player.ourHeroID]).IncreaseExperience(50);
                                        player.AddCulture(player.GetCulturePerTurn()*4);
                                        ancientRuins.RemoveRuins();
                                    },
                                }
                            }
                        }
                    }
                },
                new EventOption
                {
                    optionText = "Leave the obelisk untouched.",
                    eventEffects = (player, ancientRuins) =>
                    {
                        ancientRuins.RemoveRuins();
                    },
                }
            }
        };
        AddEventsRecursive(dreamingObelisk, ruinsEventDict);
        eventStartPoints.Add(dreamingObelisk);
        
        RuinsEvent livingSanctum = new RuinsEvent
        {
            eventID = "Sanctum_1",
            title = "The Breathing Sanctum",
            description = "As your people enter the chamber seems to pulse gently, as if inhaling and exhaling. Vines move without wind. Walls shimmer with bioluminescent veins. A whisper echoes: 'Feed me purpose.'",
            options = new List<EventOption>
            {
                new EventOption
                {
                    optionText = "Offer your hero’s strength.",
                    nextEvents = new List<RuinsEvent>
                    {
                        new RuinsEvent
                        {
                            eventID = "Sanctum_2_A",
                            title = "The Strength Offering",
                            description = "Your hero places their hand on the central altar. The sanctum responds with a surge of energy, testing their resolve.",
                            options = new List<EventOption>
                            {
                                new EventOption
                                {
                                    optionText = "Endure the trial.",
                                    nextEvents = new List<RuinsEvent>
                                    {
                                        new RuinsEvent
                                        {
                                            eventID = "Sanctum_3_A1",
                                            title = "Trial Survived",
                                            description = "Your hero withstands the pain and emerges stronger.",
                                            weight = 0.7f,
                                            options = new List<EventOption>
                                            {
                                                new EventOption
                                                {
                                                    optionText = "Done. Hero gains a level instantly. (Ends Ruin Event Chain)",
                                                    eventEffects = (player, ancientRuins) =>
                                                    {
                                                        ((Hero)Global.gameManager.game.unitDictionary[player.ourHeroID]).IncreaseExperience(((Hero)Global.gameManager.game.unitDictionary[player.ourHeroID]).experienceToLevelUp[((Hero)Global.gameManager.game.unitDictionary[player.ourHeroID]).level]);
                                                        ancientRuins.RemoveRuins();
                                                    },
                                                }
                                            }
                                        },
                                        new RuinsEvent
                                        {
                                            eventID = "Sanctum_3_A2",
                                            title = "Trial Failed",
                                            description = "The sanctum rejects your hero’s offering. They collapse, weakened.",
                                            weight = 0.3f,
                                            options = new List<EventOption>
                                            {
                                                new EventOption
                                                {
                                                    optionText = "Done. Hero loses 30 HP. (Ends Ruin Event Chain)",
                                                    eventEffects = (player, ancientRuins) =>
                                                    {
                                                        ((Hero)Global.gameManager.game.unitDictionary[player.ourHeroID]).decreaseHealth(30);
                                                        ancientRuins.RemoveRuins();
                                                    },
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                new EventOption
                {
                    optionText = "Offer knowledge from your civilization.",
                    nextEvents = new List<RuinsEvent>
                    {
                        new RuinsEvent
                        {
                            eventID = "Sanctum_2_B",
                            title = "The Knowledge Offering",
                            description = "Your scholars inscribe your civilization’s history into the sanctum’s walls. The vines absorb the ink, glowing brighter.",
                            options = new List<EventOption>
                            {
                                new EventOption
                                {
                                    optionText = "Done. Gain 4 turns of Culture, unlocks 'Biomancer' unit type. (Ends Ruin Event Chain)",
                                    eventEffects = (player, ancientRuins) =>
                                    {
                                        player.AddCulture(player.GetCulturePerTurn()*4);
                                        ancientRuins.RemoveRuins();
                                    },
                                }
                            }
                        }
                    }
                },
                new EventOption
                {
                    optionText = "Offer silence. Observe without interference.",
                    nextEvents = new List<RuinsEvent>
                    {
                        new RuinsEvent
                        {
                            eventID = "Sanctum_2_C",
                            title = "The Silent Watch",
                            description = "You sit in stillness. The sanctum reveals a hidden chamber filled with dormant spores and a glowing seed.",
                            options = new List<EventOption>
                            {
                                new EventOption
                                {
                                    optionText = "Take the seed. Gain 2 turns of Science, +50 Gold. (Ends Ruin Event Chain)",
                                    eventEffects = (player, ancientRuins) =>
                                    {
                                        player.AddScience(player.GetSciencePerTurn()*2);
                                        player.AddGold(50);
                                        ancientRuins.RemoveRuins();
                                    },
                                },
                                new EventOption
                                {
                                    optionText = "Leave the seed untouched. (Ends Ruin Event Chain)",
                                    eventEffects = (player, ancientRuins) =>
                                    {
                                        ancientRuins.RemoveRuins();
                                    },
                                }
                            }
                        }
                    }
                },
                new EventOption
                {
                    optionText = "Destroy the sanctum.",
                    nextEvents = new List<RuinsEvent>
                    {
                        new RuinsEvent
                        {
                            eventID = "Sanctum_2_D",
                            title = "Sanctum Destroyed",
                            description = "You burn the vines and collapse the chamber. As the sanctum dies, a final pulse radiates outward.",
                            options = new List<EventOption>
                            {
                                new EventOption
                                {
                                    optionText = "Done. +150 Gold, but hero loses 20 HP. (Ends Ruin Event Chain)",
                                    eventEffects = (player, ancientRuins) =>
                                    {
                                        player.AddGold(150);
                                        ((Hero)Global.gameManager.game.unitDictionary[player.ourHeroID]).decreaseHealth(20);
                                        ancientRuins.RemoveRuins();
                                    },
                                }
                            }
                        }
                    }
                }
            }
        };
        AddEventsRecursive(livingSanctum, ruinsEventDict);
        eventStartPoints.Add(livingSanctum);

        RuinsEvent collapsedObservatory = new RuinsEvent
        {
            eventID = "Observatory_1",
            title = "Collapsed Observatory",
            description = "Your explorers discover the remains of an old observatory buried beneath rubble. Rusted gears, shattered lenses, and scattered star charts hint at a once-thriving scientific outpost.",
            options = new List<EventOption>
            {
                new EventOption
                {
                    optionText = "Salvage the star charts.",
                    nextEvents = new List<RuinsEvent>
                    {
                        new RuinsEvent
                        {
                            eventID = "Observatory_2_A",
                            title = "Chart Recovery",
                            description = "Your team carefully extracts the surviving charts. Some depict constellations no longer visible in your region.",
                            options = new List<EventOption>
                            {
                                new EventOption
                                {
                                    optionText = "Done. +2 Turns of Science. (Ends Ruin Event Chain)",
                                    eventEffects = (player, ancientRuins) =>
                                    {
                                        player.AddScience(player.GetSciencePerTurn()*2);
                                        ancientRuins.RemoveRuins();
                                    },
                                }
                            }
                        }
                    }
                },
                new EventOption
                {
                    optionText = "Search for usable lenses and tools.",
                    nextEvents = new List<RuinsEvent>
                    {
                        new RuinsEvent
                        {
                            eventID = "Observatory_2_B",
                            title = "Optics Salvage",
                            description = "Among the debris, your team finds intact lenses and precision tools that can be repurposed.",
                            options = new List<EventOption>
                            {
                                new EventOption
                                {
                                    optionText = "Done. +50 Gold, +1 Turns of Science. (Ends Ruin Event Chain)",
                                    eventEffects = (player, ancientRuins) =>
                                    {
                                        player.AddGold(50);
                                        player.AddScience(player.GetSciencePerTurn());
                                        ancientRuins.RemoveRuins();
                                    },
                                }
                            }
                        }
                    }
                },
                new EventOption
                {
                    optionText = "Restore the observatory as a cultural site.",
                    nextEvents = new List<RuinsEvent>
                    {
                        new RuinsEvent
                        {
                            eventID = "Observatory_2_C",
                            title = "Restoration Effort",
                            description = "Your workers clear the rubble and rebuild the observatory’s foundation. It becomes a modest landmark for your people.",
                            options = new List<EventOption>
                            {
                                new EventOption
                                {
                                    optionText = "Done. +20 Culture, Create Observatory on this tile (NOT IMPLEMENTED, instead +1 Population in nearest city.) (Ends Ruin Event Chain)",
                                    eventEffects = (player, ancientRuins) =>
                                    {
                                        player.AddCulture(20);
                                        Global.gameManager.game.cityDictionary[player.cityList[0]].GrowCity();
                                        //add observatory 
                                        ancientRuins.RemoveRuins();
                                    },
                                }
                            }
                        }
                    }
                },
                new EventOption
                {
                    optionText = "Leave the observatory untouched. (Ends Ruin Event Chain)",
                    eventEffects = (player, ancientRuins) =>
                    {
                        ancientRuins.RemoveRuins();
                    },
                }
            }
        };

    }

    public static RuinsEvent PickWeightedEvent(List<RuinsEvent> candidates, AncientRuins ancientRuins)
    {
        if (candidates == null || candidates.Count == 0) return null;
        if (candidates.Count == 1) return candidates[0];

        float totalWeight = candidates.Sum(e => e.weight);
        // we use the q,r,and turn to make a random seed the same on all machines
        float roll = new Random(ancientRuins.hex.q + ancientRuins.hex.r + Global.gameManager.game.turnManager.currentTurn).NextSingle() * totalWeight; 
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
    public string eventID { get; set; }
    public string nextEventID { get; set; }
    public bool activeEvent { get; set; } = false;
    public int activeEventTeamNum { get; set; }
    public AncientRuins(Hex hex, string eventID)
    {
        this.hex = hex;
        eventID = AncientRuinsLoader.eventStartPoints[Random.Shared.Next(AncientRuinsLoader.eventStartPoints.Count)].eventID;
        nextEventID = eventID;
        Global.gameManager.game.mainGameBoard.gameHexDict[hex].ancientRuins = this;
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
        {
            var data = new Godot.Collections.Dictionary
            {
                { "q", hex.q },
                { "r", hex.r },
                { "s", hex.s }
            };
            manager.CallDeferred("NewRuins", data);
        }
    }


    public AncientRuins()
    {
    }

    public void RemoveRuins()
    {
        Global.gameManager.game.mainGameBoard.gameHexDict[hex].ancientRuins = null;
        if (Global.gameManager.TryGetGraphicManager(out GraphicManager manager))
        {
            var data = new Godot.Collections.Dictionary
            {
                { "q", hex.q },
                { "r", hex.r },
                { "s", hex.s }
            };
            manager.CallDeferred("RemoveRuins", data);
        }
    }

}

public class RuinsEvent
{
    public string eventID { get; set; }
    public string title { get; set; }
    public string description { get; set; }
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
    public Action<Player, AncientRuins> eventEffects { get; set; }
    public List<RuinsEvent> nextEvents { get; set; } = new();

}
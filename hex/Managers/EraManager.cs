using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum EraType
{
    Classical,
    Revolutionary,
    Modern
}
[Serializable]
public class EraManager
{
    public EraType currentEra { get; set; }
    public int highestClassicalTier { get; set; } = 8;
    public int highestRevolutionaryTier { get; set; } = 17;
    public int highestModernTier { get; set; } = 18;
    public int revolutionaryEraResearchCount { get; set; }
    public int modernEraResearchCount { get; set; }
    public Dictionary<EraType, float> eraResearchCostModifier { get; set; } = new();
    public EraManager()
    {
        currentEra = EraType.Classical;
        eraResearchCostModifier.Add(EraType.Classical, 1.0f);
        eraResearchCostModifier.Add(EraType.Revolutionary, 100.0f);
        eraResearchCostModifier.Add(EraType.Modern, 100.0f);
    }
    public void OnTurnStarted()
    {
        int newEraNeeded = 0;
        int currentWeight = 0;
        int majorPlayerCount = 0;

        foreach (Player player in Global.gameManager.game.playerDictionary.Values)
        {
            if(!FactionLoader.IsFactionMinor(player.faction))
            {
                majorPlayerCount++;
            }
        }

        //eventually add a turn based weighting, to speed up or slow down progress
        //newEraNeeded += 100;
        //currentWeight += Global.gameManager.game.turnManager.currentTurn;

        //each player contributes 10 weight to the era progression requirement
        newEraNeeded += majorPlayerCount * 10;

        //each final tech tree research adds 10 to current weight
        if (currentEra == EraType.Classical)
        {
            currentWeight += revolutionaryEraResearchCount * 10;
        }
        else if(currentEra == EraType.Revolutionary)
        {
            currentWeight += modernEraResearchCount * 10;
        }
        //if everybody has reached the final research or somebody has done it multiple times
        if (currentWeight > newEraNeeded)
        {
            //NEW ERA, Classical -> Revolutionary
            if (currentEra == EraType.Classical)
            {
                currentEra = EraType.Revolutionary;
                eraResearchCostModifier[EraType.Classical] = 0.3333f;
                eraResearchCostModifier[EraType.Revolutionary] = 1.0f;
            }
            //NEW ERA, Revolutionary -> Modern
            else if (currentEra == EraType.Revolutionary)
            {
                currentEra = EraType.Modern;
                eraResearchCostModifier[EraType.Classical] = 0.1f;
                eraResearchCostModifier[EraType.Revolutionary] = 0.33333f;
                eraResearchCostModifier[EraType.Modern] = 1.0f;
            }
            ResearchLoader.UpdateResearchCosts();
            CultureResearchLoader.UpdateResearchCosts();
        }
    }
}

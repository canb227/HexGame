using Godot;
using System;
using System.Collections.Generic;
using System.IO;
public enum DiplomaticState
{
    War,
    Peace,
    ForcedPeace,
    Ally
}
[Serializable]
public class TeamManager
{
    public Dictionary<int, Dictionary<int, int>> relationships { get; set; } = new Dictionary<int, Dictionary<int, int>>();
    public Dictionary<int, Dictionary<int, DiplomaticState>> diplomaticStates { get; set; } = new();
    public Dictionary<int, DiplomacyDeal> pendingDeals { get; set; } = new();

    public Dictionary<TradeDealKey, GoldPerTurnDeal> goldTradeDealDict { get; set; } = new();

    public void OnTurnStarted()
    {
        foreach(var goldTradeDealKey in goldTradeDealDict.Keys)
        {
            var valueTuple = goldTradeDealDict[goldTradeDealKey];
            goldTradeDealDict[goldTradeDealKey] = new GoldPerTurnDeal { amount = valueTuple.amount, turnsLeft = valueTuple.turnsLeft - 1 };
            if (goldTradeDealDict[goldTradeDealKey].turnsLeft <= 0)
            {
                goldTradeDealDict.Remove(goldTradeDealKey);
            }
        }
    }

    public void AddTeam(int newTeamNum, int defaultRelationship)
    {
        Dictionary<int, int> newTeam = new();
        Dictionary<int, DiplomaticState> newStates = new();
        foreach(int oldTeamNum in relationships.Keys)
        {
            relationships[oldTeamNum].Add(newTeamNum, defaultRelationship);
            diplomaticStates[oldTeamNum].Add(newTeamNum, DiplomaticState.Peace);

            newTeam.Add(oldTeamNum, defaultRelationship);
            newStates.Add(oldTeamNum, DiplomaticState.Peace);
        }
        relationships.Add(newTeamNum, newTeam);
        diplomaticStates.Add(newTeamNum, newStates);
    }
    public void SetRelationship(int team1, int team2, int relationship)
    {
        if (!relationships.ContainsKey(team1) || !relationships.ContainsKey(team2))
        {
            throw new Exception("One or both teams do not exist.");
        }

        relationships[team1][team2] = relationship;


        //relationships[team2][team1] = relationship; //for symmetric relationships
    }

    public void SetDiplomaticState(int team1, int team2, DiplomaticState diplomaticState)
    {
        if (!relationships.ContainsKey(team1) || !relationships.ContainsKey(team2))
        {
            throw new Exception("One or both teams do not exist.");
        }

        diplomaticStates[team1][team2] = diplomaticState;
        diplomaticStates[team2][team1] = diplomaticState;
    }

    public DiplomaticState GetDiplomaticState(int team1, int team2)
    {
        if (diplomaticStates.ContainsKey(team1) && diplomaticStates[team1].ContainsKey(team2))
        {
            return diplomaticStates[team1][team2];
        }

        return DiplomaticState.War; // Default state for missing teams
    }

    public int GetRelationship(int team1, int team2)
    {
        if (relationships.ContainsKey(team1) && relationships[team1].ContainsKey(team2))
        {
            return relationships[team1][team2];
        }

        return -99; // Default relationship
    }


    public List<int> GetAllies(int teamId)
    {
        var allies = new List<int>();

        if (diplomaticStates.ContainsKey(teamId))
        {
            Dictionary<int, DiplomaticState> diplomaticStatesDict = diplomaticStates[teamId];
            foreach (int relationshipID in diplomaticStatesDict.Keys)
            {
                if (diplomaticStatesDict[relationshipID] == DiplomaticState.Ally)
                {
                    allies.Add(relationshipID);
                }
            }
        }

        return allies;
    }

    public List<int> GetNeutralPlayers(int teamId)
    {
        var neutrals = new List<int>();

        if (diplomaticStates.ContainsKey(teamId))
        {
            Dictionary<int, DiplomaticState> diplomaticStatesDict = diplomaticStates[teamId];
            foreach (int relationshipID in diplomaticStatesDict.Keys)
            {
                if (diplomaticStatesDict[relationshipID] == DiplomaticState.Peace || diplomaticStatesDict[relationshipID] == DiplomaticState.ForcedPeace)
                {
                    neutrals.Add(relationshipID);
                }
            }
        }

        return neutrals;
    }

    public List<int> GetEnemies(int teamId)
    {
        var enemies = new List<int>();

        if (diplomaticStates.ContainsKey(teamId))
        {
            Dictionary<int, DiplomaticState> diplomaticStatesDict = diplomaticStates[teamId];
            foreach (int relationshipID in diplomaticStatesDict.Keys)
            {
                if (diplomaticStatesDict[relationshipID] == DiplomaticState.War)
                {
                    enemies.Add(relationshipID);
                }
            }
        }

        return enemies;
    }

    public void AddPendingDeal(DiplomacyDeal deal)
    {
        Global.gameManager.game.teamManager.pendingDeals.Add(deal.id, deal);
        if(deal.toTeamNum == Global.gameManager.game.localPlayerTeamNum)
        {
            Global.gameManager.graphicManager.uiManager.CallDeferred("NewDiplomaticDeal", deal);
        }
    }

    public void RemoveDeal(int dealID)
    {
        Global.gameManager.game.teamManager.pendingDeals.Remove(dealID);
    }

    public void ExecuteDeal(int id)
    {
        foreach (DiplomacyAction action in Global.gameManager.game.teamManager.pendingDeals[id].requestsList)
        {
            action.ActivateAction();
        }
        foreach (DiplomacyAction action in Global.gameManager.game.teamManager.pendingDeals[id].offersList)
        {
            action.ActivateAction();
        }
        Global.gameManager.RemovePendingDeal(id);
    }
}

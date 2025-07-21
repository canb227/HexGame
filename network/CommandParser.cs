using NetworkMessages;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

    public static class CommandParser
    {


    public const bool COMMANDDEBUG = true;
    public static bool LOOKATME = true;
    static CommandParser()
    {
        if (COMMANDDEBUG) { Global.Log("Configuring CommandParser event capture."); }
        NetworkPeer.CommandMessageReceivedEvent += OnCommandMessageReceived;
    }

    private static void OnCommandMessageReceived(Command command)
    {
        //if (COMMANDDEBUG) { Global.Log("CmdMsgEvent!"); }
        string prefix = "";
        if (command.Sender == Global.clientID)
        {
            prefix = "[LOCAL]";
        }
        else
        {
            prefix = "[NETWORK][" + command.Sender + "]";
        }
        
        switch (command.CommandType)
        {
            case "ActivateAbility":
                if (COMMANDDEBUG) { Global.Log(prefix + "ActivateAbility Command received: UnitId: " + command.ActivateAbility.UnitId + ", AbilityName: " + command.ActivateAbility.AbilityName + ", Target: (" + command.ActivateAbility.Target.Q + ", " + command.ActivateAbility.Target.R + ", " + command.ActivateAbility.Target.S + ")");}
                Global.gameManager.ActivateAbility((int)command.ActivateAbility.UnitId, command.ActivateAbility.AbilityName, new Hex((int)command.ActivateAbility.Target.Q, (int)command.ActivateAbility.Target.R, (int)command.ActivateAbility.Target.S), false);
                break;
            case "MoveUnit":
                if (COMMANDDEBUG) { Global.Log(prefix + "MoveUnit Command received: UnitId: " + command.MoveUnit.UnitId + ", unit location: " + Global.gameManager.game.unitDictionary[command.MoveUnit.UnitId].hex + " Target: (" + command.MoveUnit.Target.Q + ", " + command.MoveUnit.Target.R + ", " + command.MoveUnit.Target.S + "), IsEnemy: " + command.MoveUnit.IsEnemy); }
                Global.gameManager.MoveUnit((int)command.MoveUnit.UnitId, new Hex((int)command.MoveUnit.Target.Q, (int)command.MoveUnit.Target.R, (int)command.MoveUnit.Target.S), command.MoveUnit.IsEnemy, false);
                break;
            case "AddToProductionQueue":
                if (COMMANDDEBUG) { Global.Log(prefix + "AddToProductionQueue Command received: CityID: " + command.AddToProductionQueue.CityID + ", ItemName: " + command.AddToProductionQueue.ItemName + ", Target: (" + command.AddToProductionQueue.Target.Q + ", " + command.AddToProductionQueue.Target.R + ", " + command.AddToProductionQueue.Target.S + "), Front: " + command.AddToProductionQueue.Front); }
                Global.gameManager.AddToProductionQueue((int)command.AddToProductionQueue.CityID, command.AddToProductionQueue.ItemName, new Hex((int)command.AddToProductionQueue.Target.Q, (int)command.AddToProductionQueue.Target.R, (int)command.AddToProductionQueue.Target.S), command.AddToProductionQueue.Front, false);
                break;  
            case "RemoveFromProductionQueue":
                if (COMMANDDEBUG) { Global.Log(prefix + "RemoveFromProductionQueue Command received: CityID: " + command.RemoveFromProductionQueue.CityID + ", Index: " + command.RemoveFromProductionQueue.Index); }
                Global.gameManager.RemoveFromProductionQueue((int)command.RemoveFromProductionQueue.CityID, (int)command.RemoveFromProductionQueue.Index, false);
                break;
            case "MoveToFrontOfProductionQueue":
                if (COMMANDDEBUG) { Global.Log(prefix + "MoveToFrontOfProductionQueue Command received: CityID: " + command.MoveToFrontOfProductionQueue.CityID + ", Index: " + command.MoveToFrontOfProductionQueue.Index); }
                Global.gameManager.MoveToFrontOfProductionQueue((int)command.MoveToFrontOfProductionQueue.CityID, (int)command.MoveToFrontOfProductionQueue.Index, false);
                break;
            case "ExpandToHex":
                if (COMMANDDEBUG) { Global.Log(prefix + "ExpandToHex Command received: CityID: " + command.ExpandToHex.CityID + ", Target: (" + command.ExpandToHex.Target.Q + ", " + command.ExpandToHex.Target.R + ", " + command.ExpandToHex.Target.S + ")"); }
                Global.gameManager.ExpandToHex((int)command.ExpandToHex.CityID, new Hex((int)command.ExpandToHex.Target.Q, (int)command.ExpandToHex.Target.R, (int)command.ExpandToHex.Target.S), false);
                break;
            case "DevelopDistrict":
                if (COMMANDDEBUG) { Global.Log(prefix + "DevelopDistrict Command received: CityID: " + command.DevelopDistrict.CityID + ", Target: (" + command.DevelopDistrict.Target.Q + ", " + command.DevelopDistrict.Target.R + ", " + command.DevelopDistrict.Target.S + "), DistrictType: " + command.DevelopDistrict.DistrictType); }
                Global.gameManager.DevelopDistrict((int)command.DevelopDistrict.CityID, new Hex((int)command.DevelopDistrict.Target.Q, (int)command.DevelopDistrict.Target.R, (int)command.DevelopDistrict.Target.S), (DistrictType)command.DevelopDistrict.DistrictType, false);
                break;
            case "RenameCity":
                if (COMMANDDEBUG) { Global.Log(prefix + "RenameCity Command received: CityID: " + command.RenameCity.CityID + ", Name: " + command.RenameCity.Name); }
                Global.gameManager.RenameCity((int)command.RenameCity.CityID, command.RenameCity.Name, false);
                break;
            case "SelectResearch":
                if (COMMANDDEBUG) { Global.Log(prefix + "SelectResearch Command received: TeamNum: " + command.SelectResearch.TeamNum + ", ResearchName: " + command.SelectResearch.ResearchName); }
                Global.gameManager.SelectResearch((int)command.SelectResearch.TeamNum,command.SelectResearch.ResearchName, false);
                break;
            case "SelectCulture":
                if (COMMANDDEBUG) { Global.Log(prefix + "SelectCulture Command received: TeamNum: " + command.SelectCulture.TeamNum + ", CultureName: " + command.SelectCulture.CultureName); }
                Global.gameManager.SelectCulture((int)command.SelectCulture.TeamNum, command.SelectCulture.CultureName, false);
                break;
            case "AddResourceAssignment":
                if (COMMANDDEBUG) { Global.Log(prefix + "AddResourceAssignment Command received: CityID: " + command.AddResourceAssignment.CityID + ", ResourceName: " + command.AddResourceAssignment.ResourceName + ", SourceHex: (" + command.AddResourceAssignment.SourceHex.Q + ", " + command.AddResourceAssignment.SourceHex.R + ", " + command.AddResourceAssignment.SourceHex.S + ")"); }
                Global.gameManager.AddResourceAssignment((int)command.AddResourceAssignment.CityID, (ResourceType)command.AddResourceAssignment.ResourceName, new Hex((int)command.AddResourceAssignment.SourceHex.Q, (int)command.AddResourceAssignment.SourceHex.R, (int)command.AddResourceAssignment.SourceHex.S), false);
                break;
            case "RemoveResourceAssignment":
                if (COMMANDDEBUG) { Global.Log(prefix + "RemoveResourceAssignment Command received: TeamNum: " + command.RemoveResourceAssignment.TeamNum + ", SourceHex: (" + command.RemoveResourceAssignment.SourceHex.Q + ", " + command.RemoveResourceAssignment.SourceHex.R + ", " + command.RemoveResourceAssignment.SourceHex.S + ")"); }
                Global.gameManager.RemoveResourceAssignment((int)command.RemoveResourceAssignment.TeamNum, new Hex((int)command.RemoveResourceAssignment.SourceHex.Q, (int)command.RemoveResourceAssignment.SourceHex.R, (int)command.RemoveResourceAssignment.SourceHex.S), false);
                break;
            case "EndTurn":
                if (COMMANDDEBUG) { Global.Log(prefix + "EndTurn Command received: TeamNum: " + command.EndTurn.TeamNum); }
                Global.gameManager.EndTurn((int)command.EndTurn.TeamNum, false);
                break;
            case "ExecutePendingDeal":
                if (COMMANDDEBUG) { Global.Log(prefix + $"Execute Pending Deal Command received from {command.Sender} to Execute Deal: {command.ExecutePendingDeal.DealID}"); }
                Global.gameManager.ExecutePendingDeal(command.ExecutePendingDeal.DealID,false);
                break;
            case "RemovePendingDeal":
                if (COMMANDDEBUG) { Global.Log(prefix + $"Remove Pending Deal Command received from {command.Sender} to Remove Deal: {command.RemovePendingDeal.DealID}"); }
                Global.gameManager.RemovePendingDeal(command.RemovePendingDeal.DealID, false);
                break;
            case "AddPendingDeal":
                if (COMMANDDEBUG) { Global.Log(prefix + $"Add Pending Deal Command received from {command.Sender} to Add Deal: {command.AddPendingDeal.DealID}"); }
                List<DiplomacyAction> requests = new();
                foreach (DiplomaticActionMessage request in command.AddPendingDeal.RequestedActions)
                {
                    requests.Add(DiplomaticActionMessageToDiplomacyAction(request));
                }
                List<DiplomacyAction> offers = new();
                foreach (DiplomaticActionMessage offer in command.AddPendingDeal.OfferedActions)
                {
                    offers.Add(DiplomaticActionMessageToDiplomacyAction(offer));
                }
                Global.gameManager.AddPendingDeal(command.AddPendingDeal.DealID, command.AddPendingDeal.FromTeamNum, command.AddPendingDeal.ToTeamNum, requests, offers, false);
                break;
            case "NewExportRoute":
                if (COMMANDDEBUG) { Global.Log(prefix + $"New Export Route Command received from {command.Sender} to Add Route: (ToCity:{command.NewExportRoute.ToCityID}, FromCity: {command.NewExportRoute.FromCityID}, YieldType: {command.NewExportRoute.YieldType})"); }
                Global.gameManager.NewExportRoute(command.NewExportRoute.FromCityID, command.NewExportRoute.ToCityID, (YieldType)command.NewExportRoute.YieldType,false);
                break;
            case "RemoveExportRoute":
                if (COMMANDDEBUG) { Global.Log(prefix + $"Remove Export Route Command received from {command.Sender} to Remove Route: (ToCity:{command.RemoveExportRoute.ToCityID}, FromCity: {command.RemoveExportRoute.FromCityID}, YieldType: {command.RemoveExportRoute.YieldType})"); }
                Global.gameManager.RemoveExportRoute(command.RemoveExportRoute.FromCityID, command.RemoveExportRoute.ToCityID, (YieldType)command.RemoveExportRoute.YieldType,false);
                break;
            case "NewTradeRoute":
                if (COMMANDDEBUG) { Global.Log(prefix + $"New Trade Route Command received from {command.Sender} to Add Route: (ToCity:{command.NewTradeRoute.ToCityID}, FromCity: {command.NewTradeRoute.FromCityID})"); }
                Global.gameManager.NewTradeRoute(command.NewTradeRoute.FromCityID, command.NewTradeRoute.ToCityID,false);
                break;
            case "RemoveTradeRoute":
                if (COMMANDDEBUG) { Global.Log(prefix + $"Remove Trade Route Command received from {command.Sender} to Remove Route: (ToCity:{command.RemoveTradeRoute.ToCityID}, FromCity: {command.RemoveTradeRoute.FromCityID})"); }
                Global.gameManager.RemoveTradeRoute(command.RemoveTradeRoute.FromCityID, command.RemoveTradeRoute.ToCityID,false);
                break;
            case "AssignPolicyCard":
                if (COMMANDDEBUG) { Global.Log(prefix + $"Assign Policy Card Command received from {command.Sender} to Assign cardID: {command.AssignPolicyCard.PolicyCardID}, for teamnum: {command.AssignPolicyCard.TeamNum}"); }
                Global.gameManager.AssignPolicyCard(command.AssignPolicyCard.TeamNum, command.AssignPolicyCard.PolicyCardID,false);
                break;
            case "UnassignPolicyCard":
                if (COMMANDDEBUG) { Global.Log(prefix + $"Unassign Policy Card Command received from {command.Sender} to Uassign cardID: {command.UnassignPolicyCard.PolicyCardID}, for teamnum: {command.UnassignPolicyCard.TeamNum}"); }
                Global.gameManager.UnassignPolicyCard(command.UnassignPolicyCard.TeamNum, command.UnassignPolicyCard.PolicyCardID,false);
                break;
            case "SetGovernment":
                if (COMMANDDEBUG) { Global.Log(prefix + $"Set Government Command received from {command.Sender} to set govtype: {Enum.GetNames(typeof(GovernmentType))[command.SetGovernment.GovType]}, for teamnum: {command.SetGovernment.TeamNum}"); }
                Global.gameManager.SetGovernment(command.SetGovernment.TeamNum, (GovernmentType)command.SetGovernment.GovType,false);
                break;
            case "SpawnUnit":
                if (COMMANDDEBUG)
                {
                    Global.Log(prefix + $"SpawnUnit command received from {command.Sender} to spawn unit (ID:{command.SpawnUnit.Id}) of type {command.SpawnUnit.UnitType} for team {command.SpawnUnit.TeamNum} at location {command.SpawnUnit.Position}");
                }
                Global.gameManager.SpawnUnit(command.SpawnUnit.UnitType, command.SpawnUnit.TeamNum, new Hex(command.SpawnUnit.Position.Q,command.SpawnUnit.Position.R,command.SpawnUnit.Position.S), command.SpawnUnit.Stackable, command.SpawnUnit.Flexible, false);
                break;
            case "SetDiplomaticState":
                //testing
                Global.gameManager.SetDiplomaticState(command.SetDiplomaticState.TeamNumOne, command.SetDiplomaticState.TeamNumTwo, (DiplomaticState)command.SetDiplomaticState.DiplomaticState, false);
                break;
            default:
                Global.Log($"The command type {command.CommandType} is not supported. This breaks gamestate so hes dead jim.");
                throw new Exception(prefix + $"The command type {command.CommandType} is not supported. This breaks gamestate so hes dead jim.");
                break;
        }
    }

    public static Command ConstructMoveUnitCommand(int unitID, Hex target, bool isEnemy)
    {
        MoveUnit moveUnit = new MoveUnit();
        moveUnit.UnitId = unitID;
        moveUnit.Target = new NetworkMessages.Hex();
        moveUnit.Target.Q = target.q;
        moveUnit.Target.R = target.r;
        moveUnit.Target.S = target.s;
        moveUnit.IsEnemy = isEnemy;

        Command command = new();
        command.CommandType = "MoveUnit";
        command.MoveUnit = moveUnit;
        command.Sender = Global.clientID;
        return command;
    }

    public static Command ConstructActivateAbilityCommand(int unitID, string abilityName, Hex target) 
    {
        ActivateAbility activateAbility = new ActivateAbility();
        activateAbility.UnitId = unitID;
        activateAbility.AbilityName = abilityName;
        activateAbility.Target = new NetworkMessages.Hex();
        activateAbility.Target.Q = target.q;
        activateAbility.Target.R = target.r;
        activateAbility.Target.S = target.s;

        Command command = new();
        command.CommandType = "ActivateAbility";
        command.ActivateAbility = activateAbility;
        command.Sender = Global.clientID;
        return command;
    }

    internal static Command ConstructAddToProductionQueueCommand(int cityID, string name, Hex targetHex, bool front)
    {
        AddToProductionQueue addToProductionQueue = new AddToProductionQueue();
        addToProductionQueue.ItemName = name;
        addToProductionQueue.CityID = cityID;
        addToProductionQueue.Target = new NetworkMessages.Hex();
        addToProductionQueue.Target.Q = targetHex.q;
        addToProductionQueue.Target.R = targetHex.r;
        addToProductionQueue.Target.S = targetHex.s;
        addToProductionQueue.Front = front;

        Command command = new Command();
        command.CommandType = "AddToProductionQueue";
        command.AddToProductionQueue = addToProductionQueue;
        command.Sender = Global.clientID;
        return command;
    }

    internal static Command ConstructRemoveFromProductionQueueCommand(int cityID, int index)
    {
        RemoveFromProductionQueue removeFromProductionQueue = new RemoveFromProductionQueue();
        removeFromProductionQueue.CityID = cityID;
        removeFromProductionQueue.Index = index;


        Command command = new Command();
        command.CommandType = "RemoveFromProductionQueue";
        command.RemoveFromProductionQueue = removeFromProductionQueue;
        command.Sender = Global.clientID;
        return command;
    }

    internal static Command ConstructMoveToFrontOfProductionQueueCommand(int cityID, int index)
    {
        MoveToFrontOfProductionQueue moveToFrontOfProductionQueue = new MoveToFrontOfProductionQueue();
        moveToFrontOfProductionQueue.CityID = cityID;
        moveToFrontOfProductionQueue.Index = index;

        Command command = new Command();
        command.CommandType = "MoveToFrontOfProductionQueue";
        command.MoveToFrontOfProductionQueue = moveToFrontOfProductionQueue;
        command.Sender = Global.clientID;
        return command;
    }

    internal static Command ConstructExpandToHexCommand(int cityID, Hex target)
    {
        ExpandToHex expandToHex = new ExpandToHex();
        expandToHex.CityID = cityID;
        expandToHex.Target = new NetworkMessages.Hex();
        expandToHex.Target.Q = target.q;
        expandToHex.Target.R = target.r;
        expandToHex.Target.S = target.s;

        Command command = new Command();
        command.CommandType = "ExpandToHex";
        command.ExpandToHex = expandToHex;
        command.Sender = Global.clientID;
        return command;
    }

    internal static Command ConstructDevelopDistrictCommand(int cityID, Hex target, DistrictType districtType)
    {
        DevelopDistrict developDistrict = new DevelopDistrict();
        developDistrict.CityID = cityID;
        developDistrict.Target = new NetworkMessages.Hex();
        developDistrict.Target.Q = target.q;
        developDistrict.Target.R = target.r;
        developDistrict.Target.S = target.s;
        developDistrict.DistrictType = (int)districtType;
        
        Command command = new Command();
        command.CommandType = "DevelopDistrict";
        command.DevelopDistrict = developDistrict;
        command.Sender = Global.clientID;
        return command;
    }

    internal static Command ConstructRenameCityCommand(int cityID, string name)
    {
        RenameCity city = new RenameCity();
        city.CityID = cityID;
        city.Name = name;

        Command command = new Command();
        command.CommandType = "RenameCity";
        command.RenameCity = city;
        command.Sender = Global.clientID;
        return command;
    }

    internal static Command ConstructSelectResearchCommand(int teamNum, string researchName)
    {
        SelectResearch selectResearch = new SelectResearch();
        selectResearch.TeamNum = teamNum;
        selectResearch.ResearchName = researchName;

        Command command = new Command();
        command.CommandType = "SelectResearch";
        command.SelectResearch = selectResearch;
        command.Sender = Global.clientID;
        return command;

    }

    internal static Command ConstructSelectCultureCommand(int teamNum, string cultureName)
    {
        SelectCulture culture = new SelectCulture();
        culture.TeamNum = teamNum;
        culture.CultureName = cultureName;

        Command command = new Command();
        command.CommandType = "SelectCulture";
        command.SelectCulture = culture;
        command.Sender = Global.clientID;
        return command;
    }

    internal static Command ConstructAddResourceAssignmentCommand(int cityID, ResourceType resourceType, Hex sourceHex)
    {
        AddResourceAssignment addResourceAssignment = new AddResourceAssignment();
        addResourceAssignment.CityID = cityID;
        addResourceAssignment.ResourceName = (int)resourceType;
        addResourceAssignment.SourceHex = new NetworkMessages.Hex();
        addResourceAssignment.SourceHex.Q = sourceHex.q;
        addResourceAssignment.SourceHex.R = sourceHex.r;
        addResourceAssignment.SourceHex.S = sourceHex.s;

        Command command = new Command();
        command.CommandType = "AddResourceAssignment";
        command.AddResourceAssignment = addResourceAssignment;
        command.Sender = Global.clientID;
        return command;


    }

    internal static Command ConstructRemoveResourceAssignmentCommand(int teamNum, Hex sourceHex)
    {
        RemoveResourceAssignment removeResourceAssignment = new RemoveResourceAssignment();
        removeResourceAssignment.TeamNum = teamNum;
        removeResourceAssignment.SourceHex = new NetworkMessages.Hex();
        removeResourceAssignment.SourceHex.Q = sourceHex.q;
        removeResourceAssignment.SourceHex.R = sourceHex.r;
        removeResourceAssignment.SourceHex.S = sourceHex.s;

        Command command = new Command();
        command.CommandType = "RemoveResourceAssignment";
        command.RemoveResourceAssignment = removeResourceAssignment;
        command.Sender = Global.clientID;
        return command;

    }

    internal static Command ConstructEndTurnCommand(int teamNum)
    {
        EndTurn endTurn = new EndTurn();
        endTurn.TeamNum = teamNum;

        Command command = new Command();
        command.CommandType = "EndTurn";
        command.EndTurn = endTurn;
        command.Sender = Global.clientID;
        return command;
    }

    internal static Command ConstructExecutePendingDealCommand(int dealID)
    {
        ExecutePendingDeal execute = new();
        execute.DealID = dealID;

        Command command = new Command();
        command.CommandType = "ExecutePendingDeal";
        command.ExecutePendingDeal = execute;
        command.Sender = Global.clientID;
        return command;
    }

    internal static Command ConstructRemovePendingDealCommand(int dealID)
    {
        RemovePendingDeal remove = new();
        remove.DealID = dealID;

        Command command = new Command();
        command.CommandType = "RemovePendingDeal";
        command.RemovePendingDeal = remove;
        command.Sender = Global.clientID;
        return command;
    }

    internal static Command ConstructAddPendingDealCommand(int dealID, int fromTeamNum, int toTeamNum, List<DiplomacyAction> requests, List<DiplomacyAction> offers)
    {
        AddPendingDeal add = new();
        add.DealID = dealID;
        add.FromTeamNum = fromTeamNum;
        add.ToTeamNum = toTeamNum;

        foreach (DiplomacyAction action in requests)
        {
            add.RequestedActions.Add(DiplomacyActionToDiplomaticActionMessage(action));
        }

        foreach (DiplomacyAction action in offers)
        {
            add.OfferedActions.Add(DiplomacyActionToDiplomaticActionMessage(action));
        }


        Command command = new Command();
        command.CommandType = "AddPendingDeal";
        command.AddPendingDeal = add;
        command.Sender = Global.clientID;
        return command;
    }

    public static DiplomaticActionMessage DiplomacyActionToDiplomaticActionMessage(DiplomacyAction action)
    {
        DiplomaticActionMessage message = new DiplomaticActionMessage();
        message.FromTeamNum = action.teamNum;
        message.ToTeamNum = action.targetTeamNum;
        message.ActionName = action.actionName;
        if (action.hasDuration)
        {
            message.Duration = action.duration;
        }
        if (action.hasQuantity)
        {
            message.Quantity = action.quantity;
        }
        return message;
    }

    public static DiplomacyAction DiplomaticActionMessageToDiplomacyAction(DiplomaticActionMessage message)
    {
        DiplomacyAction action = new();
        action.actionName = message.ActionName;
        action.teamNum = message.FromTeamNum;
        action.targetTeamNum = message.ToTeamNum;
        if (action.hasDuration = message.HasDuration)
        {
            action.duration = message.Duration;
        }
        if (action.hasQuantity = message.HasQuantity)
        {
            action.quantity = message.Quantity;
        }
        return action;
    }

    internal static Command ConstructSetGovernmentCommand(int teamNum, GovernmentType govType)
    {
        SetGovernment setGovernment = new SetGovernment();
        setGovernment.TeamNum = teamNum;
        setGovernment.GovType = (int)govType;

        Command command = new Command();
        command.CommandType = "SetGovernment";
        command.SetGovernment = setGovernment;
        command.Sender = Global.clientID;
        return command;

    }

    internal static Command ConstructAssignPolicyCardCommand(int teamNum, int policyCardID)
    {
        AssignPolicyCard assignPolicyCard = new AssignPolicyCard();
        assignPolicyCard.TeamNum = teamNum;
        assignPolicyCard.PolicyCardID  = policyCardID;

        Command command = new Command();
        command.CommandType = "AssignPolicyCard";
        command.AssignPolicyCard = assignPolicyCard;
        command.Sender = Global.clientID;
        return command;
    }

    internal static Command ConstructRemoveTradeRouteCommand(int fromCityID, int toCityID)
    {
        RemoveTradeRoute removeTradeRoute = new();
        removeTradeRoute.FromCityID = fromCityID;
        removeTradeRoute.ToCityID = toCityID;

        Command command = new();
        command.CommandType = "RemoveTradeRoute";
        command.RemoveTradeRoute = removeTradeRoute;
        command.Sender = Global.clientID;
        return command;
    }

    internal static Command ConstructNewTradeRouteCommand(int fromCityID, int toCityID)
    {
        NewTradeRoute newTradeRoute = new NewTradeRoute();
        newTradeRoute.FromCityID = fromCityID;
        newTradeRoute.ToCityID = toCityID;

        Command command = new();
        command.CommandType = "NewTradeRoute";
        command.NewTradeRoute = newTradeRoute;
        command.Sender = Global.clientID;
        return command;
    }

    internal static Command ConstructRemoveExportRouteCommand(int fromCityID, int toCityID, YieldType yieldType)
    {
        RemoveExportRoute removeExportRoute = new();
        removeExportRoute.FromCityID=fromCityID;
        removeExportRoute.ToCityID=toCityID;
        removeExportRoute.YieldType = (int)yieldType;
        
        Command command = new();
        command.CommandType = "RemoveExportRoute";
        command.RemoveExportRoute = removeExportRoute;
        command.Sender = Global.clientID;
        return command;
    }

    internal static Command ConstructNewExportRouteCommand(int fromCityID, int toCityID, YieldType yieldType)
    {
        NewExportRoute newExportRoute = new();
        newExportRoute.FromCityID = fromCityID;
        newExportRoute.ToCityID = toCityID;
        newExportRoute.YieldType = (int)yieldType;

        Command command = new();
        command.CommandType = "NewExportRoute";
        command.NewExportRoute = newExportRoute;
        command.Sender = Global.clientID;
        return command;
    }

    internal static Command ConstructUnassignPolicyCardCommand(int teamNum, int policyCardID)
    {
        UnassignPolicyCard unassignPolicyCard = new UnassignPolicyCard();
        unassignPolicyCard.TeamNum = teamNum;
        unassignPolicyCard.PolicyCardID = policyCardID;

        Command command = new Command();
        command.CommandType = "UnassignPolicyCard";
        command.UnassignPolicyCard = unassignPolicyCard;
        command.Sender = Global.clientID;
        return command;
    }

    internal static Command ConstructSetDiplomaticStateCommand(int teamNumOne, int teamNumTwo, DiplomaticState diplomaticState)
    {
        SetDiplomaticState setDiplomaticState = new SetDiplomaticState();
        setDiplomaticState.TeamNumOne = teamNumOne;
        setDiplomaticState.TeamNumTwo = teamNumTwo;
        setDiplomaticState.DiplomaticState=(int)diplomaticState;

        Command command = new();
        command.CommandType = "SetDiplomaticState";
        command.SetDiplomaticState = setDiplomaticState;
        command.Sender= Global.clientID;
        return command;
    }

    internal static Command ConstructSpawnUnitCommand(string unitType, int teamNum, Hex position, bool stackable, bool flexible)
    {
        SpawnUnit spawnUnit = new SpawnUnit();
        spawnUnit.UnitType = unitType;

        spawnUnit.TeamNum = teamNum;
        NetworkMessages.Hex hex = new NetworkMessages.Hex();
        hex.Q = position.q;
        hex.R = position.r;
        hex.S = position.s;
        spawnUnit.Position = hex;
        spawnUnit.Stackable = stackable;
        spawnUnit.Flexible = flexible;

        Command command = new();
        command.CommandType = "SpawnUnit";
        command.SpawnUnit = spawnUnit;
        command.Sender = Global.clientID;
        return command;
    }
}


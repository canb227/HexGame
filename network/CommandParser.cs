using NetworkMessages;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

    public static class CommandParser
    {



    static CommandParser()
    {
        NetworkPeer.CommandMessageReceivedEvent += OnCommandMessageReceived;
    }

    private static void OnCommandMessageReceived(Command command)
    {
        Global.Log("Command received: " + command.CommandType + " from " + command.Sender);
        switch (command.CommandType)
        {
            case "ActivateAbility":
                Global.gameManager.ActivateAbility((int)command.ActivateAbility.UnitId, command.ActivateAbility.AbilityName, new Hex((int)command.ActivateAbility.Target.Q, (int)command.ActivateAbility.Target.R, (int)command.ActivateAbility.Target.S), false);
                break;
            case "MoveUnit":
                Global.gameManager.MoveUnit((int)command.MoveUnit.UnitId, new Hex((int)command.MoveUnit.Target.Q, (int)command.MoveUnit.Target.R, (int)command.MoveUnit.Target.S), command.MoveUnit.IsEnemy, false);
                break;
            case "AddToProductionQueue":
                Global.gameManager.AddToProductionQueue((int)command.AddToProductionQueue.CityID, command.AddToProductionQueue.ItemName, new Hex((int)command.AddToProductionQueue.Target.Q, (int)command.AddToProductionQueue.Target.R, (int)command.AddToProductionQueue.Target.S), command.AddToProductionQueue.Front);
                break;  
            case "RemoveFromProductionQueue":
                Global.gameManager.RemoveFromProductionQueue((int)command.RemoveFromProductionQueue.CityID, (int)command.RemoveFromProductionQueue.Index);
                break;
            case "MoveToFrontOfProductionQueue":
                Global.gameManager.MoveToFrontOfProductionQueue((int)command.MoveToFrontOfProductionQueue.CityID, (int)command.MoveToFrontOfProductionQueue.Index);
                break;
        }
    }

    public static Command ConstructMoveUnitCommand(int unitID, Hex target, bool isEnemy)
    {
        MoveUnit moveUnit = new MoveUnit();
        moveUnit.UnitId = (ulong)unitID;
        moveUnit.Target = new NetworkMessages.Hex();
        moveUnit.Target.Q = (ulong)target.q;
        moveUnit.Target.R = (ulong)target.r;
        moveUnit.Target.S = (ulong)target.s;
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
        activateAbility.UnitId = (ulong)unitID;
        activateAbility.AbilityName = abilityName;
        activateAbility.Target = new NetworkMessages.Hex();
        activateAbility.Target.Q = (ulong)target.q;
        activateAbility.Target.R = (ulong)target.r;
        activateAbility.Target.S = (ulong)target.s;

        Command command = new();
        command.CommandType = "ActivateAbility";
        command.ActivateAbility = activateAbility;
        command.Sender = Global.clientID;
        return command;
    }




    internal static Command ConstructCityGrowthExpansionCommand(int cityID, Hex target)
    {
        throw new NotImplementedException();
    }

    internal static Command ConstructAddToProductionQueueCommand(int cityID, string name, Hex targetHex, bool front)
    {
        AddToProductionQueue addToProductionQueue = new AddToProductionQueue();
        addToProductionQueue.ItemName = name;
        addToProductionQueue.CityID = (ulong)cityID;
        addToProductionQueue.Target = new NetworkMessages.Hex();
        addToProductionQueue.Target.Q = (ulong)targetHex.q;
        addToProductionQueue.Target.R = (ulong)targetHex.r;
        addToProductionQueue.Target.S = (ulong)targetHex.s;
        addToProductionQueue.Front = front;

        Command command = new Command();
        command.CommandType = "AddToProductionQueue";
        command.AddToProductionQueue = addToProductionQueue;
        command.Sender = Global.clientID;
        return command;
    }

    internal static Command ConstructRemoveFromProductionQueueCommand(int cityID, ulong index)
    {
        RemoveFromProductionQueue removeFromProductionQueue = new RemoveFromProductionQueue();
        removeFromProductionQueue.CityID = (ulong)cityID;
        removeFromProductionQueue.Index = index;


        Command command = new Command();
        command.CommandType = "RemoveFromProductionQueue";
        command.RemoveFromProductionQueue = removeFromProductionQueue;
        command.Sender = Global.clientID;
        return command;
    }

    internal static Command ConstructMoveToFrontOfProductionQueueCommand(int cityID, ulong index)
    {
        MoveToFrontOfProductionQueue moveToFrontOfProductionQueue = new MoveToFrontOfProductionQueue();
        moveToFrontOfProductionQueue.CityID = (ulong)cityID;
        moveToFrontOfProductionQueue.Index = index;

        Command command = new Command();
        command.CommandType = "MoveToFrontOfProductionQueue";
        command.MoveToFrontOfProductionQueue = moveToFrontOfProductionQueue;
        command.Sender = Global.clientID;
        return command;
    }
}


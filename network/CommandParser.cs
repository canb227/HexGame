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
        switch (command.CommandType)
        {
            case "MoveUnit":
                Global.gameManager.ActivateAbility((int)command.ActivateAbility.UnitId, command.ActivateAbility.AbilityName, new Hex((int)command.ActivateAbility.Target.Q, (int)command.ActivateAbility.Target.R, (int)command.ActivateAbility.Target.S));
                break;
            case "ActivateAbility":
                Global.gameManager.MoveUnit((int)command.MoveUnit.UnitId, new Hex((int)command.MoveUnit.Target.Q, (int)command.MoveUnit.Target.R, (int)command.MoveUnit.Target.S));
                break;
        }
    }

    public static Command ConstructMoveUnitCommand(int unitID, Hex target)
    {
        MoveUnit moveUnit = new MoveUnit();
        moveUnit.UnitId = (ulong)unitID;
        moveUnit.Target = new NetworkMessages.Hex();
        moveUnit.Target.Q = (ulong)target.q;
        moveUnit.Target.R = (ulong)target.r;
        moveUnit.Target.S = (ulong)target.s;

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
        command.CommandType = "MoveUnit";
        command.ActivateAbility = activateAbility;
        command.Sender = Global.clientID;
        return command;
    }


    internal static Command ConstructChangeProductionQueueCommand(City city, List<ProductionQueueType> queue)
    {
        throw new NotImplementedException();
    }
}


using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data;
using Godot;
using System.IO;
using NetworkMessages;
using System.Drawing;

[Serializable]
public class MinorPlayer : BasePlayer
{
    public MinorPlayer( int teamNum, Godot.Color teamColor, bool isAI) : base(teamNum, teamColor, isAI)
    {

    }

    public MinorPlayer()
    {
        //used for loading
    }

    private void SetBaseHexYields()
    {
        flatYields.food = 1;
        roughYields.production = 1;
        //mountainYields.production += 0;
        coastalYields.food = 1;
        oceanYields.gold = 1;

        desertYields.gold = 1;
        plainsYields.production = 1;
        grasslandYields.food = 1;
        tundraYields.happiness = 1;
        //arcticYields

    }

    public override void OnTurnStarted(int turnNumber, bool updateUI)
    {
        base.OnTurnStarted(turnNumber, false);
    }

}



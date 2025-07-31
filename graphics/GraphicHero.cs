using Godot;
using NetworkMessages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;

public partial class GraphicHero : GraphicUnit
{
    public GraphicHero(Unit unit) : base(unit)
    {
    }
    public override void Selected()
    {
        if (unit.teamNum == Global.gameManager.game.localPlayerTeamNum)
        {
            GenerateHexTriangles(unit.MovementRange());
        }
        Global.gameManager.graphicManager.uiManager.HeroSelected(unit as Hero);
        if (unit.name == "Settler" || unit.name == "Founder")
        {
            GraphicGameBoard ggb = ((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);
            ggb.ShowSettleUI();
        }
    }

    public override void Unselected()
    {
        GraphicGameBoard ggb = ((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);
        ggb.ClearSelectionGraphic();
        if (unit.name == "Settler" || unit.name == "Founder")
        {
            ggb.HideSettleUI();
        }
        Global.gameManager.graphicManager.uiManager.HeroUnselected(unit as Hero);
    }
}
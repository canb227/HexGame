using Godot;
using NetworkMessages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using static Godot.Projection;

public partial class GraphicUnit : GraphicObject
{
    public Unit unit;
    public Node3D node3D;
    public UnitAbility waitingAbility;
    public UnitWorldUI unitWorldUI;
    public Hex graphicalHex;
    public GraphicUnit(Unit unit)
    {
        this.unit = unit;
        node3D = new Node3D();
        //InstantiateUnitUI(unit);
        unitWorldUI = new UnitWorldUI(unit);
        AddChild(unitWorldUI);
        UpdateGraphic(GraphicUpdateType.Visibility);
    }
    public override void _Ready()
    {
        InstantiateUnit(unit);
    }

    public override void UpdateGraphic(GraphicUpdateType graphicUpdateType)
    {
        if (!IsInstanceValid(this)) { return; }
        if (graphicUpdateType == GraphicUpdateType.Remove)
        {
            if(Global.gameManager.graphicManager.selectedObjectID == unit.id)
            {
                Global.gameManager.graphicManager.UnselectObject();
            }
            Visible = false;
            Global.gameManager.graphicManager.toBeDeleted.Add(unit.id, this);
            Global.gameManager.graphicManager.hexObjectDictionary[unit.hex].Remove(this);
        }
        else if (graphicUpdateType == GraphicUpdateType.Move || graphicUpdateType == GraphicUpdateType.Update)
        {
            Transform3D newTransform = node3D.Transform;
            GraphicGameBoard ggb = (GraphicGameBoard)(Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);
            Point graphicalHexPoint = Global.gameManager.graphicManager.layout.HexToPixel(ggb.HexToGraphicHex(unit.hex));
            float height = ggb.Vector3ToHeightMapVal(node3D.Transform.Origin);
            newTransform.Origin = new Vector3((float)graphicalHexPoint.y, height, (float)graphicalHexPoint.x);


            //newTransform.Origin = new Vector3((float)hexPoint.y, height, (float)hexPoint.x);
            node3D.Transform = newTransform;
            UpdateMovementGraphics();
            unitWorldUI.Update();
        }
        else if (graphicUpdateType == GraphicUpdateType.Visibility)
        {
            if (Global.gameManager.game.localPlayerRef.visibleGameHexDict.ContainsKey(unit.hex))
            {
                this.Visible = true;
                unitWorldUI.Visible = true;
            }
            else
            {
                this.Visible = false;
                unitWorldUI.Visible = false;
            }
        }
    }

    private void InstantiateUnit(Unit unit)
    {
        UnitInfo unitInfo = new();
        if (unit is Hero hero)
        {
            unitInfo = HeroLoader.heroDict[hero.name].unitInfo;
        }
        else
        {
            UnitLoader.unitsDict.TryGetValue(unit.unitType, out unitInfo);
        }
        node3D = Godot.ResourceLoader.Load<PackedScene>("res://" + unitInfo.ModelPaths[Global.gameManager.game.playerDictionary[unit.teamNum].faction]).Instantiate<Node3D>();
        Transform3D newTransform = node3D.Transform;
        GraphicGameBoard ggb = (GraphicGameBoard)(Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);
        Point hexPoint = Global.gameManager.graphicManager.layout.HexToPixel(ggb.HexToGraphicHex(unit.hex));
        newTransform.Origin = new Vector3((float)hexPoint.y, 1, (float)hexPoint.x);
        node3D.Transform = newTransform;
        Global.gameManager.graphicManager.hexObjectDictionary[unit.hex].Add(this);
        AddChild(node3D);
        if((unit.name == "Founder" || unit is Hero) && unit.teamNum == Global.gameManager.game.localPlayerTeamNum)
        {
            Global.camera.SetHexTarget(unit.hex);
        }
    }

    private void InstantiateUnitUI(Unit unit)
    {
    }

    public override void Unselected()
    {
        GraphicGameBoard ggb = ((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);
        ggb.ClearSelectionGraphic();
        if(unit.name == "Settler" || unit.name == "Founder")
        {
            ggb.HideSettleUI();
        }
        Global.gameManager.graphicManager.uiManager.UnitUnselected(unit);
    }

    public override void Selected()
    {
        if(unit.teamNum == Global.gameManager.game.localPlayerTeamNum)
        {
            GenerateHexTriangles(unit.MovementRange());
        }
        Global.gameManager.graphicManager.uiManager.UnitSelected(unit);
        if (unit.name == "Settler" || unit.name == "Founder")
        {
            GraphicGameBoard ggb = ((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);
            ggb.ShowSettleUI();
        }
    }

    public void UpdateMovementGraphics()
    {
        bool hadMovementRangeHexes = false;
        bool hadMovementRangeLines = false;
        foreach (Node3D child in GetChildren())
        {
            if (child.Name == "MovementRangeHexes")
            {
                hadMovementRangeHexes = true;
                child.Free();
            }
            else if (child.Name == "MovementRangeLines")
            {
                hadMovementRangeLines = true;
                child.Free();
            }
        }
        if(hadMovementRangeLines)
        {
        }
        if (hadMovementRangeHexes)
        {
            GenerateHexTriangles(unit.MovementRange());
        }
    }


    public override void ProcessRightClick(Hex hex)
    {
        hex = hex.WrapHex();
        Global.gameManager.MoveUnit(unit.id, hex, Global.gameManager.game.mainGameBoard.gameHexDict[hex].IsEnemyPresent(unit.teamNum)); //networked command
        Global.gameManager.graphicManager.graphicObjectDictionary[unit.id].UpdateGraphic(GraphicUpdateType.Move);
/*        foreach (Unit unit in Global.gameManager.game.unitDictionary.Values)
        {
            Global.gameManager.graphicManager.graphicObjectDictionary[unit.id].UpdateGraphic(GraphicUpdateType.Move);
        }*/
        Unselected();
        Selected();
    }

    public void GenerateTargetingPrompt(UnitAbility ability)
    {
        List<Hex> hexes = new List<Hex>();
        foreach(Hex hex in unit.hex.WrappingRange(ability.range, Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.top, Global.gameManager.game.mainGameBoard.bottom))
        {
            if (ability.validTargetTypes.IsHexValidTarget(Global.gameManager.game.mainGameBoard.gameHexDict[hex], unit))
            {
                hexes.Add(hex);
            }
        }

        if(hexes.Count > 0)
        {
            Global.gameManager.graphicManager.SetWaitForTargeting(true);
            Global.gameManager.graphicManager.uiManager.HideGenericUIForTargeting();
            Global.gameManager.graphicManager.HideAllCityWorldUI();
            waitingAbility = ability;
            Global.gameManager.graphicManager.GenerateHexSelectionLines(hexes, Godot.Colors.Gold, "UnitMove");
            Global.gameManager.graphicManager.GenerateHexSelectionTriangles(hexes, Godot.Colors.BlueViolet, "UnitMove");
        }
    }

    public override void RemoveTargetingPrompt()
    {
        Global.gameManager.graphicManager.uiManager.ShowGenericUIAfterTargeting();
        Global.gameManager.graphicManager.ShowAllWorldUI();
        GraphicGameBoard ggb = ((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);
        ggb.ClearSelectionGraphic();
    }

    protected void GenerateHexTriangles(Dictionary<Hex, float> hexes)
    {
        foreach (Hex hex in hexes.Keys)
        {
            if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].IsEnemyPresent(unit.teamNum))
            {
                Global.gameManager.graphicManager.GenerateSingleHexSelectionTriangles(hex, Godot.Colors.Red, "");
            }
            else
            {
                Global.gameManager.graphicManager.GenerateSingleHexSelectionTriangles(hex, Godot.Colors.Gold, "");
            }
        }
    }

    public void SetWorldUIVisibility(bool visible)
    {
        //unitWorldUI.Visible = visible;
    }

    public override void _Process(double delta)
    {

    }
}
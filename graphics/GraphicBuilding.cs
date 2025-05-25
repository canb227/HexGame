using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public partial class GraphicBuilding : GraphicObject
{
    public Building building;
    private GraphicManager graphicManager;
    public Node3D node3D;
    public Layout layout;
    public GraphicBuilding(Building building, Layout layout, GraphicManager graphicManager)
    {
        this.building = building;
        this.graphicManager = graphicManager;
        this.layout = layout;
        node3D = new Node3D();
        InitBuilding(building);
    }

    public override void UpdateGraphic(GraphicUpdateType graphicUpdateType)
    {
        if (graphicUpdateType == GraphicUpdateType.Remove)
        {
            QueueFree();
        }
        else if (graphicUpdateType == GraphicUpdateType.Visibility)
        {
            if (graphicManager.game.playerDictionary[graphicManager.game.localPlayerTeamNum].seenGameHexDict.ContainsKey(building.district.gameHex.hex))
            {
                this.Visible = true;
            }
            else
            {
                this.Visible = false;
            }
        }
    }

    private void InitBuilding(Building building)
    {
        node3D = Godot.ResourceLoader.Load<PackedScene>("res://" + BuildingLoader.buildingsDict[building.name].ModelPath).Instantiate<Node3D>();
        Transform3D newTransform = node3D.Transform;
        Point hexPoint = layout.HexToPixel(building.district.gameHex.hex);
        newTransform.Origin = new Vector3((float)hexPoint.y, 1, (float)hexPoint.x);
        node3D.Transform = newTransform;

        graphicManager.hexObjectDictionary[building.district.gameHex.hex].Add(this);

        AddChild(node3D);
    }

    public override void Unselected()
    {
        GD.PushWarning("NOT IMPLEMENTED");
    }

    public override void Selected()
    {
        GD.PushWarning("NOT IMPLEMENTED");
    }

    public override void ProcessRightClick(Hex hex)
    {
        GD.PushWarning("NOT IMPLEMENTED");
    }

    public override void _Process(double delta)
    {

    }
    public override void RemoveTargetingPrompt()
    {
        GD.PushWarning("NOT IMPLEMENTED");
    }
}
using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public partial class GraphicBuilding : GraphicObject
{
    public Building building;
    public Node3D node3D;
    public Layout layout;
    public GraphicBuilding(Building building, Layout layout)
    {
        this.building = building;
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
    }

    private void InitBuilding(Building building)
    {
        if (building.buildingType == BuildingType.CityCenter)
        {
            node3D = Godot.ResourceLoader.Load<PackedScene>("res://graphics/models/huts.glb").Instantiate<Node3D>();
        }
        if (building.buildingType == BuildingType.Palace)
        {
            node3D = Godot.ResourceLoader.Load<PackedScene>("res://graphics/models/huts.glb").Instantiate<Node3D>();
        }
        Transform3D newTransform = node3D.Transform;
        Point hexPoint = layout.HexToPixel(building.district.gameHex.hex);
        newTransform.Origin = new Vector3((float)hexPoint.y, 1, (float)hexPoint.x);
        node3D.Transform = newTransform;

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
}
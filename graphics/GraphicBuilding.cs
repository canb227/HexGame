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
        UpdateGraphic(GraphicUpdateType.Visibility);
    }

    public override void UpdateGraphic(GraphicUpdateType graphicUpdateType)
    {
        if (graphicUpdateType == GraphicUpdateType.Remove)
        {
            QueueFree();
        }
        else if (graphicUpdateType == GraphicUpdateType.Visibility)
        {
            if (Global.gameManager.game.localPlayerRef.seenGameHexDict.ContainsKey(building.districtHex))
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
        GraphicGameBoard ggb = ((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);
        int newQ = (Global.gameManager.game.mainGameBoard.left + (building.districtHex.r >> 1) + building.districtHex.q) % ggb.chunkSize - (building.districtHex.r >> 1);
        Hex modHex = new Hex(newQ, building.districtHex.r, -newQ - building.districtHex.r);
        Point hexPoint = layout.HexToPixel(modHex);
        float height = ggb.chunkList[ggb.hexToChunkDictionary[building.districtHex]].Vector3ToHeightMapVal(node3D.Transform.Origin); //TODO
        newTransform.Origin = new Vector3((float)hexPoint.y, height, (float)hexPoint.x);
        node3D.Transform = newTransform;


        Global.gameManager.graphicManager.hexObjectDictionary[building.districtHex].Add(this);

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
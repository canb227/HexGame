using Godot;
using NetworkMessages;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public partial class GraphicBuilding : GraphicObject
{
    public string buildingName;
    public Hex buildingHex;
    public bool isDistrictCenterBuilding;
    public Node3D node3D;
    public Layout layout;
    public GraphicBuilding(string buildingName, Hex buildingHex, bool isDistrictCenterBuilding, Layout layout)
    {
        this.buildingName = buildingName;
        this.buildingHex = buildingHex;
        this.isDistrictCenterBuilding = isDistrictCenterBuilding;
        this.layout = layout;
        node3D = new Node3D();
        InitBuilding();
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
            if (Global.gameManager.game.localPlayerRef.seenGameHexDict.ContainsKey(buildingHex))
            {
                this.Visible = true;
            }
            else
            {
                this.Visible = false;
            }
        }
        else if (graphicUpdateType == GraphicUpdateType.Update)
        {
            if(Global.gameManager.game.mainGameBoard.gameHexDict[buildingHex].district.isUrban)
            {
                if (!Global.gameManager.game.mainGameBoard.gameHexDict[buildingHex].district.isCityCenter && !isDistrictCenterBuilding)
                {
                    this.Visible = false;
                }
            }
        }
    }

    private void InitBuilding()
    {
        node3D = Godot.ResourceLoader.Load<PackedScene>("res://" + BuildingLoader.buildingsDict[buildingName].ModelPath).Instantiate<Node3D>();
        Transform3D newTransform = node3D.Transform;
        GraphicGameBoard ggb = ((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);
        int newQ = (Global.gameManager.game.mainGameBoard.left + (buildingHex.r >> 1) + buildingHex.q) % ggb.chunkSize - (buildingHex.r >> 1);
        int heightMapQ = newQ + ggb.chunkList[ggb.hexToChunkDictionary[buildingHex]].graphicalOrigin.q;
        Hex modHex = new Hex(newQ, buildingHex.r, -newQ - buildingHex.r);
        Hex heightMapHex = new Hex(heightMapQ, buildingHex.r, -heightMapQ - buildingHex.r);
        Point hexPoint = Global.gameManager.graphicManager.layout.HexToPixel(modHex);
        Point heightMapPoint = Global.gameManager.graphicManager.layout.HexToPixel(heightMapHex);
        float height = ggb.Vector3ToHeightMapVal(new Vector3((float)heightMapPoint.y, 0.0f, (float)heightMapPoint.x));
        newTransform.Origin = new Vector3((float)hexPoint.y, height+0.85f, (float)hexPoint.x);
        node3D.Transform = newTransform;


        Global.gameManager.graphicManager.hexObjectDictionary[buildingHex].Add(this);

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
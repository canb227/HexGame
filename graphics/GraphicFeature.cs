using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime;

public partial class GraphicFeature : GraphicObject
{
    public Hex hex;
    public FeatureType featureType;
    public Node3D featureModel;

    public GraphicFeature(Hex hex, FeatureType featureType)
    {
        this.hex = hex;
        this.featureType = featureType;
        featureModel = null;
        switch (featureType)
        {
            case FeatureType.Forest:
                featureModel = Godot.ResourceLoader.Load<PackedScene>("res://graphics/models/trees.glb").Instantiate<Node3D>();
                break;
            case FeatureType.River:
                Random rand = new Random();
                if (rand.NextDouble() > 0.5)
                {
                    featureModel = Godot.ResourceLoader.Load<PackedScene>("res://graphics/models/river1.glb").Instantiate<Node3D>();
                }
                else
                {
                    featureModel = Godot.ResourceLoader.Load<PackedScene>("res://graphics/models/river2.glb").Instantiate<Node3D>();
                }
                break;
            case FeatureType.Road:
                featureModel = Godot.ResourceLoader.Load<PackedScene>("res://graphics/models/road.glb").Instantiate<Node3D>();
                break;
            case FeatureType.Coral:
                featureModel = Godot.ResourceLoader.Load<PackedScene>("res://graphics/models/coral.glb").Instantiate<Node3D>();
                break;
            default:
                break;
        }
        Transform3D newTransform = featureModel.Transform;
        GraphicGameBoard ggb = ((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);
        int newQ = (Global.gameManager.game.mainGameBoard.left + (hex.r >> 1) + hex.q) % ggb.chunkSize - (hex.r >> 1);
        Hex modHex = new Hex(newQ, hex.r, -newQ - hex.r);
        Point hexPoint = Global.gameManager.graphicManager.layout.HexToPixel(modHex);
        float height = ggb.Vector3ToHeightMapVal(featureModel.Transform.Origin);
        newTransform.Origin = new Vector3((float)hexPoint.y, height, (float)hexPoint.x);
        featureModel.Transform = newTransform;
        this.Visible = false;
        featureModel.Visible = false;
        AddChild(featureModel);
    }
    public override void Selected()
    {
        throw new NotImplementedException();
    }
    public override void Unselected()
    {
        throw new NotImplementedException();
    }

    public override void ProcessRightClick(Hex hex)
    {
        throw new NotImplementedException();
    }
    public override void RemoveTargetingPrompt()
    {
        throw new NotImplementedException();
    }

    public override void UpdateGraphic(GraphicUpdateType graphicUpdateType)
    {
        if (graphicUpdateType == GraphicUpdateType.Visibility)
        {
            if (Global.gameManager.game.localPlayerRef.visibleGameHexDict.ContainsKey(hex))
            {
                this.Visible = true;
                featureModel.Visible = true;
            }
            else if (Global.gameManager.game.localPlayerRef.seenGameHexDict.ContainsKey(hex))
            {
                this.Visible = true;
                featureModel.Visible = true;
            }
            else
            {
                this.Visible = false;
                featureModel.Visible = false;
            }
        }
    }
}

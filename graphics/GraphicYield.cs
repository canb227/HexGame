using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime;

public partial class GraphicYield : GraphicObject
{
    public Hex hex;
    public YieldType yieldType;
    public Label3D label;
    public float value;

    public GraphicYield(Hex hex, YieldType yieldType, float value)
    {
        this.hex = hex;
        this.yieldType = yieldType;
        this.value = value;
        label = new Label3D();
        label.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
        label.FontSize = 130;
        //label.NoDepthTest = true;
        label.OutlineModulate = new Godot.Color(0, 0, 0, 0.5f);
        label.OutlineSize = 30;
        float offsetX = 0;
        float offsetY = 0;
        switch (yieldType)
        {
            case YieldType.food:
                //featureModel = Godot.ResourceLoader.Load<PackedScene>("res://graphics/models/trees.glb").Instantiate<Node3D>();
                label.Text = "Food:" + value;
                label.Modulate = new Godot.Color(1.0f, 0.11f, 0.0f);
                offsetX -= 0;
                offsetY += 5;
                break;
            case YieldType.production:
                label.Text = "Production:" + value;
                label.Modulate = new Godot.Color(0.71f, 0.435f, 0.086f);
                offsetX -= 0;
                offsetY -= 0;
                break;
            case YieldType.gold:
                label.Text = "Gold:" + value;
                label.Modulate = new Godot.Color(1f, 0.98f, 0.0f);
                offsetX -= 0;
                offsetY -= 5;
                break;
            case YieldType.science:
                label.Text = "Science:" + value;
                label.Modulate = new Godot.Color(0.0f, 0.5f, 1.0f);
                offsetX -= 2;
                offsetY += 3;
                break;
            case YieldType.culture:
                label.Text = "Culture:" + value;
                label.Modulate = new Godot.Color(0.64f, 0.0f, 1.0f);
                offsetX -= 2;
                offsetY -= 3;
                break;
            case YieldType.happiness:
                label.Text = "Happiness:" + value;
                label.Modulate = new Godot.Color(1.0f, 0.7f, 0.0f);
                offsetX += 2;
                offsetY -= 3;
                break;
            case YieldType.influence:
                label.Text = "Influence:" + value;
                label.Modulate = new Godot.Color(0.33f, 1.0f, 0.0f);
                offsetX += 2;
                offsetY += 3;
                break;
            default:
                break;
        }
        Transform3D newTransform = label.Transform;
        GraphicGameBoard ggb = ((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);
        int newQ = (Global.gameManager.game.mainGameBoard.left + (hex.r >> 1) + hex.q) % ggb.chunkSize - (hex.r >> 1);
        Hex modHex = new Hex(newQ, hex.r, -newQ - hex.r);
        Point hexPoint = Global.gameManager.graphicManager.layout.HexToPixel(modHex);
        float height = ggb.chunkList[ggb.hexToChunkDictionary[hex]].Vector3ToHeightMapVal(label.Transform.Origin);
        newTransform.Origin = new Vector3((float)hexPoint.y+offsetX, 1.0f, (float)hexPoint.x + offsetY); //replace with height TODO
        label.Transform = newTransform;
        this.Visible = false;
        label.Visible = false;
        AddChild(label);
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
        if (graphicUpdateType == GraphicUpdateType.Update)
        {
            value = Global.gameManager.game.mainGameBoard.gameHexDict[hex].yields.YieldsToDict()[yieldType];
            if (value != 0)
            {
                this.UpdateGraphic(GraphicUpdateType.Visibility);
            }
            switch (yieldType)
            {
                case YieldType.food:
                    label.Text = "Food:" + value;
                    break;
                case YieldType.production:
                    label.Text = "Production:" + value;
                    break;
                case YieldType.gold:
                    label.Text = "Gold:" + value;
                    break;
                case YieldType.science:
                    label.Text = "Science:" + value;
                    break;
                case YieldType.culture:
                    label.Text = "Culture:" + value;
                    break;
                case YieldType.happiness:
                    label.Text = "Happiness:" + value;
                    break;
                case YieldType.influence:
                    label.Text = "Influence:" + value;
                    break;
                default:
                    break;
            }
        }
        if (graphicUpdateType == GraphicUpdateType.Visibility)
        {
            if (Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].visibleGameHexDict.ContainsKey(hex))
            {
                if (value != 0)
                {
                    this.Visible = true;
                    label.Visible = true;
                }
            }
            else if (Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].seenGameHexDict.ContainsKey(hex))
            {
                if (value != 0)
                {
                    this.Visible = true;
                    label.Visible = true;
                }
            }
            else
            {
                this.Visible = false;
                label.Visible = false;
            }
        }
    }
}

using Godot;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime;
using System.Security.AccessControl;

public partial class GraphicRuins : GraphicObject
{
    public AncientRuins ancientRuins;
    public Hex hex;
    public Node3D featureModel;

    public Node3D icon3D;
    public TextureRect ruinIcon;
    private ShaderMaterial greyScaleShaderMaterial;

    public GraphicRuins(AncientRuins ancientRuins, Hex hex)
    {
        this.ancientRuins = ancientRuins;
        this.hex = hex;
        featureModel = Godot.ResourceLoader.Load<PackedScene>("res://graphics/models/ruins.tscn").Instantiate<Node3D>();
    }

    public override void _Ready()
    {
        Transform3D newTransform = featureModel.Transform;
        GraphicGameBoard ggb = ((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);
        int newQ = (Global.gameManager.game.mainGameBoard.left + (hex.r >> 1) + hex.q) % ggb.chunkSize - (hex.r >> 1);
        int heightMapQ = newQ + ggb.chunkList[ggb.hexToChunkDictionary[hex]].graphicalOrigin.q;
        Hex modHex = new Hex(newQ, hex.r, -newQ - hex.r);
        Hex heightMapHex = new Hex(heightMapQ, hex.r, -heightMapQ - hex.r);
        Point hexPoint = Global.gameManager.graphicManager.layout.HexToPixel(modHex);
        Point heightMapPoint = Global.gameManager.graphicManager.layout.HexToPixel(heightMapHex);
        float height = ggb.Vector3ToHeightMapVal(new Vector3((float)heightMapPoint.y, 0.0f, (float)heightMapPoint.x));
        newTransform.Origin = new Vector3((float)hexPoint.y, height, (float)hexPoint.x);
        featureModel.Transform = newTransform;

        this.Visible = false;
        featureModel.Visible = false;
        AddChild(featureModel);

        icon3D = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/ResourceWorldUI.tscn").Instantiate<Node3D>();
        AddChild(icon3D);

        ruinIcon = icon3D.GetNode<TextureRect>("SubViewport/ResourceWorldUI/ResourceIcon");
        ruinIcon.Texture = Godot.ResourceLoader.Load<Texture2D>("res://graphics/ui/icons/ruins.png");
        Shader greyScaleShader = GD.Load<Shader>("res://graphics/shaders/general/greyscale.gdshader");
        greyScaleShaderMaterial = new ShaderMaterial();
        greyScaleShaderMaterial.Shader = greyScaleShader;
        ruinIcon.Material = greyScaleShaderMaterial;

        UpdateGraphic(GraphicUpdateType.Visibility);
        newTransform = icon3D.Transform;
        newTransform.Origin = new Vector3((float)hexPoint.y-1, 8, (float)hexPoint.x);
        icon3D.Transform = newTransform;

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
        if (graphicUpdateType == GraphicUpdateType.Update || graphicUpdateType == GraphicUpdateType.Visibility)
        {
            if (Global.gameManager.game.localPlayerRef.visibleGameHexDict.ContainsKey(ancientRuins.hex))
            {
                greyScaleShaderMaterial.SetShaderParameter("enabled", false);
                this.Visible = true;
                icon3D.Visible = true;
                featureModel.Visible = true;
            }
            else if (Global.gameManager.game.localPlayerRef.seenGameHexDict.ContainsKey(ancientRuins.hex))
            {
                greyScaleShaderMaterial.SetShaderParameter("enabled", true);
                this.Visible = true;
                icon3D.Visible = true;
                featureModel.Visible = true;
            }
            else
            {
                this.Visible = false;
                icon3D.Visible = false;
                featureModel.Visible = false;
            }
        }
    }
}

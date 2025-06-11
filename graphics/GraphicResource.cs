using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public partial class GraphicResource : GraphicObject
{
    public ResourceType resourceType;
    public Node3D node3D;
    public TextureRect resourceIcon;
    public Hex hex;
    private ShaderMaterial greyScaleShaderMaterial;
    public GraphicResource(ResourceType resource, Hex hex)
    {
        this.hex = hex;
        node3D = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/ResourceWorldUI.tscn").Instantiate<Node3D>();
        resourceIcon = node3D.GetNode<TextureRect>("SubViewport/ResourceWorldUI/ResourceIcon");

        resourceIcon.Texture = Godot.ResourceLoader.Load<Texture2D>("res://" + ResourceLoader.resources[resource].IconPath);
        
        Shader greyScaleShader = GD.Load<Shader>("res://graphics/shaders/general/greyscale.gdshader");
        greyScaleShaderMaterial = new ShaderMaterial();
        greyScaleShaderMaterial.Shader = greyScaleShader;
        resourceIcon.Material = greyScaleShaderMaterial;

        Transform3D newTransform = Transform;
        GraphicGameBoard ggb = ((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);
        int newQ = (Global.gameManager.game.mainGameBoard.left + (hex.r >> 1) + hex.q) % ggb.chunkSize - (hex.r >> 1);
        Hex modHex = new Hex(newQ, hex.r, -newQ - hex.r);
        Point hexPoint = Global.gameManager.graphicManager.layout.HexToPixel(modHex);
        newTransform.Origin = new Vector3((float)hexPoint.y, 8, (float)hexPoint.x);
        Transform = newTransform;
        
        this.Visible = false;
        node3D.Visible = false;
        AddChild(node3D); //configure visibility and add 3d model
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
            if (Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].visibleGameHexDict.ContainsKey(hex))
            {
                greyScaleShaderMaterial.SetShaderParameter("enabled", false);
                this.Visible = true;
                node3D.Visible = true;
            }
            else if (Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].seenGameHexDict.ContainsKey(hex))
            {
                greyScaleShaderMaterial.SetShaderParameter("enabled", true);
                this.Visible = true;
                node3D.Visible = true;
            }
            else
            {
                this.Visible = false;
                node3D.Visible = false;
            }
        }
    }
}

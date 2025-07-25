using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime;

public partial class GraphicResource : GraphicObject
{
    public ResourceType resourceType;
    public Node3D node3D;
    public TextureRect resourceIcon;
    public Hex hex;
    private ShaderMaterial greyScaleShaderMaterial;
    private ShaderMaterial greyScale3DShaderMaterial;
    public MeshInstance3D resourceMeshInstance;
    public MeshInstance3D improvementMeshInstance;
    public bool improved = false;
    public GraphicResource(ResourceType resource, Hex hex)
    {
        this.resourceType = resource;
        this.hex = hex;
    }
    public override void _Ready()
    {
        Node3D temp = Godot.ResourceLoader.Load<PackedScene>("res://" + ResourceLoader.resources[resourceType].ModelPath).Instantiate<Node3D>();
        AddChild(temp);
        resourceMeshInstance = temp.GetNode<MeshInstance3D>("Resource");
        improvementMeshInstance = temp.GetNode<MeshInstance3D>("Improvement");

        Transform3D newTransform = resourceMeshInstance.Transform;
        GraphicGameBoard ggb = ((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);
        int newQ = (Global.gameManager.game.mainGameBoard.left + (hex.r >> 1) + hex.q) % ggb.chunkSize - (hex.r >> 1);
        int heightMapQ = newQ + ggb.chunkList[ggb.hexToChunkDictionary[hex]].graphicalOrigin.q;
        Hex modHex = new Hex(newQ, hex.r, -newQ - hex.r);
        Hex heightMapHex = new Hex(heightMapQ, hex.r, -heightMapQ - hex.r);
        Point hexPoint = Global.gameManager.graphicManager.layout.HexToPixel(modHex);
        Point heightMapPoint = Global.gameManager.graphicManager.layout.HexToPixel(heightMapHex);
        float height = ggb.Vector3ToHeightMapVal(new Vector3((float)heightMapPoint.y, 0.0f, (float)heightMapPoint.x));
        newTransform.Origin = new Vector3((float)hexPoint.y, height, (float)hexPoint.x);
        resourceMeshInstance.Transform = newTransform;
        improvementMeshInstance.Transform = newTransform;
        improvementMeshInstance.Visible = false;


        node3D = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/ResourceWorldUI.tscn").Instantiate<Node3D>();
        AddChild(node3D);

        resourceIcon = node3D.GetNode<TextureRect>("SubViewport/ResourceWorldUI/ResourceIcon");
        resourceIcon.Texture = Godot.ResourceLoader.Load<Texture2D>("res://" + ResourceLoader.resources[resourceType].IconPath);
        Shader greyScaleShader = GD.Load<Shader>("res://graphics/shaders/general/greyscale.gdshader");
        greyScaleShaderMaterial = new ShaderMaterial();
        greyScaleShaderMaterial.Shader = greyScaleShader;
        resourceIcon.Material = greyScaleShaderMaterial;

        //2dworldui
        newTransform = node3D.Transform;
        newTransform.Origin = new Vector3((float)hexPoint.y, 8, (float)hexPoint.x + 4);
        node3D.Transform = newTransform;

        this.Visible = false;
        node3D.Visible = false;
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
            //hide the resource if we dont have the research yet, end early
            if (Global.gameManager.game.localPlayerRef.hiddenResources.Contains(resourceType))
            {
                node3D.Visible = false;
                resourceMeshInstance.Visible = false;
                return;
            }

            if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].district != null)
            {
                improvementMeshInstance.Visible = true;
            }
            else
            {
                improvementMeshInstance.Visible = false;
            }

            if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].district != null && Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.isUrban)
            {
                //this.Visible = false;
                //node3D.Visible = false;
                resourceMeshInstance.Visible = false;
                improvementMeshInstance.Visible = false;
            }
        }
        if (graphicUpdateType == GraphicUpdateType.Visibility)
        {
            //hide the resource if we dont have the research yet, end early
            if (Global.gameManager.game.localPlayerRef.hiddenResources.Contains(resourceType))
            {
                node3D.Visible = false;
                resourceMeshInstance.Visible = false;
                return;
            }

            if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].district != null && Global.gameManager.game.mainGameBoard.gameHexDict[hex].district.isUrban)
            {
                //this.Visible = false;
                //node3D.Visible = false;
                resourceMeshInstance.Visible = false;
                improvementMeshInstance.Visible = false;
            }
            else
            {
                if (Global.gameManager.game.localPlayerRef.visibleGameHexDict.ContainsKey(hex))
                {
                    greyScaleShaderMaterial.SetShaderParameter("enabled", false);
                    //greyScale3DShaderMaterial.SetShaderParameter("enabled", false);
                    this.Visible = true;
                    node3D.Visible = true;
                    resourceMeshInstance.Visible = true;
                    if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].district != null)
                    {
                        improvementMeshInstance.Visible = true;
                    }
                }
                else if (Global.gameManager.game.localPlayerRef.seenGameHexDict.ContainsKey(hex))
                {
                    greyScaleShaderMaterial.SetShaderParameter("enabled", true);
                    //greyScale3DShaderMaterial.SetShaderParameter("enabled", true);
                    this.Visible = true;
                    node3D.Visible = true;
                    resourceMeshInstance.Visible = true;
                    if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].district != null)
                    {
                        improvementMeshInstance.Visible = true;
                    }
                }
                else
                {
                    this.Visible = false;
                    node3D.Visible = false;
                    resourceMeshInstance.Visible = false;
                    improvementMeshInstance.Visible = false;
                }
            }
        }
    }
}

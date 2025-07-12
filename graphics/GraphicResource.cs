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
        //GD.Print(resource);
        this.hex = hex;
        Node3D temp = Godot.ResourceLoader.Load<PackedScene>("res://" + ResourceLoader.resources[resource].ModelPath).Instantiate<Node3D>();
        resourceMeshInstance = temp.GetNode<MeshInstance3D>("Resource");
        improvementMeshInstance = temp.GetNode<MeshInstance3D>("Improvement");

        /*        // Load the base material
                ShaderMaterial baseMaterial = meshInstance.GetActiveMaterial(0) as ShaderMaterial;

                // Load the grayscale shader
                Shader greyScale3DShader = GD.Load<Shader>("res://graphics/shaders/general/greyscale3D.gdshader");
                greyScale3DShaderMaterial = new ShaderMaterial();
                greyScale3DShaderMaterial.Shader = greyScale3DShader;

                // Apply the Next Pass
                baseMaterial.NextPass = greyScale3DShaderMaterial;*/
        //meshInstance.MaterialOverlay = greyScale3DShaderMaterial;

        Transform3D newTransform = resourceMeshInstance.Transform;
        GraphicGameBoard ggb = ((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);
        int newQ = (Global.gameManager.game.mainGameBoard.left + (hex.r >> 1) + hex.q) % ggb.chunkSize - (hex.r >> 1);
        Hex modHex = new Hex(newQ, hex.r, -newQ - hex.r);
        Point hexPoint = Global.gameManager.graphicManager.layout.HexToPixel(modHex);
        newTransform.Origin = new Vector3((float)hexPoint.y, 0.0f, (float)hexPoint.x);
        float height = ggb.Vector3ToHeightMapVal(newTransform.Origin); //TODO
        newTransform.Origin = new Vector3((float)hexPoint.y, height, (float)hexPoint.x);
        resourceMeshInstance.Transform = newTransform;
        improvementMeshInstance.Transform = newTransform;
        improvementMeshInstance.Visible = false;

        AddChild(temp);

        node3D = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/ResourceWorldUI.tscn").Instantiate<Node3D>();
        resourceIcon = node3D.GetNode<TextureRect>("SubViewport/ResourceWorldUI/ResourceIcon");
        resourceIcon.Texture = Godot.ResourceLoader.Load<Texture2D>("res://" + ResourceLoader.resources[resource].IconPath);
        Shader greyScaleShader = GD.Load<Shader>("res://graphics/shaders/general/greyscale.gdshader");
        greyScaleShaderMaterial = new ShaderMaterial();
        greyScaleShaderMaterial.Shader = greyScaleShader;
        resourceIcon.Material = greyScaleShaderMaterial;

        newTransform = node3D.Transform;
        newTransform.Origin = new Vector3((float)hexPoint.y, 8, (float)hexPoint.x);
        node3D.Transform = newTransform;
        
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
        if(graphicUpdateType == GraphicUpdateType.Update)
        {
            if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].district != null)
            {
                improved = true;
                improvementMeshInstance.Visible = true;
            }
            else
            {
                improved = false;
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
                    if (improved)
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
                    if (improved)
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

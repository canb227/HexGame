using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Google.Protobuf.Reflection.FeatureSet.Types;

public class HexChunk
{
    public MeshInstance3D mesh;
    public Hex origin;
    public Hex graphicalOrigin;
    private int deltaQ;
    private List<Hex> ourHexes;
    bool firstRun = true;
    public Image heightMap;
    public Image visibilityImage;
    public ImageTexture visibilityTexture;
    public ShaderMaterial terrainShaderMaterial;
    public HexChunk(MeshInstance3D mesh, List<Hex> ourHexes, Hex origin, Hex graphicalOrigin, Image heightMap, ShaderMaterial terrainShaderMaterial, ImageTexture visibilityTexture)
    {
        this.mesh = mesh;
        this.ourHexes = ourHexes;
        this.origin = origin;
        this.graphicalOrigin = graphicalOrigin;
        deltaQ = graphicalOrigin.q - origin.q;
        this.heightMap = heightMap;
        this.terrainShaderMaterial = terrainShaderMaterial;
        this.visibilityTexture = visibilityTexture;
        UpdateGraphicalOrigin(graphicalOrigin);
    }

    public void UpdateGraphicalOrigin(Hex newOrigin)
    {
        if(!newOrigin.Equals(graphicalOrigin) || firstRun)
        {
            graphicalOrigin = newOrigin;
            deltaQ = graphicalOrigin.q - origin.q;
            Transform3D newTransform = mesh.Transform;
            newTransform.Origin = new Vector3((float)Global.layout.HexToPixel(graphicalOrigin).y, -1.0f, (float)Global.layout.HexToPixel(graphicalOrigin).x);
            mesh.Transform = newTransform;
            
            terrainShaderMaterial.SetShaderParameter("chunkOffset", graphicalOrigin.q * Math.Sqrt(3) * 10.0f);
            if (!firstRun)
            {
                GraphicGameBoard ggb = ((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);
                foreach (Hex hex in ourHexes)
                {
                    List<GraphicObject> objectsToUpdate = new();
                    foreach (GraphicObject graphicObj in Global.gameManager.graphicManager.hexObjectDictionary[hex])
                    {
                        if(graphicObj is GraphicUnit)
                        {
                            objectsToUpdate.Add(graphicObj);
                        }
                    }
                    foreach(GraphicObject graphicObj in objectsToUpdate)
                    {
                        graphicObj.UpdateGraphic(GraphicUpdateType.Update);
                    }
                }
            }
            firstRun = false;

        }
    }

    public void GenerateVisibilityGrid(List<Hex> visibleHexes, List<Hex> seenHexes)
    {
        visibilityImage.Fill(new Godot.Color(0, 0, 0, 1)); // Default to hidden, unseen

        foreach (Hex hex in seenHexes)
        {
            visibilityImage.SetPixel(hex.q, hex.r, new Godot.Color(0, 1, 0, 1)); // Mark as seen
        }

        foreach (Hex hex in visibleHexes)
        {
            visibilityImage.SetPixel(hex.q, hex.r, new Godot.Color(1, 0, 0, 1)); // Set visible
        }
        visibilityImage.SavePng("testVis.png");
        visibilityTexture.Update(visibilityImage);
    }

    public Hex HexToGraphicalHex(Hex hex)
    {
        return new Hex(hex.q+deltaQ, hex.r, -(hex.q + deltaQ)-hex.r);
    }
}

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Google.Protobuf.Reflection.FeatureSet.Types;

public class HexChunk
{
    public MultiMeshInstance3D multiMeshInstance;
    public MultiMeshInstance3D yieldMultiMeshInstance;
    public float chunkOffset;
    public Hex origin;
    public Hex graphicalOrigin;
    private int deltaQ;
    private List<Hex> ourHexes;
    bool firstRun = true;
    public ShaderMaterial terrainShaderMaterial;
    public float widthPix;
    public float heightPix;
    public HexChunk(MultiMeshInstance3D multiMeshInstance, MultiMeshInstance3D yieldMultiMeshInstance, List<Hex> ourHexes, Hex origin, Hex graphicalOrigin, ShaderMaterial terrainShaderMaterial, float chunkOffset, float widthPix, float heightPix)
    {
        this.multiMeshInstance = multiMeshInstance;
        this.yieldMultiMeshInstance = yieldMultiMeshInstance;
        this.ourHexes = ourHexes;
        this.origin = origin;
        this.graphicalOrigin = graphicalOrigin;
        deltaQ = graphicalOrigin.q - origin.q;
        this.terrainShaderMaterial = terrainShaderMaterial;
        this.chunkOffset = chunkOffset;
        this.widthPix = widthPix;
        this.heightPix = heightPix;
        UpdateGraphicalOrigin(graphicalOrigin);
    }

    public void UpdateHexYield(Hex hex)
    {
        int index = ourHexes.FindIndex(h => h.Equals(hex));
        Dictionary<YieldType, float> yieldDict = Global.gameManager.game.mainGameBoard.gameHexDict[hex].yields.YieldsToDict();
        for (int l = 0; l < 7; l++)
        {
            yieldMultiMeshInstance.Multimesh.SetInstanceCustomData(index * 7 + l, new Godot.Color(l / 7.0f, yieldDict[(YieldType)l] / 100.0f, hex.q / 255f, hex.r / 255f));//r is type, g is value, b is hex.q, a is hex.r
        }
    }

    public void UpdateGraphicalOrigin(Hex newOrigin)
    {
        if(!newOrigin.Equals(graphicalOrigin) || firstRun)
        {
            graphicalOrigin = newOrigin;
            deltaQ = graphicalOrigin.q - origin.q;
            Transform3D newTransform = multiMeshInstance.Transform;
            newTransform.Origin = new Vector3((float)Global.layout.HexToPixel(graphicalOrigin).y, -1.0f, (float)Global.layout.HexToPixel(graphicalOrigin).x);
            multiMeshInstance.Transform = newTransform;
            
            //terrainShaderMaterial.SetShaderParameter("chunkOffset", graphicalOrigin.q * Math.Sqrt(3) * 10.0f);
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



    public Hex HexToGraphicalHex(Hex hex)
    {
        return new Hex(hex.q+deltaQ, hex.r, -(hex.q + deltaQ)-hex.r);
    }
}

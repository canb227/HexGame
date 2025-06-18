using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public Image heightMap;
    public ShaderMaterial terrainShaderMaterial;
    public float widthPix;
    public float heightPix;
    public HexChunk(MultiMeshInstance3D multiMeshInstance, MultiMeshInstance3D yieldMultiMeshInstance, List<Hex> ourHexes, Hex origin, Hex graphicalOrigin, Image heightMap, ShaderMaterial terrainShaderMaterial, float chunkOffset, float widthPix, float heightPix)
    {
        this.multiMeshInstance = multiMeshInstance;
        this.yieldMultiMeshInstance = yieldMultiMeshInstance;
        this.ourHexes = ourHexes;
        this.origin = origin;
        this.graphicalOrigin = graphicalOrigin;
        deltaQ = graphicalOrigin.q - origin.q;
        this.heightMap = heightMap;
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

    public float Vector3ToHeightMapVal(Vector3 pixel)
    {
        //GD.PushWarning("NOT IMPLEMENTED"); //TODO
        Random rand = new Random();
        return 0.0f;// (float)rand.NextDouble()*2.0f;
        /*        GraphicGameBoard ggb = (GraphicGameBoard)(Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);


                Point graphicalHexPoint = Global.gameManager.graphicManager.layout.HexToPixel(ggb.HexToGraphicHex(unit.hex)); //use for X and Z
                Hex tempHex = unit.hex;
                int newQ = tempHex.q + (tempHex.r >> 1);
                tempHex = new Hex(newQ, tempHex.r, -newQ - tempHex.r);
                Point hexPoint = Global.gameManager.graphicManager.layout.HexToPixel(tempHex); // use for Y
                *//*
                            //calculate our wrapped pixel value since heightmaps are per chunk we use chunkOffset BUT what if we have gone far left or right, how do we wrap the pixel
                            //we calculate the left and right bounds of the whole gameboard (at our pixel?), which should be gameboard.right * some hex size (10.0f or the gap sqrt(3)etc)
                            int chunkIndex = ggb.hexToChunkDictionary[unit.hex];
                            float hoffset = (float)Math.Sqrt(3) * 10.0f / 2.0f;
                            Vector3 nodeOrigin = node3D.Transform.Origin;
                *//*            if (node3D.Transform.Origin.Z < 0)
                            {
                                nodeOrigin.Z = node3D.Transform.Origin.Z - (ggb.chunkList[chunkIndex].widthPix * ggb.chunkCount);
                            }*//*

                            int pixelX = (int)Math.Round((nodeOrigin.Z + hoffset - ggb.chunkList[chunkIndex].chunkOffset));
                            int pixelY = (int)Math.Round(nodeOrigin.X + 5.0f);
                            //GD.Print("Hex Point: " + hexPoint.x + ", " + hexPoint.y + "cum: " + ggb.chunkList[chunkIndex].widthPix);
                            //GD.Print("World Point: " + node3D.Transform.Origin.Z, ", " + node3D.Transform.Origin.X);
                            //GD.Print("Hex Val: " + Global.layout.HexToPixel(unit.hex).x + " Node3d Z Val: " + node3D.Transform.Origin.Z + " wrapped node3d Z Val: " + nodeOrigin.Z);
                            GD.Print((Global.layout.HexToPixel(unit.hex).x + ggb.chunkList[chunkIndex].chunkOffset) + " | " + pixelX );
                            float height = ggb.chunkList[chunkIndex].heightMap.GetPixel(pixelX, pixelY).R * 10.0f;

                            GD.Print(unit.hex + " | " + Global.gameManager.graphicManager.layout.PixelToHex(new Point(pixelX, pixelY)).HexRound());*//*
                int chunkIndex = ggb.hexToChunkDictionary[unit.hex];
                //GD.Print(unit.hex.q + " | " + Global.gameManager.graphicManager.layout.PixelToHex(new Point((int)(hexPoint.x % ggb.chunkList[chunkIndex].widthPix), (int)hexPoint.y)).q);
                GD.Print(-hexPoint.x + " " + (int)(-hexPoint.x % ggb.chunkList[chunkIndex].widthPix));
                float height = ggb.chunkList[chunkIndex].heightMap.GetPixel((int)(-hexPoint.x % ggb.chunkList[chunkIndex].widthPix), (int)hexPoint.y).R;*/
    }

    public Hex HexToGraphicalHex(Hex hex)
    {
        return new Hex(hex.q+deltaQ, hex.r, -(hex.q + deltaQ)-hex.r);
    }
}

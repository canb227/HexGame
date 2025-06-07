using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class HexChunk
{
    public MeshInstance3D mesh;
    public Hex origin;
    public Hex graphicalOrigin;
    private int deltaQ;
    private List<Hex> ourHexes;
    bool firstRun = true;
    public HexChunk(MeshInstance3D mesh, List<Hex> ourHexes, Hex origin, Hex graphicalOrigin)
    {
        this.mesh = mesh;
        this.ourHexes = ourHexes;
        this.origin = origin;
        this.graphicalOrigin = graphicalOrigin;
        deltaQ = graphicalOrigin.q - origin.q;
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
            if(!firstRun)
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
        GD.Print(hex + " " + deltaQ);
        GD.Print("CHUNK HEXES: " + origin + " " + graphicalOrigin);
        return new Hex(hex.q+deltaQ, hex.r, -(hex.q + deltaQ)-hex.r);
    }
}

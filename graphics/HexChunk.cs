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
    public HexChunk(MeshInstance3D mesh, Hex origin, Hex graphicalOrigin)
    {
        this.mesh = mesh;
        this.origin = origin;
        this.graphicalOrigin = graphicalOrigin;
        deltaQ = Math.Abs(origin.q - graphicalOrigin.q);
    }

    public void UpdateGraphicalOrigin(Hex newOrigin)
    {
        graphicalOrigin = newOrigin;
        deltaQ = Math.Abs(origin.q - graphicalOrigin.q);
        Transform3D newTransform = mesh.Transform;
        newTransform.Origin = new Vector3((float)Global.layout.HexToPixel(graphicalOrigin).y, -1.0f, (float)Global.layout.HexToPixel(graphicalOrigin).x);
        mesh.Transform = newTransform;
    }

    public Hex HexToGraphicalHex(Hex hex)
    {
        GD.Print(hex + " " + deltaQ);
        GD.Print("CHUNK HEXES: " + origin + " " + graphicalOrigin);
        return new Hex(hex.q+deltaQ, hex.r, -(hex.q + deltaQ)-hex.r);
    }
}

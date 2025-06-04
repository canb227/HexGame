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
        deltaQ = origin.q + graphicalOrigin.q;
    }

    public Hex HexToGraphicalHex(Hex hex)
    {
        GD.Print(hex + " " + deltaQ);
        return new Hex(hex.q+deltaQ, hex.r, -(hex.q + deltaQ)-hex.r);
    }
}

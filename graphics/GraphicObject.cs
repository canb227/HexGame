using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public enum GraphicUpdateType
{
    Add,
    Remove,
    Update,
    Move,
    Attack
}
public abstract partial class GraphicObject : Node3D
{
    public int logicalID;
    public Mesh myMesh;
    public Rid meshInstance;
    public override void _Ready()
    {
        // Create a visual instance (for 3D).
        //Rid meshInstance = RenderingServer.InstanceCreate();
        // Set the scenario from the world, this ensures it
        // appears with the same objects as the scene.
        //Rid scenario = GetWorld3D().Scenario;
        //RenderingServer.InstanceSetScenario(meshInstance, scenario);
        // Add a mesh to it.
        // Remember, keep the reference.
        //myMesh = Godot.ResourceLoader.Load<Mesh>("res://MyMesh.obj");
        //RenderingServer.InstanceSetBase(meshInstance, myMesh.GetRid());
        // Move the mesh around.
        //Transform3D xform = new Transform3D(Basis.Identity, new Vector3(20, 100, 0));
        //RenderingServer.InstanceSetTransform(meshInstance, xform);
    }

    public abstract void UpdateGraphic(GraphicUpdateType graphicUpdateType);

    public abstract void Unselected();
    public abstract void Selected();

    public override void _Process(double delta)
    {

    }
}
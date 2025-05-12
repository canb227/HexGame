using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static Godot.Projection;

public partial class GraphicUnit : GraphicObject
{
    public Unit unit;
    public Node3D node3D;
    public Layout layout;
    public Label3D healthNumber;
    public GraphicUnit(Unit unit, Layout layout)
    {
        this.unit = unit;
        this.layout = layout;
        node3D = new Node3D();
        InstantiateUnit(unit);
        InstantiateUnitUI(unit);
    }

    public override void UpdateGraphic(GraphicUpdateType graphicUpdateType)
    {
        if (graphicUpdateType == GraphicUpdateType.Remove)
        {
            Free();
        }
        else if (graphicUpdateType == GraphicUpdateType.Move)
        {
            Transform3D newTransform = node3D.Transform;
            Point hexPoint = layout.HexToPixel(unit.gameHex.hex);
            newTransform.Origin = new Vector3((float)hexPoint.y, 1, (float)hexPoint.x);
            node3D.Transform = newTransform;
        }
        else if (graphicUpdateType == GraphicUpdateType.Update)
        {
            healthNumber.Text = unit.health.ToString();
        }
    }

    private void InstantiateUnit(Unit unit)
    {

        if (unit.unitType == UnitType.Scout)
        {
            node3D = Godot.ResourceLoader.Load<PackedScene>("res://graphics/models/baseperson_remesh3.glb").Instantiate<Node3D>();
        }
        Transform3D newTransform = node3D.Transform;
        Point hexPoint = layout.HexToPixel(unit.gameHex.hex);
        newTransform.Origin = new Vector3((float)hexPoint.y, 1, (float)hexPoint.x);
        node3D.Transform = newTransform;
        AddChild(node3D);
    }

    private void InstantiateUnitUI(Unit unit)
    {
        healthNumber = new Label3D();
        healthNumber.Text = unit.health.ToString();
        healthNumber.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;

        Transform3D newTransform = healthNumber.Transform;
        newTransform.Origin = new Vector3(0, 11, 0);
        healthNumber.Transform = newTransform;
        node3D.AddChild(healthNumber);
    }

    public override void Unselected()
    {
        Transform3D newTransform = node3D.Transform;
        Point hexPoint = layout.HexToPixel(unit.gameHex.hex);
        newTransform.Origin = new Vector3((float)hexPoint.y, 1, (float)hexPoint.x);
        node3D.Transform = newTransform;
        foreach (Node3D child in GetChildren())
        {
            if(child.Name == "MovementRangeHexes")
            {
                child.Free();
            }
            else if(child.Name == "MovementRangeLines")
            {
                child.Free(); 
            }
        }
    }

    public override void Selected()
    {
        Transform3D newTransform = node3D.Transform;
        Point hexPoint = layout.HexToPixel(unit.gameHex.hex);
        newTransform.Origin = new Vector3((float)hexPoint.y, 10, (float)hexPoint.x);
        node3D.Transform = newTransform;
        GenerateHexLines();
        GenerateHexTriangles();
    }

    public override void ProcessRightClick(Hex hex)
    {
        unit.MoveTowards(unit.gameHex.gameBoard.gameHexDict[hex], unit.gameHex.gameBoard.game.teamManager, unit.gameHex.gameBoard.gameHexDict[hex].IsEnemyPresent(unit.teamNum));
        Transform3D newTransform = node3D.Transform;
        Point hexPoint = layout.HexToPixel(unit.gameHex.hex);
        newTransform.Origin = new Vector3((float)hexPoint.y, 10, (float)hexPoint.x);
        node3D.Transform = newTransform;
        Unselected();
        Selected();
    }




    private void GenerateHexLines()
    {
        MeshInstance3D lines = new MeshInstance3D();

        SurfaceTool st = new SurfaceTool();

        st.Begin(Mesh.PrimitiveType.Lines);
        st.SetColor(Godot.Colors.Red);


        foreach (Hex hex in unit.MovementRange().Keys)
        {
            List<Point> points = layout.PolygonCorners(hex);
            st.AddVertex(new Vector3((float)points[0].y, 0.1f, (float)points[0].x));
            foreach (Point point in points)
            {
                Vector3 temp = new Vector3((float)point.y, 0.1f, (float)point.x);
                //GD.Print(temp);
                st.AddVertex(temp);
                st.AddVertex(temp);

            }
            st.AddVertex(new Vector3((float)points[0].y, 0.1f, (float)points[0].x));
        }
        //st.GenerateNormals();
        lines.Mesh = st.Commit();
        lines.Name = "MovementRangeLines";
        AddChild(lines);
    }

    private void GenerateHexTriangles()
    {
        MeshInstance3D triangles = new MeshInstance3D();
        
        SurfaceTool st = new SurfaceTool();
        st.Begin(Mesh.PrimitiveType.Triangles);
        st.SetColor(Godot.Colors.Gold);


        foreach (Hex hex in unit.MovementRange().Keys)
        {
            if(unit.gameHex.gameBoard.gameHexDict[hex].IsEnemyPresent(unit.teamNum))
            {
                st.SetColor(Godot.Colors.Red);
            }
            else
            {
                st.SetColor(Godot.Colors.Gold);
            }
            List<Point> points = layout.PolygonCorners(hex);

            Vector3 origin = new Vector3((float)points[0].y, 0.05f, (float)points[0].x);
            for (int i = 1; i < 6; i++)
            {
                st.AddVertex(origin); // Add the origin point as the first vertex for the triangle fan

                Vector3 pointTwo = new Vector3((float)points[i].y, 0.05f, (float)points[i].x); // Get the next point in the polygon
                st.AddVertex(pointTwo); // Add the next point in the polygon as the second vertex for the triangle fan

                Vector3 pointThree = new Vector3((float)points[i - 1].y, 0.05f, (float)points[i - 1].x);
                st.AddVertex(pointThree); // Add the next point in the polygon as the third vertex for the triangle fan
            }
        }
        st.GenerateNormals();

        triangles.Mesh = st.Commit();
        StandardMaterial3D material = new StandardMaterial3D();
        material.VertexColorUseAsAlbedo = true;
        triangles.SetSurfaceOverrideMaterial(0, material);
        triangles.Name = "MovementRangeHexes";
        AddChild(triangles);
    }

    public override void _Process(double delta)
    {

    }
}
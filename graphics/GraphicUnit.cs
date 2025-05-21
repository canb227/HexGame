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
    public GraphicManager graphicManager;
    public Label3D healthNumber;
    public Label3D movementNumber;
    public Label3D attacksNumber;
    public GraphicUnit(Unit unit, GraphicManager graphicManager)
    {
        this.unit = unit;
        this.graphicManager = graphicManager;
        node3D = new Node3D();
        InstantiateUnit(unit);
        InstantiateUnitUI(unit);
    }

    public override void UpdateGraphic(GraphicUpdateType graphicUpdateType)
    {
        if (graphicUpdateType == GraphicUpdateType.Remove)
        {
            graphicManager.UnselectObject();
            Free();
        }
        else if (graphicUpdateType == GraphicUpdateType.Move)
        {
            Transform3D newTransform = node3D.Transform;
            Point hexPoint = graphicManager.layout.HexToPixel(unit.gameHex.hex);
            newTransform.Origin = new Vector3((float)hexPoint.y, 1, (float)hexPoint.x);
            node3D.Transform = newTransform;
        }
        else if (graphicUpdateType == GraphicUpdateType.Update)
        {
            healthNumber.Text = "Health: " + unit.health.ToString();
            movementNumber.Text = "Movement Left: " + unit.remainingMovement.ToString();
            attacksNumber.Text = "Attacks Left: " + unit.attacksLeft.ToString();
            UpdateMovementGraphics();
        }
    }

    private void InstantiateUnit(Unit unit)
    {
        UnitLoader.unitsDict.TryGetValue(unit.unitType, out UnitInfo unitInfo);
        node3D = Godot.ResourceLoader.Load<PackedScene>("res://" + unitInfo.ModelPath).Instantiate<Node3D>();
        Transform3D newTransform = node3D.Transform;
        Point hexPoint = graphicManager.layout.HexToPixel(unit.gameHex.hex);
        newTransform.Origin = new Vector3((float)hexPoint.y, 1, (float)hexPoint.x);
        node3D.Transform = newTransform;
        AddChild(node3D);
    }

    private void InstantiateUnitUI(Unit unit)
    {
        //health
        healthNumber = new Label3D();
        healthNumber.Text = "Health: " + unit.health.ToString();
        healthNumber.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;

        Transform3D newTransform = healthNumber.Transform;
        newTransform.Origin = new Vector3(0, 8, 0);
        healthNumber.Transform = newTransform;
        node3D.AddChild(healthNumber);

        //movement
        movementNumber = new Label3D();
        movementNumber.Text = "Movement Left: " + unit.remainingMovement.ToString();
        movementNumber.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;

        newTransform = movementNumber.Transform;
        newTransform.Origin = new Vector3(0, 8.3f, 0);
        movementNumber.Transform = newTransform;
        node3D.AddChild(movementNumber);

        //attacks
        attacksNumber = new Label3D();
        attacksNumber.Text = "Attacks Left: " + unit.attacksLeft.ToString();
        attacksNumber.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;

        newTransform = attacksNumber.Transform;
        newTransform.Origin = new Vector3(0, 8.6f, 0);
        attacksNumber.Transform = newTransform;
        node3D.AddChild(attacksNumber);
    }

    public override void Unselected()
    {
        Transform3D newTransform = node3D.Transform;
        Point hexPoint = graphicManager.layout.HexToPixel(unit.gameHex.hex);
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
            else if (child.Name == "AbilityTargetingLines")
            {
                child.Free();
            }
            else if (child.Name == "AbilityTargetHexes")
            {
                child.Free();
            }
        }
        graphicManager.uiManager.UnitUnselected(unit);
    }

    public override void Selected()
    {
        Transform3D newTransform = node3D.Transform;
        Point hexPoint = graphicManager.layout.HexToPixel(unit.gameHex.hex);
        newTransform.Origin = new Vector3((float)hexPoint.y, 2, (float)hexPoint.x);
        node3D.Transform = newTransform;
        GenerateHexLines(unit.MovementRange());
        GenerateHexTriangles(unit.MovementRange());
        graphicManager.uiManager.UnitSelected(unit);
    }

    public void UpdateMovementGraphics()
    {
        bool hadMovementRangeHexes = false;
        bool hadMovementRangeLines = false;
        foreach (Node3D child in GetChildren())
        {
            if (child.Name == "MovementRangeHexes")
            {
                hadMovementRangeHexes = true;
                child.Free();
            }
            else if (child.Name == "MovementRangeLines")
            {
                hadMovementRangeLines = true;
                child.Free();
            }
/*            else if (child.Name == "AbilityTargetingLines")
            {
                child.Free();
            }
            else if (child.Name == "AbilityTargetHexes")
            {
                child.Free();
            }*/
        }
        if(hadMovementRangeLines)
        {
            GenerateHexLines(unit.MovementRange());
        }
        if (hadMovementRangeHexes)
        {
            GenerateHexTriangles(unit.MovementRange());
        }
    }


    public override void ProcessRightClick(Hex hex)
    {
        unit.MoveTowards(unit.gameHex.gameBoard.gameHexDict[hex], unit.gameHex.gameBoard.game.teamManager, unit.gameHex.gameBoard.gameHexDict[hex].IsEnemyPresent(unit.teamNum));
        Transform3D newTransform = node3D.Transform;
        Point hexPoint = graphicManager.layout.HexToPixel(unit.gameHex.hex);
        newTransform.Origin = new Vector3((float)hexPoint.y, 10, (float)hexPoint.x);
        node3D.Transform = newTransform;
        Unselected();
        Selected();
    }

    public void GenerateTargetingPrompt(UnitAbility ability)
    {
        List<Hex> hexes = new List<Hex>();
        foreach(Hex hex in unit.gameHex.hex.WrappingRange(ability.range+1, unit.gameHex.gameBoard.left, unit.gameHex.gameBoard.right, unit.gameHex.gameBoard.top, unit.gameHex.gameBoard.bottom))
        {
            if (ability.validTargetTypes.IsHexValidTarget(unit.gameHex.gameBoard.gameHexDict[hex], unit))
            {
                hexes.Add(hex);
            }
        }

        if(hexes.Count > 0)
        {
            graphicManager.SetWaitForTargeting(true);
            graphicManager.waitingAbility = ability;
            GenerateHexSelectionLines(hexes);
            GenerateHexSelectionTriangles(hexes);
        }
    }

    public void RemoveTargetingPrompt()
    {
        foreach (Node3D child in GetChildren())
        {
            if (child.Name == "AbilityTargetingLines")
            {
                child.Free();
            }
            else if (child.Name == "AbilityTargetHexes")
            {
                child.Free();
            }
        }
    }

    private void GenerateHexLines(Dictionary<Hex, float> hexes)
    {
        GenerateHexLines(hexes.Keys.ToList());
    }

    private void GenerateHexTriangles(Dictionary<Hex, float> hexes)
    {
        GenerateHexTriangles(hexes.Keys.ToList());
    }

    private void GenerateHexLines(List<Hex> hexes)
    {
        MeshInstance3D lines = new MeshInstance3D();

        SurfaceTool st = new SurfaceTool();

        st.Begin(Mesh.PrimitiveType.Lines);
        st.SetColor(Godot.Colors.Red);


        foreach (Hex hex in hexes)
        {
            List<Point> points = graphicManager.layout.PolygonCorners(hex);
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

    private void GenerateHexTriangles(List<Hex> hexes)
    {
        MeshInstance3D triangles = new MeshInstance3D();
        
        SurfaceTool st = new SurfaceTool();
        st.Begin(Mesh.PrimitiveType.Triangles);
        st.SetColor(Godot.Colors.Gold);


        foreach (Hex hex in hexes)
        {
            if(unit.gameHex.gameBoard.gameHexDict[hex].IsEnemyPresent(unit.teamNum))
            {
                st.SetColor(Godot.Colors.Red);
            }
            else
            {
                st.SetColor(Godot.Colors.Gold);
            }
            List<Point> points = graphicManager.layout.PolygonCorners(hex);

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

    private void GenerateHexSelectionLines(List<Hex> hexes)
    {
        MeshInstance3D lines = new MeshInstance3D();

        SurfaceTool st = new SurfaceTool();

        st.Begin(Mesh.PrimitiveType.Lines);
        st.SetColor(Godot.Colors.Gold);


        foreach (Hex hex in hexes)
        {
            List<Point> points = graphicManager.layout.PolygonCorners(hex);
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
        lines.Name = "AbilityTargetingLines";
        AddChild(lines);
    }

    private void GenerateHexSelectionTriangles(List<Hex> hexes)
    {
        MeshInstance3D triangles = new MeshInstance3D();

        SurfaceTool st = new SurfaceTool();
        st.Begin(Mesh.PrimitiveType.Triangles);
        st.SetColor(Godot.Colors.BlueViolet);


        foreach (Hex hex in hexes)
        {
            List<Point> points = graphicManager.layout.PolygonCorners(hex);

            Vector3 origin = new Vector3((float)points[0].y, 0.15f, (float)points[0].x);
            for (int i = 1; i < 6; i++)
            {
                st.AddVertex(origin); // Add the origin point as the first vertex for the triangle fan

                Vector3 pointTwo = new Vector3((float)points[i].y, 0.15f, (float)points[i].x); // Get the next point in the polygon
                st.AddVertex(pointTwo); // Add the next point in the polygon as the second vertex for the triangle fan

                Vector3 pointThree = new Vector3((float)points[i - 1].y, 0.15f, (float)points[i - 1].x);
                st.AddVertex(pointThree); // Add the next point in the polygon as the third vertex for the triangle fan
            }
        }
        st.GenerateNormals();

        triangles.Mesh = st.Commit();
        StandardMaterial3D material = new StandardMaterial3D();
        material.VertexColorUseAsAlbedo = true;
        if (triangles.GetSurfaceOverrideMaterialCount() != 0)
        {
            triangles.SetSurfaceOverrideMaterial(0, material);
            triangles.Name = "AbilityTargetHexes";
            AddChild(triangles);
        }
    }

    public override void _Process(double delta)
    {

    }
}
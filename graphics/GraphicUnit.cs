using Godot;
using NetworkMessages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using static Godot.Projection;

public partial class GraphicUnit : GraphicObject
{
    public Unit unit;
    public Node3D node3D;
    public UnitAbility waitingAbility;
    public UnitWorldUI unitWorldUI;
    public Hex graphicalHex;
    public GraphicUnit(Unit unit)
    {
        this.unit = unit;
        node3D = new Node3D();
        //InstantiateUnitUI(unit);
        unitWorldUI = new UnitWorldUI(unit);
        AddChild(unitWorldUI);
        UpdateGraphic(GraphicUpdateType.Visibility);
    }
    public override void _Ready()
    {
        InstantiateUnit(unit);
    }

    public override void UpdateGraphic(GraphicUpdateType graphicUpdateType)
    {
        if (!IsInstanceValid(this)) { return; }
        if (graphicUpdateType == GraphicUpdateType.Remove)
        {
            if(Global.gameManager.graphicManager.selectedObjectID == unit.id)
            {
                Global.gameManager.graphicManager.UnselectObject();
            }
            Visible = false;
            Global.gameManager.graphicManager.toBeDeleted.Add(unit.id, this);
            Global.gameManager.graphicManager.hexObjectDictionary[unit.hex].Remove(this);
            //Free();
        }
        else if (graphicUpdateType == GraphicUpdateType.Move || graphicUpdateType == GraphicUpdateType.Update)
        {
            Transform3D newTransform = node3D.Transform;
            GraphicGameBoard ggb = (GraphicGameBoard)(Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);
            Point graphicalHexPoint = Global.gameManager.graphicManager.layout.HexToPixel(ggb.HexToGraphicHex(unit.hex));
            float height = ggb.Vector3ToHeightMapVal(node3D.Transform.Origin);
            newTransform.Origin = new Vector3((float)graphicalHexPoint.y, height, (float)graphicalHexPoint.x);


            //newTransform.Origin = new Vector3((float)hexPoint.y, height, (float)hexPoint.x);
            node3D.Transform = newTransform;
            UpdateMovementGraphics();
            unitWorldUI.Update();
        }
        else if (graphicUpdateType == GraphicUpdateType.Visibility)
        {
            if (Global.gameManager.game.localPlayerRef.visibleGameHexDict.ContainsKey(unit.hex))
            {
                this.Visible = true;
                unitWorldUI.Visible = true;
            }
            else
            {
                this.Visible = false;
                unitWorldUI.Visible = false;
            }
        }
    }

    private void InstantiateUnit(Unit unit)
    {
        UnitLoader.unitsDict.TryGetValue(unit.unitType, out UnitInfo unitInfo);
        node3D = Godot.ResourceLoader.Load<PackedScene>("res://" + unitInfo.ModelPath).Instantiate<Node3D>();
        Transform3D newTransform = node3D.Transform;
        GraphicGameBoard ggb = (GraphicGameBoard)(Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);
        Point hexPoint = Global.gameManager.graphicManager.layout.HexToPixel(ggb.HexToGraphicHex(unit.hex));
        newTransform.Origin = new Vector3((float)hexPoint.y, 1, (float)hexPoint.x);
        node3D.Transform = newTransform;
        Global.gameManager.graphicManager.hexObjectDictionary[unit.hex].Add(this);
        AddChild(node3D);
        if(unit.name == "Founder" && unit.teamNum == Global.gameManager.game.localPlayerTeamNum)
        {
            Global.camera.SetHexTarget(unit.hex);
        }
    }

    private void InstantiateUnitUI(Unit unit)
    {
    }

    public override void Unselected()
    {
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
            else if (child.Name.ToString().Contains("TargetingLines") || child.Name.ToString().Contains("TargetHexes"))
            {
                child.Free();
            }
        }
        Global.gameManager.graphicManager.uiManager.UnitUnselected(unit);
    }

    public override void Selected()
    {
        if(unit.teamNum == Global.gameManager.game.localPlayerTeamNum)
        {
            GenerateHexLines(unit.MovementRange());
            GenerateHexTriangles(unit.MovementRange());
        }
        Global.gameManager.graphicManager.uiManager.UnitSelected(unit);
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
        hex = hex.WrapHex();
        Global.gameManager.MoveUnit(unit.id, hex, Global.gameManager.game.mainGameBoard.gameHexDict[hex].IsEnemyPresent(unit.teamNum)); //networked command
        Global.gameManager.graphicManager.graphicObjectDictionary[unit.id].UpdateGraphic(GraphicUpdateType.Move);
/*        foreach (Unit unit in Global.gameManager.game.unitDictionary.Values)
        {
            Global.gameManager.graphicManager.graphicObjectDictionary[unit.id].UpdateGraphic(GraphicUpdateType.Move);
        }*/
        Unselected();
        Selected();
    }

    public void GenerateTargetingPrompt(UnitAbility ability)
    {
        List<Hex> hexes = new List<Hex>();
        foreach(Hex hex in unit.hex.WrappingRange(ability.range+1, Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.top, Global.gameManager.game.mainGameBoard.bottom))
        {
            if (ability.validTargetTypes.IsHexValidTarget(Global.gameManager.game.mainGameBoard.gameHexDict[hex], unit))
            {
                hexes.Add(hex);
            }
        }

        if(hexes.Count > 0)
        {
            Global.gameManager.graphicManager.SetWaitForTargeting(true);
            Global.gameManager.graphicManager.uiManager.HideGenericUIForTargeting();
            Global.gameManager.graphicManager.HideAllCityWorldUI();
            waitingAbility = ability;
            AddChild(Global.gameManager.graphicManager.GenerateHexSelectionLines(hexes, Godot.Colors.Gold, "UnitMove"));
            AddChild(Global.gameManager.graphicManager.GenerateHexSelectionTriangles(hexes, Godot.Colors.BlueViolet, "UnitMove"));
        }
    }

    public override void RemoveTargetingPrompt()
    {
        Global.gameManager.graphicManager.uiManager.ShowGenericUIAfterTargeting();
        Global.gameManager.graphicManager.ShowAllWorldUI();
        foreach (Node3D child in GetChildren())
        {
            if (child.Name.ToString().Contains("TargetingLines") || child.Name.ToString().Contains("TargetHexes"))
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
            GraphicGameBoard ggb = ((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);
            Hex graphicHex = ggb.HexToGraphicHex(hex);
            List<Point> points = Global.gameManager.graphicManager.layout.PolygonCorners(graphicHex);
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
            GraphicGameBoard ggb = ((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);
            Hex graphicHex = ggb.HexToGraphicHex(hex);
            if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].IsEnemyPresent(unit.teamNum))
            {
                st.SetColor(Godot.Colors.Red);
            }
            else
            {
                st.SetColor(Godot.Colors.Gold);
            }
            List<Point> points = Global.gameManager.graphicManager.layout.PolygonCorners(graphicHex);

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

    public void SetWorldUIVisibility(bool visible)
    {
        //unitWorldUI.Visible = visible;
    }

    public override void _Process(double delta)
    {

    }
}
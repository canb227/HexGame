using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public partial class GraphicGameBoard : GraphicObject
{
    public GameBoard gameBoard;
    Layout layout;
    private GraphicManager graphicManager;
    public GraphicGameBoard(GameBoard gameBoard, GraphicManager graphicManager, Layout layout)
    {
        this.graphicManager = graphicManager;
        this.gameBoard = gameBoard;
        this.layout = layout;
        DrawBoard(layout);
    }
    public override void _Ready()
    {
        
    }

    public override void UpdateGraphic(GraphicUpdateType graphicUpdateType)
    {
        //placeholder
        SimpleRedrawBoard(layout);
    }

    //super simple just delete all our children and redraw the board using the current state of gameboard
    private void SimpleRedrawBoard(Layout pointy)
    {
        foreach (Node child in this.GetChildren())
        {
            if(child.Name == "GameBoardTerrain" || child.Name == "GameBoardTerrainLines" || child.Name == "GameBoardTerrainFog2" || child.Name == "GameBoardTerrainFog")
            {
                child.Free();
            }
        }
        List<Hex> seen = Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].seenGameHexDict.Keys.ToList();
        List<Hex> visible = Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].visibleGameHexDict.Keys.ToList();
        List<Hex> all = gameBoard.gameHexDict.Keys.ToList();

        HashSet<Hex> seenHexSet = new HashSet<Hex>(seen);
        List<Hex> nonSeenHexes = all.Where(hex => !seenHexSet.Contains(hex)).ToList();

        List<Hex> seenButNotVisible = seen.Except(visible).ToList();
        AddBoard(Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].seenGameHexDict.Keys.ToList(), pointy, 0);
        AddBoardFog(seenButNotVisible, nonSeenHexes, pointy, 0.5f);
        graphicManager.UpdateVisibility();
    }

    private void DrawBoard(Layout pointy)
    {
        List<Hex> seen = Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].seenGameHexDict.Keys.ToList();
        List<Hex> visible = Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].visibleGameHexDict.Keys.ToList();
        List<Hex> all = gameBoard.gameHexDict.Keys.ToList();

        HashSet<Hex> seenHexSet = new HashSet<Hex>(seen);
        List<Hex> nonSeenHexes = all.Where(hex => !seenHexSet.Contains(hex)).ToList();

        List<Hex> seenButNotVisible = seen.Except(visible).ToList();

        AddBoard(seen, pointy, 0);
        AddBoardFog(seenButNotVisible, nonSeenHexes, pointy, 0.5f);



        //AddHexTemperature(pointy);
        AddHexFeatures(pointy);
        AddHexUnits(pointy);
        //AddHexType(pointy);
        AddHexDistrictsAndCities(pointy);
        //AddHexCoords(pointy);
        //AddHexYields(pointy);
    }

    private void AddBoard(List<Hex> hexList, Layout pointy, float height)
    {
        MeshInstance3D triangles = new MeshInstance3D();
        triangles.Mesh = GenerateHexTriangles(hexList, pointy, 0);
        StandardMaterial3D material = new StandardMaterial3D();
        material.VertexColorUseAsAlbedo = true;
        triangles.SetSurfaceOverrideMaterial(0, material);
        triangles.Name = "GameBoardTerrain";
        AddChild(triangles);

        MeshInstance3D lines = new MeshInstance3D();
        lines.Mesh = GenerateHexLines(hexList, pointy, 0.01f);
        lines.Name = "GameBoardTerrainLines";
        AddChild(lines);
    }

    private void AddBoardFog(List<Hex> seenButNotVisible, List<Hex> nonSeenHexes, Layout pointy, float height)
    {
        if (seenButNotVisible.Count != 0)
        {
            MeshInstance3D triangles = new MeshInstance3D();
            triangles.Mesh = GenerateHexTrianglesFog(seenButNotVisible, pointy, 0.5f, new Godot.Color(0.0f, 0.0f, 0.0f, 0.5f));
            StandardMaterial3D material = new StandardMaterial3D();
            material.VertexColorUseAsAlbedo = true;
            material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
            triangles.SetSurfaceOverrideMaterial(0, material);
            triangles.Name = "GameBoardTerrainFog";
            AddChild(triangles);
        }
        if (nonSeenHexes.Count != 0)
        {
            MeshInstance3D triangles = new MeshInstance3D();
            triangles.Mesh = GenerateHexTrianglesFog(nonSeenHexes, pointy, 0.5f, new Godot.Color(0.6f, 0.6f, 0.6f, 1.0f));
            StandardMaterial3D material = new StandardMaterial3D();
            material.VertexColorUseAsAlbedo = true;
            triangles.SetSurfaceOverrideMaterial(0, material);
            triangles.Name = "GameBoardTerrainFog2";
            AddChild(triangles);
        }
        

        
    }

    private void AddHexYields(Layout layout)
    {
        foreach (Hex hex in gameBoard.gameHexDict.Keys)
        {
            Point point = layout.HexToPixel(hex);
            Label3D lbl = new Label3D();
            lbl.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
            lbl.FontSize = 100;
            lbl.Position = new Vector3((float)point.y, .5f, (float)point.x - 3);
            lbl.Text = hex.q.ToString() + "," + hex.r.ToString() + "," + hex.s.ToString();
            AddChild(lbl);
        }
    }

    private void AddHexCoords(Layout layout)
    {
        foreach (Hex hex in gameBoard.gameHexDict.Keys)
        {
            Point point = layout.HexToPixel(hex);
            Label3D lbl = new Label3D();
            lbl.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
            lbl.FontSize = 100;
            lbl.Position = new Vector3((float)point.y, .5f, (float)point.x + 3);

            Yields yields = gameBoard.gameHexDict[hex].yields;

            lbl.Text = "Yields: " + yields.food + "F," + yields.production + "P," + yields.gold + "G," + yields.science + "S," + yields.culture + "C," + yields.happiness + "H";
            //lbl.Text = "Yields: 1F, 1P, 1G, 1S, 1C, 1F";
            AddChild(lbl);
        }
    }

    private void AddHexType(Layout layout)
    {
        foreach (Hex hex in gameBoard.gameHexDict.Keys)
        {
            Point point = layout.HexToPixel(hex);
            TerrainTemperature temp = gameBoard.gameHexDict[hex].terrainTemp;
            Label3D lbl = new Label3D();
            lbl.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
            lbl.FontSize = 100;
            lbl.Position = new Vector3((float)point.y + 2, 1f, (float)point.x - 1);
            switch (gameBoard.gameHexDict[hex].terrainType)
            {
                case TerrainType.Flat:
                    lbl.Text = "Type: Flat";
                    break;
                case TerrainType.Rough:
                    lbl.Text = "Type: Rough";
                    break;
                case TerrainType.Mountain:
                    lbl.Text = "Type: Mountain";
                    break;
                case TerrainType.Coast:
                    lbl.Text = "Type: Coast";
                    break;
                case TerrainType.Ocean:
                    lbl.Text = "Type: Ocean";
                    break;
                default:
                    break;


            }
            AddChild(lbl);
        }
    }

    private void AddHexDistrictsAndCities(Layout layout)
    {
        foreach (Hex hex in gameBoard.gameHexDict.Keys)
        {
            /*            Point point = layout.HexToPixel(hex);
                        Label3D districtLabel = new Label3D();
                        districtLabel.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
                        districtLabel.FontSize = 100;
                        districtLabel.Position = new Vector3((float)point.y, 1f, (float)point.x);*/
            
            if (gameBoard.gameHexDict[hex].district != null)
            {
                if (gameBoard.gameHexDict[hex].district.isCityCenter)
                {
                    graphicManager.NewCity(Global.gameManager.game.cityDictionary[gameBoard.gameHexDict[hex].district.cityID]);
                    graphicManager.NewDistrict(gameBoard.gameHexDict[hex].district);
                }
                else
                {
                    graphicManager.NewDistrict(gameBoard.gameHexDict[hex].district);
                }
            }
        }
    }

    private void AddHexUnits(Layout layout)
    {
        foreach (Hex hex in gameBoard.gameHexDict.Keys)
        {
            foreach (int unitID in gameBoard.gameHexDict[hex].units)
            {
                Unit unit = Global.gameManager.game.unitDictionary[unitID];
                graphicManager.NewUnit(unit);
            }
        }
    }

    private void AddHexFeatures(Layout layout)
    {
        foreach (Hex hex in gameBoard.gameHexDict.Keys)
        {
            Point point = layout.HexToPixel(hex);
            TerrainTemperature temp = gameBoard.gameHexDict[hex].terrainTemp;
            int xoffset = 1;
            foreach (FeatureType feature in gameBoard.gameHexDict[hex].featureSet)
            {
                Label3D lbl = new Label3D();
                lbl.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
                lbl.FontSize = 100;
                lbl.Position = new Vector3((float)point.y - 2, 1f, (float)point.x - 2);
                lbl.Position = new Vector3((float)point.y - 2, 1f, (float)point.x - 2 + xoffset++);
                switch (feature)
                {
                    case FeatureType.Forest:
                        lbl.Text = "Forest";
                        break;
                    case FeatureType.River:
                        lbl.Text = "River";
                        break;
                    case FeatureType.Road:
                        lbl.Text = "Road";
                        break;
                    case FeatureType.Coral:
                        lbl.Text = "Coral";
                        break;
                    default:
                        break;
                }
                AddChild(lbl);
            }
        }
    }

    private void AddHexTemperature(Layout layout)
    {

        foreach (Hex hex in gameBoard.gameHexDict.Keys)
        {
            Point point = layout.HexToPixel(hex);
            TerrainTemperature temp = gameBoard.gameHexDict[hex].terrainTemp;
            Label3D lbl = new Label3D();
            lbl.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
            lbl.FontSize = 100;
            lbl.Position = new Vector3((float)point.y + 2, 1f, (float)point.x + 2);
            switch (temp)
            {
                case TerrainTemperature.Desert:
                    lbl.Text = "Temp: Desert";
                    break;
                case TerrainTemperature.Grassland:
                    lbl.Text = "Temp: Grasslands";
                    break;
                case TerrainTemperature.Plains:
                    lbl.Text = "Temp: Plains";
                    break;
                case TerrainTemperature.Tundra:
                    lbl.Text = "Temp: Tundra";
                    break;
                case TerrainTemperature.Arctic:
                    lbl.Text = "Temp: Arctic";
                    break;
                default:
                    break;
            }
            AddChild(lbl);
        }

    }
    ArrayMesh GenerateHexLines(List<Hex> hexList, Layout layout, float height)
    {

        SurfaceTool st = new SurfaceTool();

        st.Begin(Mesh.PrimitiveType.Lines);

        foreach (Hex hex in hexList)
        {
            List<Point> points = layout.PolygonCorners(hex);
            st.AddVertex(new Vector3((float)points[0].y, 0.01f, (float)points[0].x));
            foreach (Point point in points)
            {
                Vector3 temp = new Vector3((float)point.y, 0.01f, (float)point.x);
                st.AddVertex(temp);
                st.AddVertex(temp);

            }
            st.AddVertex(new Vector3((float)points[0].y, 0.01f, (float)points[0].x));
        }
        //st.GenerateNormals();
        return st.Commit();
    }

    ArrayMesh GenerateHexTriangles(List<Hex> hexList, Layout layout, float height)
    {
        SurfaceTool st = new SurfaceTool();
        st.Begin(Mesh.PrimitiveType.Triangles);


        foreach (Hex hex in hexList)
        {
            switch (gameBoard.gameHexDict[hex].terrainType)
            {
                case TerrainType.Flat:
                    st.SetColor(Godot.Colors.ForestGreen);
                    break;
                case TerrainType.Rough:
                    st.SetColor(Godot.Colors.SaddleBrown);
                    break;
                case TerrainType.Mountain:
                    st.SetColor(Godot.Colors.Black);
                    break;
                case TerrainType.Coast:
                    st.SetColor(Godot.Colors.LightBlue);
                    break;
                case TerrainType.Ocean:
                    st.SetColor(Godot.Colors.DarkBlue);
                    break;
                default:
                    break;
            }

            List<Point> points = layout.PolygonCorners(hex);

            Vector3 origin = new Vector3((float)points[0].y, height, (float)points[0].x);
            for (int i = 1; i < 6; i++)
            {
                st.AddVertex(origin); // Add the origin point as the first vertex for the triangle fan

                Vector3 pointTwo = new Vector3((float)points[i].y, height, (float)points[i].x); // Get the next point in the polygon
                st.AddVertex(pointTwo); // Add the next point in the polygon as the second vertex for the triangle fan

                Vector3 pointThree = new Vector3((float)points[i - 1].y, height, (float)points[i - 1].x);
                st.AddVertex(pointThree); // Add the next point in the polygon as the third vertex for the triangle fan
            }
        }
        st.GenerateNormals();

        return st.Commit();
    }


    ArrayMesh GenerateHexTrianglesFog(List<Hex> hexList, Layout layout, float height, Godot.Color color)
    {
        SurfaceTool st = new SurfaceTool();
        st.Begin(Mesh.PrimitiveType.Triangles);


        foreach (Hex hex in hexList)
        {
            st.SetColor(color);

            List<Point> points = layout.PolygonCorners(hex);

            Vector3 origin = new Vector3((float)points[0].y, height, (float)points[0].x);
            for (int i = 1; i < 6; i++)
            {
                st.AddVertex(origin); // Add the origin point as the first vertex for the triangle fan

                Vector3 pointTwo = new Vector3((float)points[i].y, height, (float)points[i].x); // Get the next point in the polygon
                st.AddVertex(pointTwo); // Add the next point in the polygon as the second vertex for the triangle fan

                Vector3 pointThree = new Vector3((float)points[i - 1].y, height, (float)points[i - 1].x);
                st.AddVertex(pointThree); // Add the next point in the polygon as the third vertex for the triangle fan
            }
        }
        st.GenerateNormals();

        return st.Commit();
    }

    public override void Unselected()
    {
        GD.PushWarning("NOT IMPLEMENTED");
    }

    public override void Selected()
    {
        GD.PushWarning("NOT IMPLEMENTED");
    }

    public override void ProcessRightClick(Hex hex)
    {
        GD.PushWarning("NOT IMPLEMENTED");
    }

    public override void _Process(double delta)
    {

    }
    public override void RemoveTargetingPrompt()
    {
        GD.PushWarning("NOT IMPLEMENTED");
    }
}
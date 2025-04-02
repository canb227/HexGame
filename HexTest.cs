using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public partial class HexTest : Node3D
{
    Game game;

    List<Hex> hexSubset;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
        Dictionary<int, Player> playerDictionary = new();
        TeamManager teamManager = new TeamManager();
        TurnManager turnManager = new TurnManager();

        //game = new Game("hex/sample");



        game = GameTests.MapLoadTest();
       
        hexSubset = game.mainGameBoard.gameHexDict.Keys.ToList();
        Layout pointy = new Layout(Layout.pointy, new Point(-10, 10), new Point(0, 0));
        Global.layout = pointy;
        Global.game = game;


        MeshInstance3D triangles = new MeshInstance3D();
        triangles.Mesh = GenerateHexTriangles(hexSubset, pointy);



        StandardMaterial3D material = new StandardMaterial3D();
        material.VertexColorUseAsAlbedo = true;
        triangles.SetSurfaceOverrideMaterial(0, material);
        AddChild(triangles);

        MeshInstance3D lines = new MeshInstance3D();
        lines.Mesh = GenerateHexLines(hexSubset, pointy);
        AddChild(lines);

        AddHexTemperature(game.mainGameBoard, pointy);
        AddHexFeatures(game.mainGameBoard,pointy);
        AddHexUnits(game.mainGameBoard, pointy);
        AddHexType(game.mainGameBoard, pointy);
        AddHexDistrictsAndCities(game.mainGameBoard, pointy);
        AddHexCoords(game.mainGameBoard, pointy);
        AddHexYields(game.mainGameBoard, pointy);
    }

    private void AddHexYields(GameBoard mainGameBoard, Layout layout)
    {
        foreach (Hex hex in hexSubset)
        {
            Point point = layout.HexToPixel(hex);
            Label3D lbl = new Label3D();
            lbl.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
            lbl.FontSize = 100;
            lbl.Position = new Vector3((float)point.y, .5f, (float)point.x -3);
            lbl.Text = hex.q.ToString() + "," + hex.r.ToString() + "," + hex.s.ToString();
            AddChild(lbl);
        }
    }

    public override void _Process(double delta)
    {
        
    }



    private void AddHexCoords(GameBoard mainGameBoard, Layout layout)
    {
        foreach (Hex hex in hexSubset)
        {
            Point point = layout.HexToPixel(hex);
            Label3D lbl = new Label3D();
            lbl.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
            lbl.FontSize = 100;
            lbl.Position = new Vector3((float)point.y, .5f, (float)point.x + 3);

            Yields yields = game.mainGameBoard.gameHexDict[hex].yields;

            lbl.Text = "Yields: " + yields.food + "F," + yields.production + "P," + yields.gold + "G," + yields.science + "S," + yields.culture + "C," + yields.happiness + "H";
            //lbl.Text = "Yields: 1F, 1P, 1G, 1S, 1C, 1F";
            AddChild(lbl);
        }
    }

    private void AddHexType(GameBoard mainGameBoard, Layout layout)
    {
        foreach (Hex hex in hexSubset)
        {
            Point point = layout.HexToPixel(hex);
            TerrainTemperature temp = game.mainGameBoard.gameHexDict[hex].terrainTemp;
            Label3D lbl = new Label3D();
            lbl.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
            lbl.FontSize = 100;
            lbl.Position = new Vector3((float)point.y + 2, 1f, (float)point.x - 1);
            switch (game.mainGameBoard.gameHexDict[hex].terrainType)
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

    private void AddHexDistrictsAndCities(GameBoard board, Layout layout)
    {
        foreach (Hex hex in hexSubset)
        {
            Point point = layout.HexToPixel(hex);
            Label3D districtLabel = new Label3D();
            districtLabel.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
            districtLabel.FontSize = 100;
            districtLabel.Position = new Vector3((float)point.y, 1f, (float)point.x);
            if (game.mainGameBoard.gameHexDict[hex].district != null)
            {
                GD.Print("District found");
                if (game.mainGameBoard.gameHexDict[hex].district.isCityCenter)
                {
                    districtLabel.Text = "City: " + game.mainGameBoard.gameHexDict[hex].district.city.name;
                }
                else
                {
                    districtLabel.Text = "District: " + game.mainGameBoard.gameHexDict[hex].district.city.name;
                }
                AddChild(districtLabel);
            }
        }
    }

    private void AddHexUnits(GameBoard board, Layout layout)
    {
        foreach (Hex hex in hexSubset)
        {
            Point point = layout.HexToPixel(hex);
            Label3D lbl = new Label3D();
            lbl.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
            lbl.FontSize = 100;
            lbl.Position = new Vector3((float)point.y - 2, 1f, (float)point.x + 1);
            foreach (Unit unit in game.mainGameBoard.gameHexDict[hex].unitsList)
            {
                lbl.Text += unit.name + " ";
            }
            AddChild(lbl);
        }


    }

    private void AddHexFeatures(GameBoard board,Layout layout)
    {
        foreach (Hex hex in hexSubset)
        {
            Point point = layout.HexToPixel(hex);
            TerrainTemperature temp = game.mainGameBoard.gameHexDict[hex].terrainTemp;
            int xoffset = 1;
            foreach (FeatureType feature in game.mainGameBoard.gameHexDict[hex].featureSet)
            {
                GD.Print("ding:");
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

    private void AddHexTemperature(GameBoard board,Layout layout)
    {

        foreach (Hex hex in hexSubset)
        {
            Point point = layout.HexToPixel(hex);
            TerrainTemperature temp = game.mainGameBoard.gameHexDict[hex].terrainTemp;
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




    ArrayMesh GenerateHexLines(List<Hex> hexes, Layout layout)
    {
      
        SurfaceTool st = new SurfaceTool();
        
        st.Begin(Mesh.PrimitiveType.Lines);

        foreach (Hex hex in hexSubset)
        {
            List<Point> points = layout.PolygonCorners(hex);
            st.AddVertex(new Vector3((float)points[0].y, 0.01f, (float)points[0].x));
            foreach (Point point in points)
            {
                Vector3 temp = new Vector3((float)point.y, 0.01f, (float)point.x);
                //GD.Print(temp);
                st.AddVertex(temp);
                st.AddVertex(temp);

            }
            st.AddVertex(new Vector3((float)points[0].y, 0.01f, (float)points[0].x));
        }
        //st.GenerateNormals();
        return st.Commit();
    }

    ArrayMesh GenerateHexTriangles(List<Hex> hexes, Layout layout)
    {
        SurfaceTool st = new SurfaceTool();
        st.Begin(Mesh.PrimitiveType.Triangles);


        foreach (Hex hex in hexSubset)
        {
            if (game == null)
            {
                GD.Print("Game is null");
            }
            if (game.mainGameBoard == null)
            {
                GD.Print("Game board is null");
            }
            if (game.mainGameBoard.gameHexDict == null)
            {
                GD.Print("Game hex dict is null");
            }   
            if (game.mainGameBoard.gameHexDict[hex] == null)
            {
                GD.Print("Hex is null");
            }

            switch (game.mainGameBoard.gameHexDict[hex].terrainType)
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

            Vector3 origin = new Vector3((float)points[0].y, 0, (float)points[0].x);
            for (int i = 1; i < 6; i++)
            {
                st.AddVertex(origin); // Add the origin point as the first vertex for the triangle fan
                
                Vector3 pointTwo = new Vector3((float)points[i ].y, 0, (float)points[i ].x); // Get the next point in the polygon
                st.AddVertex(pointTwo); // Add the next point in the polygon as the second vertex for the triangle fan

                Vector3 pointThree = new Vector3((float)points[i -1].y, 0, (float)points[i -1].x);
                st.AddVertex(pointThree); // Add the next point in the polygon as the third vertex for the triangle fan
            }

            /*
            foreach (Point point in points)
            {
                
                //GD.Print(temp);
                st.AddVertex(temp);
            }

            Vector3 one = new Vector3((float)points[0].y, 0, (float)points[0].x);
            Vector3 two = new Vector3((float)points[2].y, 0, (float)points[2].x);
            Vector3 three = new Vector3((float)points[3].y, 0, (float)points[3].x);
            
            st.AddVertex(one);
            st.AddVertex(two);
            st.AddVertex(three);

            one = new Vector3((float)points[0].y, 0, (float)points[0].x);
            two = new Vector3((float)points[3].y, 0, (float)points[3].x);
            three = new Vector3((float)points[5].y, 0, (float)points[5].x);
            st.AddVertex(one);
            st.AddVertex(two);
            st.AddVertex(three);
            */
        }
        st.GenerateNormals();
       
        return st.Commit();
        
    }
}

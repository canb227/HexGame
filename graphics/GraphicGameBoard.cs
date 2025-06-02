using Godot;
using NetworkMessages;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using static System.Net.Mime.MediaTypeNames;

public partial class GraphicGameBoard : GraphicObject
{
    public GameBoard gameBoard;
    Layout layout;
    bool firstRun = true;
    public Godot.Image heightImage;
    private Mesh hexMesh;
    private Godot.Image visibilityImage;
    private ImageTexture visibilityTexture = new ImageTexture();
    public GraphicGameBoard(GameBoard gameBoard, Layout layout)
    {
        this.gameBoard = gameBoard;
        this.layout = layout;
        DrawBoard(layout);
        Node3D temp = Godot.ResourceLoader.Load<PackedScene>("res://graphics/models/hexagon.glb").Instantiate<Node3D>();
        MeshInstance3D tempMesh = (MeshInstance3D)temp.GetChild(0);
        hexMesh = tempMesh.Mesh;
        Name = "GraphicGameBoard";

    }
    public override void _Ready()
    {
        List<Hex> all = gameBoard.gameHexDict.Keys.ToList();
        visibilityImage = Godot.Image.CreateEmpty(gameBoard.right, gameBoard.bottom, false, Godot.Image.Format.Rg8);
        AddChild(GenerateHexMultiMesh(all, layout, -1f));
    }

    public override void UpdateGraphic(GraphicUpdateType graphicUpdateType)
    {
        //placeholder
        SimpleRedrawBoard(layout);
    }

    //super simple just delete all our children and redraw the board using the current state of gameboard
    private void SimpleRedrawBoard(Layout pointy)
    {
        /*        foreach (Node child in this.GetChildren())
                {
                    if(child.Name == "GameBoardTerrain" || child.Name == "GameBoardTerrainLines" || child.Name == "GameBoardTerrainFog2" || child.Name == "GameBoardTerrainFog")
                    {
                        child.Free();
                    }
                }*/
        List<Hex> seen = Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].seenGameHexDict.Keys.ToList();
        List<Hex> visible = Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].visibleGameHexDict.Keys.ToList();
        GenerateVisibilityGrid(visible, seen);
        visibilityTexture.Update(visibilityImage);

        //AddBoard(gameBoard.gameHexDict.Keys.ToList(), pointy, 0);
        //AddBoardFog(seenButNotVisible, nonSeenHexes, pointy, 0.5f);
        Global.gameManager.graphicManager.UpdateVisibility();
    }

    private void DrawBoard(Layout pointy)
    {
/*        List<Hex> seen = Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].seenGameHexDict.Keys.ToList();
        List<Hex> visible = Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].visibleGameHexDict.Keys.ToList();
        List<Hex> all = gameBoard.gameHexDict.Keys.ToList();

        HashSet<Hex> seenHexSet = new HashSet<Hex>(seen);
        List<Hex> nonSeenHexes = all.Where(hex => !seenHexSet.Contains(hex)).ToList();

        List<Hex> seenButNotVisible = seen.Except(visible).ToList();*/

        //AddBoard(all, pointy, 0);
        //AddBoardFog(seenButNotVisible, nonSeenHexes, pointy, 0.5f);
        //Add3DBoard(all, pointy, 100, 100, 1);



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
        triangles.Mesh = GenerateHexTriangles(hexList, pointy, 10);
        triangles.Transparency = 0.5f;
        StandardMaterial3D material = new StandardMaterial3D();
        material.VertexColorUseAsAlbedo = true;
        triangles.SetSurfaceOverrideMaterial(0, material);
        triangles.Name = "GameBoardTerrain";
        AddChild(triangles);

        MeshInstance3D lines = new MeshInstance3D();
        lines.Mesh = GenerateHexLines(hexList, pointy, 10.01f);
        lines.Transparency = 0.5f;
        lines.Name = "GameBoardTerrainLines";
        AddChild(lines);
    }

    Rid mesh;
    Rid instance;
    Rid materialShader;
    Rid terrainMaterial;
    ImageTexture heightMapTexture;
    Shader terrainShader = GD.Load<Shader>("res://graphics/shaders/terrain/terrainChunk.gdshader");
    CompressedTexture2D rock = GD.Load<CompressedTexture2D>("res://.godot/imported/rock_alb.png-4e918266e00415e40ecb0ea000c74b30.ctex");
    CompressedTexture2D grass = GD.Load<CompressedTexture2D>("res://.godot/imported/ground_alb.png-b0923ef3d19a3922700780568807ede9.ctex");
    CompressedTexture2D rockNormal = GD.Load<CompressedTexture2D>("res://.godot/imported/rock_nrm.png-bc2a7cb2d1cfae3a12302531bce73b06.ctex");
    CompressedTexture2D grassNormal = GD.Load<CompressedTexture2D>("res://.godot/imported/ground_nrm.png-9c1f59f3a389c9cb54cbe0438e4bfd93.ctex");

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
                    Global.gameManager.graphicManager.NewCity(Global.gameManager.game.cityDictionary[gameBoard.gameHexDict[hex].district.cityID]);
                    Global.gameManager.graphicManager.NewDistrict(gameBoard.gameHexDict[hex].district);
                }
                else
                {
                    Global.gameManager.graphicManager.NewDistrict(gameBoard.gameHexDict[hex].district);
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
                Global.gameManager.graphicManager.NewUnit(unit);
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
            st.AddVertex(new Vector3((float)points[0].y, height, (float)points[0].x));
            foreach (Point point in points)
            {
                Vector3 temp = new Vector3((float)point.y, height, (float)point.x);
                st.AddVertex(temp);
                st.AddVertex(temp);

            }
            st.AddVertex(new Vector3((float)points[0].y, height, (float)points[0].x));
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

    public MultiMeshInstance3D GenerateHexMultiMesh(List<Hex> hexList, Layout layout, float height)
    {
        MultiMeshInstance3D multiMeshInstance = new MultiMeshInstance3D();
        MultiMesh multiMesh = new MultiMesh();

        multiMesh.Mesh = hexMesh;  // Use shared hex mesh
        multiMesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
        multiMesh.UseCustomData = true;
        multiMesh.InstanceCount = hexList.Count;  // Set number of instances


/*        Transform3D multiTransform = multiMeshInstance.Transform;
        multiTransform.Rotated(Vector3.Up, Mathf.DegToRad(90));
        multiMeshInstance.Transform = multiTransform;*/

        multiMeshInstance.Multimesh = multiMesh;

        Layout rotatedLayout = new Layout(Layout.pointy, new Point(10, 10), new Point(0, 0));
        // Assign positions
        for (int i = 0; i < hexList.Count; i++)
        {
            Hex hex = hexList[i];
            Point worldPos = layout.HexToPixel(hex);
            Transform3D transform = new Transform3D(Basis.Identity, new Vector3((float)worldPos.y, height, (float)worldPos.x));
            multiMesh.SetInstanceTransform(i, transform);

            Godot.Color hexData = new Godot.Color(hex.q / 255f, hex.r / 255f, 0, 1);
            multiMeshInstance.Multimesh.SetInstanceCustomData(i, hexData);
        }

        List<Hex> seen = Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].seenGameHexDict.Keys.ToList();
        List<Hex> visible = Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].visibleGameHexDict.Keys.ToList();


        CompressedTexture2D grassTexture = GD.Load<CompressedTexture2D>("res://graphics/textures/ground_alb.png");

        Shader shader = GD.Load<Shader>("res://graphics/shaders/terrain/hex.gdshader");
        ShaderMaterial shaderMaterial = new ShaderMaterial();
        shaderMaterial.Shader = shader;

        GenerateVisibilityGrid(visible, seen);
        visibilityTexture = ImageTexture.CreateFromImage(visibilityImage);
        shaderMaterial.SetShaderParameter("visibilityGrid", visibilityTexture);
        shaderMaterial.SetShaderParameter("gameBoardWidth", gameBoard.right - gameBoard.left);
        shaderMaterial.SetShaderParameter("gameBoardHeight", gameBoard.bottom - gameBoard.top);
        shaderMaterial.SetShaderParameter("grassTexture", grassTexture);

        multiMeshInstance.MaterialOverride = shaderMaterial;

        return multiMeshInstance;
    }

    public void GenerateVisibilityGrid(List<Hex> visibleHexes, List<Hex> seenHexes)
    {
        visibilityImage.Fill(new Godot.Color(0, 0, 0, 1)); // Default to hidden, unseen

        foreach (Hex hex in seenHexes)
        {
            visibilityImage.SetPixel(hex.q, hex.r, new Godot.Color(0, 1, 0, 1)); // Mark as seen
        }

        foreach (Hex hex in visibleHexes)
        {
            visibilityImage.SetPixel(hex.q, hex.r, new Godot.Color(1, 0, 0, 1)); // Set visible
        }
        visibilityImage.SavePng("testVis.png");
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

    public override void RemoveTargetingPrompt()
    {
        GD.PushWarning("NOT IMPLEMENTED");
    }
}
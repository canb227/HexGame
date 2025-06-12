using Godot;
using NetworkMessages;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml;
using static Google.Protobuf.Reflection.FeatureSet.Types;

public partial class GraphicGameBoard : GraphicObject
{
    public GameBoard gameBoard;
    Layout layout;
    public Mesh hexMesh;
    Shader terrainShader = GD.Load<Shader>("res://graphics/shaders/terrain/hex.gdshader");
    ImageTexture heightMapTexture;
    public Dictionary<Hex, int> hexToChunkDictionary = new();
    public List<HexChunk> chunkList = new();
    public int chunkSize = 0;
    public int chunkCount = 0;
    private Image visibilityImage;
    private ImageTexture visibilityTexture;
    private Image terrainInfoImage;
    public GraphicGameBoard(GameBoard gameBoard, Layout layout)
    {
        this.gameBoard = gameBoard;
        this.layout = layout;
        Node3D temp = Godot.ResourceLoader.Load<PackedScene>("res://graphics/models/hexagon.glb").Instantiate<Node3D>();
        MeshInstance3D tempMesh = (MeshInstance3D)temp.GetChild(0);
        hexMesh = tempMesh.Mesh;


        DrawBoard(layout);
    }
    public override void _Ready()
    {
        AddHexResource();
        AddHexFeatures(layout);
        AddHexUnits(layout);
        AddHexDistrictsAndCities(layout);
        SimpleRedrawBoard(layout);
    }

    public Hex HexToGraphicHex(Hex hex)
    {
        int chunkID = hexToChunkDictionary[hex];
        //GD.Print("CHUNKID: " + chunkID);
        return chunkList[chunkID].HexToGraphicalHex(hex);
    }

    public override void UpdateGraphic(GraphicUpdateType graphicUpdateType)
    {
        //placeholder
        SimpleRedrawBoard(layout);
    }

    //super simple just delete all our children and redraw the board using the current state of gameboard
    private void SimpleRedrawBoard(Layout pointy)
    {
        List<Hex> seen = Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].seenGameHexDict.Keys.ToList();
        List<Hex> visible = Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].visibleGameHexDict.Keys.ToList();
        UpdateVisibilityTexture(visible, seen);
        Global.gameManager.graphicManager.UpdateVisibility();

        //AddBoard(Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].seenGameHexDict.Keys.ToList(), 1, pointy);
        //AddBoardFog(seenButNotVisible, nonSeenHexes, pointy, 0.5f);


    }



    private void DrawBoard(Layout pointy)
    {
        List<Hex> seen = Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].seenGameHexDict.Keys.ToList();
        List<Hex> visible = Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].visibleGameHexDict.Keys.ToList();
        List<Hex> all = gameBoard.gameHexDict.Keys.ToList();

        HashSet<Hex> seenHexSet = new HashSet<Hex>(seen);
        List<Hex> nonSeenHexes = all.Where(hex => !seenHexSet.Contains(hex)).ToList();

        List<Hex> seenButNotVisible = seen.Except(visible).ToList();

        chunkCount = 4;
        AddBoard(all, chunkCount, pointy);
        //AddBoardFog(seenButNotVisible, nonSeenHexes, pointy, 0.5f);
    }

/*    public void UpdateHexChunkOrigins()
    {
        foreach(HexChunk hexChunk in chunkList)
        {
            Global.camera.GetCameraHexQ();
            //hexChunk.UpdateGraphicalOrigin();
        }
    }*/

    private void AddBoard(List<Hex> hexList, int chunkCount, Layout pointy)
    {
        int boardWidth = Global.gameManager.game.mainGameBoard.right - Global.gameManager.game.mainGameBoard.left;
        chunkSize = (int) Math.Ceiling((float)boardWidth/chunkCount);
        List<List<Hex>> hexListChunks = new List<List<Hex>>();
        for (int chunkIndex = 0; chunkIndex < chunkCount; chunkIndex++)
        {
            List<Hex> subHexList = new List<Hex>();
            //GD.Print("chunk offset: " + (chunkSize * chunkIndex));
            foreach (Hex hex in hexList)
            {
                int left = ( Global.gameManager.game.mainGameBoard.left + (chunkSize * chunkIndex) ) - (hex.r >> 1);
                int right = (left + chunkSize);
                if (hex.q >= left)
                {
                    if (hex.q < right)
                    {
                        subHexList.Add(hex);
                    }
                }
            }
            hexListChunks.Add(subHexList);
        }
        visibilityImage = Godot.Image.CreateEmpty(Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.bottom, false, Godot.Image.Format.Rg8);
        visibilityTexture = ImageTexture.CreateFromImage(visibilityImage);
        List<Hex> seen = Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].seenGameHexDict.Keys.ToList();
        List<Hex> visible = Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].visibleGameHexDict.Keys.ToList();
        UpdateVisibilityTexture(visible, seen);

        int i = 0;
        foreach(List<Hex> subHexList in hexListChunks)
        {
            MultiMeshInstance3D multiMeshInstance = new MultiMeshInstance3D();
            MultiMesh multiMesh = new MultiMesh();
            //MeshInstance3D triangles = new MeshInstance3D();

            multiMesh.Mesh = hexMesh;
            multiMesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
            multiMesh.UseCustomData = true;
            multiMesh.InstanceCount = subHexList.Count;

            multiMeshInstance.Multimesh = multiMesh;

            for (int j = 0; j < hexListChunks[0].Count; j++)
            {
                Hex hex = hexListChunks[0][j];
                Point worldPos = layout.HexToPixel(hex);
                Transform3D transform = new Transform3D(Basis.Identity, new Vector3((float)worldPos.y, -1.0f, (float)worldPos.x));
                multiMesh.SetInstanceTransform(j, transform);

                Hex realHex = subHexList[j];
                Godot.Color hexData = new Godot.Color(realHex.q / 255f, realHex.r / 255f, 0, 1);
                multiMeshInstance.Multimesh.SetInstanceCustomData(j, hexData);
            }
            foreach (Hex hex in subHexList)
            {
                hexToChunkDictionary.Add(hex, i);
            }
            //triangles.Mesh = GenerateHexTriangles(subHexList, hexListChunks[0], i, pointy);

            ShaderMaterial terrainShaderMaterial = new ShaderMaterial();
            terrainShaderMaterial.Shader = terrainShader;

            float hSpace = (float)Math.Sqrt(3) * 10.0f;
            float vSpace = 10.0f;
            float chunkOffset = (float)(i * chunkSize * hSpace);


            FastNoiseLite noise = new FastNoiseLite();
            noise.Frequency = 0.05f;
            int noiseImageSizeX = (int)((chunkSize+1) * hSpace);
            int noiseImageSizeY = (int)(Global.gameManager.game.mainGameBoard.bottom * 1.5f * 10);

            noise.Offset = new Vector3((float)(hSpace/2.0f - chunkOffset), 0.0f, 0.0f);
            Image heightMap = Godot.Image.CreateEmpty(noiseImageSizeX, noiseImageSizeY, false, Image.Format.Rgba8);
            Image noiseMap = noise.GetImage(noiseImageSizeX, noiseImageSizeY);

            for (int x = 0; x < noiseImageSizeX; x++)
            {
                for(int y = 0; y < noiseImageSizeY; y++)
                {
                    Godot.Color pix = heightMap.GetPixel(x, y);
                    //pix = new Godot.Color(pix.R / 2.0f + 0.5f, 0.0f, 0.0f);
                    FractionalHex fHex = Global.layout.PixelToHex(new Point(-x + hSpace/2.0f - chunkOffset, (y - vSpace/2.0f)));
                    Hex hex = fHex.HexRound(); 
                    Hex wrapHex = hex.WrapHex();
                    //heightMap.SetPixel(x, y, new Godot.Color((float)x/noiseImageSize, 0.0f, 0.0f));
                    //GD.Print(wrapHex);
                    
                    if (Global.gameManager.game.mainGameBoard.gameHexDict.TryGetValue(wrapHex, out GameHex gameHex))
                    {
                        //GD.Print(x + ", " + y + " " + wrapHex);
                        if (gameHex.terrainType == TerrainType.Mountain)
                        {
                            heightMap.SetPixel(x, y, new Godot.Color(1.0f, 0.0f, 0.0f));
                        }
                        else if (gameHex.terrainType == TerrainType.Rough)
                        {
                            heightMap.SetPixel(x, y, new Godot.Color(0.4f, 0.0f, 0.0f));

                        }
                        else if (gameHex.terrainType == TerrainType.Flat)
                        {
                            heightMap.SetPixel(x, y, new Godot.Color(0.1f, 0.0f, 0.0f));
                        }
                        else
                        {
                            heightMap.SetPixel(x, y, new Godot.Color(0.0f, 0.0f, 0.0f));
                        }
                    }
                    else
                    {
                        heightMap.SetPixel(x, y, new Godot.Color(0.0f, 0.0f, 0.0f));
                    }
                }
            }
            //heightMap.SavePng("heightMap" + i + ".png");
            GaussianBlur(heightMap, 3);
            //heightMap.SavePng("heightMapBlurred" + i + ".png");

            //apply noise
            for (int x = 0; x < noiseImageSizeX; x++)
            {
                for (int y = 0; y < noiseImageSizeY; y++)
                {
                    heightMap.SetPixel(x, y, new Godot.Color(noiseMap.GetPixel(x,y).R * heightMap.GetPixel(x,y).R, 0.0f, 0.0f));    
                }
            }

            heightMapTexture = ImageTexture.CreateFromImage(heightMap);


            terrainInfoImage = GenerateTerrainInfoImage();
            ImageTexture terrainInfoTexture = ImageTexture.CreateFromImage(terrainInfoImage);

            terrainShaderMaterial.SetShaderParameter("heightMap", heightMapTexture);
            terrainShaderMaterial.SetShaderParameter("visibilityGrid", visibilityTexture);
            terrainShaderMaterial.SetShaderParameter("terrainInfo", terrainInfoTexture);

            terrainShaderMaterial.SetShaderParameter("gameBoardWidth", Global.gameManager.game.mainGameBoard.right);
            terrainShaderMaterial.SetShaderParameter("gameBoardHeight", Global.gameManager.game.mainGameBoard.bottom);
            terrainShaderMaterial.SetShaderParameter("chunkOffset", chunkOffset);
            terrainShaderMaterial.SetShaderParameter("widthDiv", Math.Sqrt(3) * 10.0f * (chunkSize+1));
            terrainShaderMaterial.SetShaderParameter("heightDiv", 1.5 * 10.0 * Global.gameManager.game.mainGameBoard.bottom);


            multiMeshInstance.MaterialOverride = terrainShaderMaterial;
            multiMeshInstance.Name = "GameBoardTerrain" + i;

            chunkList.Add(new HexChunk(multiMeshInstance, subHexList, subHexList.First(), subHexList.First(), heightMap, terrainShaderMaterial, chunkOffset, (float)Math.Sqrt(3) * 10.0f * (chunkSize + 1), 1.5f * 10.0f * Global.gameManager.game.mainGameBoard.bottom));//we set graphical to our default location here then update as we move it around

            AddChild(multiMeshInstance);
            i++;
        }


/*        MeshInstance3D lines = new MeshInstance3D();
        lines.Mesh = GenerateHexLines(hexList, pointy, 0.01f);
        lines.Name = "GameBoardTerrainLines";
        AddChild(lines);*/
    }

    public void UpdateVisibilityTexture(List<Hex> visibleHexes, List<Hex> seenHexes)
    {
        visibilityImage.Fill(new Godot.Color(0, 0, 0, 1)); // Default to hidden, unseen

        foreach (Hex hex in seenHexes)
        {
            Hex wrapHex = hex.WrapHex();
            int newQ = wrapHex.q + (wrapHex.r >> 1);
            visibilityImage.SetPixel(newQ, wrapHex.r, new Godot.Color(0, 1, 0, 1)); // Mark as seen
        }

        foreach (Hex hex in visibleHexes)
        {
            Hex wrapHex = hex.WrapHex();
            int newQ = wrapHex.q + (wrapHex.r >> 1);
            visibilityImage.SetPixel(newQ, wrapHex.r, new Godot.Color(1, 0, 0, 1)); // Set visible
        }
        visibilityTexture.Update(visibilityImage);
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

    private void AddHexResource()
    {
        foreach (Hex hex in gameBoard.gameHexDict.Keys)
        {
            if(Global.gameManager.game.mainGameBoard.gameHexDict[hex].resourceType != ResourceType.None)
            {
                Global.gameManager.graphicManager.NewResource(Global.gameManager.game.mainGameBoard.gameHexDict[hex].resourceType, hex);
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

    ArrayMesh GenerateHexTriangles(List<Hex> hexList, List<Hex> graphicHexList, int chunkIndex, Layout layout)
    {
        SurfaceTool st = new SurfaceTool();
        st.Begin(Mesh.PrimitiveType.Triangles);
        foreach (Hex hex in hexList)
        {
            hexToChunkDictionary.Add(hex, chunkIndex);
        }

        foreach (Hex hex in graphicHexList)
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
            Transform3D newTransform = Transform;
            Hex wrapHex = hex.WrapHex();
            newTransform.Origin = new Vector3((float)layout.HexToPixel(wrapHex).y, -1.0f, (float)layout.HexToPixel(wrapHex).x);
            st.AppendFrom(hexMesh, 0, newTransform);
        }
        st.GenerateNormals();
        //st.Index();

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

    public void GaussianBlur(Image image, int radius)
    {
        int width = image.GetWidth();
        int height = image.GetHeight();
        Image tempImage = image;

        float[] kernel = GenerateGaussianKernel(radius);

        // Horizontal pass
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Godot.Color newColor = ApplyKernel(tempImage, x, y, kernel, radius, true);
                image.SetPixel(x, y, newColor);
            }
        }

        // Vertical pass
        tempImage.CopyFrom(image);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Godot.Color newColor = ApplyKernel(tempImage, x, y, kernel, radius, false);
                image.SetPixel(x, y, newColor);
            }
        }
    }

    private float[] GenerateGaussianKernel(int radius)
    {
        int size = radius * 2 + 1;
        float[] kernel = new float[size];
        float sigma = radius / 2.0f;
        float sum = 0f;

        for (int i = 0; i < size; i++)
        {
            float x = i - radius;
            kernel[i] = Mathf.Exp(-0.5f * (x * x) / (sigma * sigma));
            sum += kernel[i];
        }

        // Normalize kernel
        for (int i = 0; i < size; i++)
        {
            kernel[i] /= sum;
        }

        return kernel;
    }

    private Godot.Color ApplyKernel(Image image, int x, int y, float[] kernel, int radius, bool horizontal)
    {
        Godot.Color newColor = new Godot.Color(0, 0, 0, 0);
        int size = kernel.Length;

        for (int i = 0; i < size; i++)
        {
            int sampleX = horizontal ? Mathf.Clamp(x + i - radius, 0, image.GetWidth() - 1) : x;
            int sampleY = horizontal ? y : Mathf.Clamp(y + i - radius, 0, image.GetHeight() - 1);
            newColor += image.GetPixel(sampleX, sampleY) * kernel[i];
        }

        return newColor;
    }
    
    private Image GenerateTerrainInfoImage()
    {
        terrainInfoImage = Godot.Image.CreateEmpty(Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.bottom, false, Godot.Image.Format.Rg8);

        foreach (Hex hex in Global.gameManager.game.mainGameBoard.gameHexDict.Keys)
        {
            Hex wrapHex = hex.WrapHex();
            int newQ = wrapHex.q + (wrapHex.r >> 1);
            if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].terrainTemp == TerrainTemperature.Desert)
            {
                terrainInfoImage.SetPixel(newQ, wrapHex.r, new Godot.Color(0.1f, 0, 0, 1)); // Mark as seen
            }
            else if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].terrainTemp == TerrainTemperature.Plains)
            {
                terrainInfoImage.SetPixel(newQ, wrapHex.r, new Godot.Color(0.2f, 0, 0, 1)); // Mark as seen
            }
            else if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].terrainTemp == TerrainTemperature.Grassland)
            {
                terrainInfoImage.SetPixel(newQ, wrapHex.r, new Godot.Color(0.3f, 0, 0, 1)); // Mark as seen
            }
            else if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].terrainTemp == TerrainTemperature.Tundra)
            {
                terrainInfoImage.SetPixel(newQ, wrapHex.r, new Godot.Color(0.4f, 0, 0, 1)); // Mark as seen
            }
            else if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].terrainTemp == TerrainTemperature.Arctic)
            {
                terrainInfoImage.SetPixel(newQ, wrapHex.r, new Godot.Color(0.5f, 0, 0, 1)); // Mark as seen
            }

            if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].terrainType == TerrainType.Flat)
            {
                terrainInfoImage.SetPixel(newQ, wrapHex.r, new Godot.Color(terrainInfoImage.GetPixel(newQ, wrapHex.r).R, 0.1f, 0, 1)); // Mark as seen
            }
            else if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].terrainType == TerrainType.Rough)
            {
                terrainInfoImage.SetPixel(newQ, wrapHex.r, new Godot.Color(terrainInfoImage.GetPixel(newQ, wrapHex.r).R, 0.2f, 0, 1)); // Mark as seen
            }
            else if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].terrainType == TerrainType.Mountain)
            {
                terrainInfoImage.SetPixel(newQ, wrapHex.r, new Godot.Color(terrainInfoImage.GetPixel(newQ, wrapHex.r).R, 0.3f, 0, 1)); // Mark as seen
            }
            else if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].terrainType == TerrainType.Coast)
            {
                terrainInfoImage.SetPixel(newQ, wrapHex.r, new Godot.Color(terrainInfoImage.GetPixel(newQ, wrapHex.r).R, 0.4f, 0, 1)); // Mark as seen
            }
            else if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].terrainType == TerrainType.Ocean)
            {
                terrainInfoImage.SetPixel(newQ, wrapHex.r, new Godot.Color(terrainInfoImage.GetPixel(newQ, wrapHex.r).R, 0.5f, 0, 1)); // Mark as seen
            }
        }

        terrainInfoImage.SavePng("testInfo.png");
        return terrainInfoImage;
    }
}
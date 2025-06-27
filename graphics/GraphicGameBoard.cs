using Godot;
using NetworkMessages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime;
using System.Text.RegularExpressions;
using System.Xml;
using static Google.Protobuf.Reflection.FeatureSet.Types;

public partial class GraphicGameBoard : GraphicObject
{
    public GameBoard gameBoard;
    Layout layout;
    public Mesh hexMesh;
    public Mesh yieldMesh;
    Shader terrainShader = GD.Load<Shader>("res://graphics/shaders/terrain/hex.gdshader");
    Shader yieldShader = GD.Load<Shader>("res://graphics/shaders/terrain/yields.gdshader");
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

        temp = Godot.ResourceLoader.Load<PackedScene>("res://graphics/models/yields.glb").Instantiate<Node3D>();
        tempMesh = (MeshInstance3D)temp.GetChild(0);
        yieldMesh = tempMesh.Mesh;


        DrawBoard(layout);
    }
    public override void _Ready()
    {
        AddHexResource();

        Add3DHexFeatures();
        //AddHexFeatures(layout);

        //Add3DHexYields();

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

    public void UpdateYield(Hex hex)
    {
        int chunkID = hexToChunkDictionary[hex];
        chunkList[chunkID].UpdateHexYield(hex);
    }

    public override void UpdateGraphic(GraphicUpdateType graphicUpdateType)
    {
        //placeholder
        SimpleRedrawBoard(layout);
    }

    //updates the vis texture and tells objects to update their vis data
    private void SimpleRedrawBoard(Layout pointy)
    {
        List<Hex> seen = Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].seenGameHexDict.Keys.ToList();
        List<Hex> visible = Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].visibleGameHexDict.Keys.ToList();
        UpdateVisibilityTexture(visible, seen);
        Global.gameManager.graphicManager.UpdateVisibility();

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
        Stopwatch sw = new Stopwatch();
        sw.Start();
        CompressedTexture2D grassTex = GD.Load<CompressedTexture2D>("res://graphics/textures/grass.jpg");
        CompressedTexture2D grassNorm = GD.Load<CompressedTexture2D>("res://graphics/textures/grass_norm.png");
        CompressedTexture2D hillTex = GD.Load<CompressedTexture2D>("res://graphics/textures/hills.jpg");
        CompressedTexture2D hillNorm = GD.Load<CompressedTexture2D>("res://graphics/textures/hills_norm.png");
        CompressedTexture2D rockTex = GD.Load<CompressedTexture2D>("res://graphics/textures/mountain.png");
        CompressedTexture2D rockNorm = GD.Load<CompressedTexture2D>("res://graphics/textures/rock_norm.png");
        CompressedTexture2D sandTex = GD.Load<CompressedTexture2D>("res://graphics/textures/sand.jpg");
        CompressedTexture2D sandNorm = GD.Load<CompressedTexture2D>("res://graphics/textures/sand_norm.png");
        CompressedTexture2D snowTex = GD.Load<CompressedTexture2D>("res://graphics/textures/snow.jpg");
        CompressedTexture2D snowNorm = GD.Load<CompressedTexture2D>("res://graphics/textures/snow_norm.png");
        CompressedTexture2D waterTex = GD.Load<CompressedTexture2D>("res://graphics/textures/water.jpg");
        CompressedTexture2D waterNorm = GD.Load<CompressedTexture2D>("res://graphics/textures/water_norm.jpg");


        terrainInfoImage = GenerateTerrainInfoImage();
        ImageTexture terrainInfoTexture = ImageTexture.CreateFromImage(terrainInfoImage);

        ShaderMaterial terrainShaderMaterial = new ShaderMaterial();
        terrainShaderMaterial.Shader = terrainShader;
        terrainShaderMaterial.SetShaderParameter("terrainInfo", terrainInfoTexture);

        terrainShaderMaterial.SetShaderParameter("grassTex", grassTex);
        terrainShaderMaterial.SetShaderParameter("grassNorm", grassNorm);
        terrainShaderMaterial.SetShaderParameter("hillTex", hillTex);
        terrainShaderMaterial.SetShaderParameter("hillNorm", hillNorm);
        terrainShaderMaterial.SetShaderParameter("rockTex", rockTex);
        terrainShaderMaterial.SetShaderParameter("rockNorm", rockNorm);
        terrainShaderMaterial.SetShaderParameter("sandTex", sandTex);
        terrainShaderMaterial.SetShaderParameter("sandNorm", sandNorm);
        terrainShaderMaterial.SetShaderParameter("snowTex", snowTex);
        terrainShaderMaterial.SetShaderParameter("snowNorm", snowNorm);
        terrainShaderMaterial.SetShaderParameter("waterTex", waterTex);
        terrainShaderMaterial.SetShaderParameter("waterNormal", waterNorm);


        int boardWidth = Global.gameManager.game.mainGameBoard.right - Global.gameManager.game.mainGameBoard.left;
        chunkSize = (int)Math.Ceiling((float)boardWidth / chunkCount);

        terrainShaderMaterial.SetShaderParameter("gameBoardWidth", Global.gameManager.game.mainGameBoard.right);
        terrainShaderMaterial.SetShaderParameter("gameBoardHeight", Global.gameManager.game.mainGameBoard.bottom);
        terrainShaderMaterial.SetShaderParameter("widthDiv", Math.Sqrt(3) * 10.0f * (chunkSize));
        terrainShaderMaterial.SetShaderParameter("heightDiv", 1.5 * 10.0 * Global.gameManager.game.mainGameBoard.bottom);

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
        terrainShaderMaterial.SetShaderParameter("visibilityGrid", visibilityTexture);
        Vector3[] yieldOffsets = new Vector3[] {
            new Vector3( 0.0f, 1.0f,  4.0f),
            new Vector3( 0.0f, 1.0f,  0.0f), 
            new Vector3( 0.0f, 1.0f, -4.0f),
            new Vector3( 3.0f, 1.0f,  3.0f),
            new Vector3( 3.0f, 1.0f, -3.0f),
            new Vector3(-3.0f, 1.0f,  3.0f),
            new Vector3(-3.0f, 1.0f, -3.0f),
        };




        ShaderMaterial yieldShaderMaterial = new ShaderMaterial();
        yieldShaderMaterial.Shader = yieldShader;

        float hSpace = (float)Math.Sqrt(3) * 10.0f;
        float vSpace = 10.0f;
        float chunkOffset = 0;


        FastNoiseLite noise = new FastNoiseLite();
        noise.Frequency = 0.05f;
        int noiseImageSizeX = (int)((chunkSize) * chunkCount * hSpace);
        int noiseImageSizeY = (int)(Global.gameManager.game.mainGameBoard.bottom * 1.5f * 10);

        //noise.Offset = new Vector3((float)(hSpace/2.0f - chunkOffset), 0.0f, 0.0f);
        Image heightMap = Godot.Image.CreateEmpty(noiseImageSizeX, noiseImageSizeY, false, Image.Format.Rgba8);
        Image noiseMap = noise.GetSeamlessImage(noiseImageSizeX, noiseImageSizeY);

        for (int x = 0; x < noiseImageSizeX; x++)
        {
            for (int y = 0; y < noiseImageSizeY; y++)
            {
                Godot.Color pix = heightMap.GetPixel(x, y);
                //pix = new Godot.Color(pix.R / 2.0f + 0.5f, 0.0f, 0.0f);
                FractionalHex fHex = Global.layout.PixelToHex(new Point(-x, (y)));
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
                        heightMap.SetPixel(x, y, new Godot.Color(0.2f, 0.0f, 0.0f));

                    }
                    else if (gameHex.terrainType == TerrainType.Flat)
                    {
                        heightMap.SetPixel(x, y, new Godot.Color(0.05f, 0.0f, 0.0f));
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
        heightMap.SavePng("heightMap.png");
        GaussianBlur(heightMap, 7);

        //apply noise
        for (int x = 0; x < noiseImageSizeX; x++)
        {
            for (int y = 0; y < noiseImageSizeY; y++)
            {
                heightMap.SetPixel(x, y, new Godot.Color(noiseMap.GetPixel(x, y).R * heightMap.GetPixel(x, y).R, 0.0f, 0.0f));
            }
        }
        heightMap.SavePng("heightMapBlurredNoised.png");

        heightMapTexture = ImageTexture.CreateFromImage(heightMap);
        terrainShaderMaterial.SetShaderParameter("heightMap", heightMapTexture);
        terrainShaderMaterial.SetShaderParameter("chunkCount", chunkCount);


        CompressedTexture2D yieldAtlasTexture = GD.Load<CompressedTexture2D>("res://graphics/ui/icons/yieldsatlas.png");
        CompressedTexture2D digitAtlasTexture = GD.Load<CompressedTexture2D>("res://graphics/ui/icons/numberatlas.png");

        //ImageTexture yieldAtlasTexture = ImageTexture.CreateFromImage(yieldAtlas);
        yieldShaderMaterial.SetShaderParameter("yieldAtlas", yieldAtlasTexture);
        yieldShaderMaterial.SetShaderParameter("digitAtlas", digitAtlasTexture);
        yieldShaderMaterial.SetShaderParameter("gameBoardWidth", Global.gameManager.game.mainGameBoard.right);
        yieldShaderMaterial.SetShaderParameter("gameBoardHeight", Global.gameManager.game.mainGameBoard.bottom);
        yieldShaderMaterial.SetShaderParameter("visibilityGrid", visibilityTexture);

        int i = 0;
        foreach(List<Hex> subHexList in hexListChunks)
        {
            MultiMeshInstance3D multiMeshInstance = new MultiMeshInstance3D();
            MultiMeshInstance3D yieldMultiMeshInstance = new MultiMeshInstance3D();

            yieldMultiMeshInstance.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;

            MultiMesh multiMesh = new MultiMesh();
            MultiMesh yieldMultiMesh = new MultiMesh();
            //MeshInstance3D triangles = new MeshInstance3D();

            multiMesh.Mesh = hexMesh;
            multiMesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
            multiMesh.UseCustomData = true;
            multiMesh.InstanceCount = subHexList.Count;

            yieldMultiMesh.Mesh = yieldMesh;
            yieldMultiMesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
            yieldMultiMesh.UseCustomData = true;
            yieldMultiMesh.InstanceCount = subHexList.Count*7;

            multiMeshInstance.Multimesh = multiMesh;
            yieldMultiMeshInstance.Multimesh = yieldMultiMesh;

            for (int j = 0; j < hexListChunks[0].Count; j++)
            {
                Hex hex = hexListChunks[0][j];
                Point worldPos = layout.HexToPixel(hex);
                Transform3D transform = new Transform3D(Basis.Identity, new Vector3((float)worldPos.y, -1.0f, (float)worldPos.x));
                multiMesh.SetInstanceTransform(j, transform);


                Hex realHex = subHexList[j];
                Godot.Color hexData = new Godot.Color(realHex.q / 255f, realHex.r / 255f, 0, 1);
                multiMeshInstance.Multimesh.SetInstanceCustomData(j, hexData);

                Dictionary<YieldType, float> yieldDict = Global.gameManager.game.mainGameBoard.gameHexDict[realHex].yields.YieldsToDict();
                for(int l = 0; l < 7; l ++)
                {
                    Vector3 yieldPosition = new Vector3((float)worldPos.y, 2.0f, (float)worldPos.x) + yieldOffsets[l];
                    Transform3D yieldTransform = new Transform3D(Basis.Identity, yieldPosition);
                    yieldMultiMesh.SetInstanceTransform(j*7+l, yieldTransform);
                    yieldMultiMeshInstance.Multimesh.SetInstanceCustomData(j*7+l, new Godot.Color(l/7.0f, yieldDict[(YieldType)l]/100.0f, realHex.q / 255f, realHex.r / 255f));//r is type, g is value, b is hex.q, a is hex.r
                }
            }
            foreach (Hex hex in subHexList)
            {
                hexToChunkDictionary.Add(hex, i);
            }
            //triangles.Mesh = GenerateHexTriangles(subHexList, hexListChunks[0], i, pointy);



            multiMeshInstance.MaterialOverride = terrainShaderMaterial;
            //multiMeshInstance.SetInstanceShaderParameter("chunkOffset", chunkOffset);
            multiMeshInstance.Name = "GameBoardTerrain" + i;

            //yieldShaderMaterial.RenderPriority = 10;
            yieldMultiMeshInstance.MaterialOverride = yieldShaderMaterial;
            yieldMultiMeshInstance.Name = "Yield" + i;

            chunkList.Add(new HexChunk(multiMeshInstance, yieldMultiMeshInstance, subHexList, subHexList.First(), subHexList.First(), heightMap, terrainShaderMaterial, chunkOffset, (float)Math.Sqrt(3) * 10.0f * (chunkSize + 1), 1.5f * 10.0f * Global.gameManager.game.mainGameBoard.bottom));//we set graphical to our default location here then update as we move it around

            AddChild(multiMeshInstance);
            multiMeshInstance.AddChild(yieldMultiMeshInstance);
            i++;
        }
        sw.Stop();
        GD.Print(sw.ElapsedMilliseconds);
    }

    private Godot.Color PackYields(Dictionary<YieldType, float> yields)
    {
        return new Godot.Color(
            (yields[YieldType.food] / 100.0f) + ((yields[YieldType.production] / 100.0f) * 0.01f) + ((yields[YieldType.gold] / 100.0f) * 0.0001f),  // Pack into `r`
            (yields[YieldType.science] / 100.0f) + ((yields[YieldType.culture] / 100.0f) * 0.01f),                               // Pack into `g`
            (yields[YieldType.happiness] / 100.0f) + ((yields[YieldType.influence] / 100.0f) * 0.01f)                            // Pack into `b`
        );
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
        //GD.Print("Add Resources");
        foreach (Hex hex in gameBoard.gameHexDict.Keys)
        {
            if(Global.gameManager.game.mainGameBoard.gameHexDict[hex].resourceType != ResourceType.None)
            {
                //GD.Print("New Resource");
                Global.gameManager.graphicManager.NewResource(Global.gameManager.game.mainGameBoard.gameHexDict[hex].resourceType, hex);
            }
        }
    }

    private void Add3DHexFeatures()
    {
        foreach (Hex hex in gameBoard.gameHexDict.Keys)
        {
            Point point = layout.HexToPixel(hex);
            foreach (FeatureType feature in gameBoard.gameHexDict[hex].featureSet)
            {
                Global.gameManager.graphicManager.NewFeature(hex, feature);
            }
        }
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
                terrainInfoImage.SetPixel(newQ, wrapHex.r, new Godot.Color(0.1f, 0, 0, 1)); 
            }
            else if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].terrainTemp == TerrainTemperature.Plains)
            {
                terrainInfoImage.SetPixel(newQ, wrapHex.r, new Godot.Color(0.2f, 0, 0, 1));
            }
            else if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].terrainTemp == TerrainTemperature.Grassland)
            {
                terrainInfoImage.SetPixel(newQ, wrapHex.r, new Godot.Color(0.3f, 0, 0, 1)); 
            }
            else if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].terrainTemp == TerrainTemperature.Tundra)
            {
                terrainInfoImage.SetPixel(newQ, wrapHex.r, new Godot.Color(0.4f, 0, 0, 1)); 
            }
            else if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].terrainTemp == TerrainTemperature.Arctic)
            {
                terrainInfoImage.SetPixel(newQ, wrapHex.r, new Godot.Color(0.5f, 0, 0, 1));
            }

            if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].terrainType == TerrainType.Flat)
            {
                terrainInfoImage.SetPixel(newQ, wrapHex.r, new Godot.Color(terrainInfoImage.GetPixel(newQ, wrapHex.r).R, 0.1f, 0, 1));
            }
            else if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].terrainType == TerrainType.Rough)
            {
                terrainInfoImage.SetPixel(newQ, wrapHex.r, new Godot.Color(terrainInfoImage.GetPixel(newQ, wrapHex.r).R, 0.2f, 0, 1)); 
            }
            else if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].terrainType == TerrainType.Mountain)
            {
                terrainInfoImage.SetPixel(newQ, wrapHex.r, new Godot.Color(terrainInfoImage.GetPixel(newQ, wrapHex.r).R, 0.3f, 0, 1)); 
            }
            else if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].terrainType == TerrainType.Coast)
            {
                terrainInfoImage.SetPixel(newQ, wrapHex.r, new Godot.Color(terrainInfoImage.GetPixel(newQ, wrapHex.r).R, 0.4f, 0, 1)); 
            }
            else if (Global.gameManager.game.mainGameBoard.gameHexDict[hex].terrainType == TerrainType.Ocean)
            {
                terrainInfoImage.SetPixel(newQ, wrapHex.r, new Godot.Color(terrainInfoImage.GetPixel(newQ, wrapHex.r).R, 0.5f, 0, 1)); 
            }
        }

        //terrainInfoImage.SavePng("testInfo.png");
        return terrainInfoImage;
    }
}
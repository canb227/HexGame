using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public partial class HexTest : Node3D
{
    Game game;

    GraphicManager graphicManager;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
/*        Layout pointyReal = new Layout(Layout.pointy, new Point(10, 10), new Point(0, 0));
        Layout pointy = new Layout(Layout.pointy, new Point(-10, 10), new Point(0, 0));
        Global.layout = pointy;
        //game = GameTests.GameStartTest();
        //game = GameTests.MapLoadTest();
        game = GameTests.TestSlingerCombat();

        //Global.gameManager.SaveGame();

        //Game loadedGame = Global.gameManager.LoadGame("C:/Users/jeffe/Desktop/Stuff/HexGame/game_data.dat");
        
        graphicManager = new GraphicManager(game, pointy);
        graphicManager.Name = "GraphicManager";
        Camera3D camera3D = GetChild<Camera3D>(0);//TODO
       // Global.gameManager.graphicManager.camera = camera3D as Camera;
       // Global.camera.SetGame(game);
       // Global.camera.SetGraphicManager(graphicManager);
        AddChild(graphicManager);*/

        //new age shit ignore it
        /*        Image terrainTypeImage = GenerateHexImage(game.mainGameBoard, pointyReal);
                Image mapImage;
                FastNoiseLite noise = new FastNoiseLite();
                noise.Frequency = 0.0005f;
                noise.Seed = 1;
                noise.FractalType = FastNoiseLite.FractalTypeEnum.None;
                noise.DomainWarpEnabled = false;
                mapImage = noise.GetImage(terrainTypeImage.GetWidth(), terrainTypeImage.GetHeight());
                mapImage.SavePng("testnoise.png");
                float heightScale = 400.0f;
                int x_axis = (int)(2 * pointyReal.size.x * (game.mainGameBoard.right - game.mainGameBoard.left)) + (int)((game.mainGameBoard.bottom - game.mainGameBoard.top) * pointyReal.size.x);
                int y_axis = (int)(2 * pointyReal.size.y * (game.mainGameBoard.bottom - game.mainGameBoard.top));
                //GD.Print(x_axis + " " + y_axis);
                BuildMesh((mapImage, terrainTypeImage), heightScale, x_axis, y_axis, new Vector3(0.0f, 0.0f, 0.0f), 1);*/
    }

    public override void _PhysicsProcess(double delta)
    {
        game.turnManager.EndCurrentTurn(0);
        game.turnManager.EndCurrentTurn(2);
        List<int> waitingForPlayerList = game.turnManager.CheckTurnStatus();
        if (!waitingForPlayerList.Any())
        {
            game.turnManager.StartNewTurn();
            graphicManager.StartNewTurn();
        }
        else
        {
            //push waitingForPlayerList to UI
        }
    }



    private void UpdateBoard(Game game, Layout pointy)
    {
        
    }



    Image GenerateHexImage(GameBoard gameBoard, Layout layout)
    {
        double xsize = Math.Abs(layout.size.x* (((double)gameBoard.right - (double)gameBoard.left) + ((double)gameBoard.bottom - (double)gameBoard.top)/2));
        double ysize = Math.Abs(layout.size.y *((double)(gameBoard.bottom - gameBoard.top)));
        Image terrainTemperatureImage = Image.CreateEmpty((int)Math.Ceiling(xsize*2), (int)Math.Ceiling(ysize *2), false, Image.Format.Rgba8);
        terrainTemperatureImage.Fill(new Godot.Color(0.5f, 0.5f, 0.5f, 1f));
        List<Vector2I> hexagonPixels = new List<Vector2I>();

        for(double x = -layout.size.x; x < layout.size.x; x += 0.2f)
        {
            for(double y = -layout.size.y; y < layout.size.y; y += 0.2f)
            {
                Hex hex = layout.PixelToHex(new Point(x, y)).HexRound();
                if (hex.q == 0 && hex.r == 0 && hex.s == 0)
                {
                    hexagonPixels.Add(new Vector2I((int)x, (int)y));
                }
            }
        }


        foreach (Hex hex in gameBoard.gameHexDict.Keys.ToList())
        {
            
            Godot.Color color;
            switch (game.mainGameBoard.gameHexDict[hex].terrainType)
            {
                case TerrainType.Rough:
                    //50% of noise map
                    color = new Godot.Color(0.5f, 0.5f, 0.5f, 1f);
                    //List<Point> points = layout.PolygonCorners(hex);
                    break;
                case TerrainType.Mountain:
                    //~100% of noise map
                    color = new Godot.Color(1.0f, 1.0f, 1.0f, 1f);
                    break;
                default:
                    color = new Godot.Color(0.0f, 0.0f, 0.0f, 1f);
                    break;
            }
            Point hexPoint = layout.HexToPixel(hex);
            int hexX = (int)hexPoint.x;
            int hexY = (int)hexPoint.y;
            foreach (Vector2I vec in hexagonPixels)
            {
                int pixX = vec.X + hexX + (int)layout.size.x;
                int pixY = vec.Y + hexY + (int)layout.size.y;
                if (pixX > 0 && pixY > 0)
                {
                    terrainTemperatureImage.SetPixel(pixX, pixY, color);
                }
            }
        }

        terrainTemperatureImage.SavePng("test.png");
        Image blurredImage = ApplyGausBlur(terrainTemperatureImage, 2);
        blurredImage.SavePng("testblur.png");
        return blurredImage;
        
    }

    Image ApplyGausBlur(Image img, int radius)
    {
        int width = img.GetWidth();
        int height = img.GetHeight();

        Image blurredImg = (Image)img.Duplicate();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Godot.Color avgColor = new Godot.Color(0, 0, 0, 0);
                int count = 0;

                for (int dx = -radius; dx <= radius; dx++)
                {
                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        int nx = x + dx;
                        int ny = y + dy;

                        if (nx >= 0 && ny >= 0 && nx < width && ny < height)
                        {
                            avgColor += img.GetPixel(nx, ny);
                            count++;
                        }
                    }
                }

                avgColor /= count;
                blurredImg.SetPixel(x, y, avgColor);
            }
        }

        return blurredImg;
    }

    private Rid BuildMesh((Image heightMap, Image terrainTypeMap) maps, float heightScale, int width, int depth, Vector3 globalPosition, int resolution)
    {
        //myGlobalPosition = globalPosition;
        // Create an array for the vertices

        width = (width / resolution) + 1;
        depth = (depth / resolution) + 1;
        Vector3[] p_vertices = new Vector3[width * depth];
        GD.Print(width + " " + depth);

        // Populate the vertices array
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < depth; j++)
            {
                int index = i * depth + j;
                p_vertices[index] = new Vector3(i * resolution * 4, 0.0f, j * resolution * 4);
            }
        }

        // Create an array for the indices
        int[] p_indices = new int[(width - 1) * (depth - 1) * 6];

        // Populate the indices array
        for (int i = 0; i < width - 1; i++)
        {
            for (int j = 0; j < depth - 1; j++)
            {
                int index = i * (depth - 1) + j;
                p_indices[index * 6 + 0] = i * width + j;
                p_indices[index * 6 + 1] = (i + 1) * width + j;
                p_indices[index * 6 + 2] = i * width + j + 1;
                p_indices[index * 6 + 3] = (i + 1) * width + j;
                p_indices[index * 6 + 4] = (i + 1) * width + j + 1;
                p_indices[index * 6 + 5] = i * width + j + 1;
            }
        }

        // Create the AABB
        Aabb p_aabb = new Aabb(new Vector3(0, -2000.0f, 0), new Vector3(width * resolution * 4, 4000.0f, depth * resolution * 4)); // Adjust the height as needed

        // Create an array for the mesh data
        Godot.Collections.Array arrays = new Godot.Collections.Array();
        arrays.Resize((int)RenderingServer.ArrayType.Max);

        // Set the vertices and indices
        arrays[(int)RenderingServer.ArrayType.Vertex] = p_vertices;
        arrays[(int)RenderingServer.ArrayType.Index] = p_indices;

        //heightMapTexture = ImageTexture.CreateFromImage(maps.heightMap);
        //terrainTypeMapTexture = ImageTexture.CreateFromImage(maps.terrainTypeMap);

        // Create the mesh
        Rid meshRid = RenderingServer.MeshCreate();
        RenderingServer.MeshAddSurfaceFromArrays(meshRid, RenderingServer.PrimitiveType.Triangles, arrays);
        // Set the custom AABB
        RenderingServer.MeshSetCustomAabb(meshRid, p_aabb);
        return meshRid;
    }

}

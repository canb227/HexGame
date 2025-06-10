using Godot;
using NetworkMessages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MapGenerator
{
    public const int TINY_WIDTH = 12;
    public const int TINY_HEIGHT = 12;
    public const int TINY_ARCTIC_HEIGHT = 1;
    public const int SMALL_WIDTH = 24;
    public const int SMALL_HEIGHT = 24;
    public const int SMALL_ARCTIC_HEIGHT = 1;
    public const int MEDIUM_WIDTH = 48;
    public const int MEDIUM_HEIGHT = 24;
    public const int MEDIUM_ARCTIC_HEIGHT = 1;
    public const int LARGE_WIDTH = 64;
    public const int LARGE_HEIGHT = 48;
    public const int LARGE_ARCTIC_HEIGHT = 2;
    public const int HUGE_WIDTH = 128;
    public const int HUGE_HEIGHT = 64;
    public const int HUGE_ARCTIC_HEIGHT = 2;

    public int mapWidth;
    public int mapHeight;
    public MapSize mapSize;
    public int numberOfPlayers;
    public int numberOfHumanPlayers;
    public bool generateRivers;
    public ResourceAmount resourceAmount;
    public MapType mapType;
    public int erosionFactor = 20;

    public List<List<AbstractHex>> abstractHexGrid;

    public struct AbstractHex
    {
        public int x;
        public int y;
        public TerrainType terrainType;
        public TerrainTemperature terrainTemperature;
        public ResourceType resourceType;
        public List<FeatureType> features;
    }

    public enum MapSize
    {
        Tiny,
        Small,
        Medium,
        Large,
        Huge,
    }

    public enum ResourceAmount
    {
        None,
        Low,
        Medium,
        High,
    }

    public enum MapType
    {
        Continents,
        ContinentsWithIslands,
        Pangea,
        DebugSquare,
        DebugRandom,
    }

    public MapGenerator()
    {

    }

    public void InitMap()
    {
        Global.debugLog("Starting map generation.");


        switch (mapSize)
        {
            case MapGenerator.MapSize.Tiny:
                mapWidth = MapGenerator.TINY_WIDTH;
                mapHeight = MapGenerator.TINY_HEIGHT;
                break;
            case MapGenerator.MapSize.Small:
                mapWidth = MapGenerator.SMALL_WIDTH;
                mapHeight = MapGenerator.SMALL_HEIGHT;
                break;
            case MapGenerator.MapSize.Medium:
                mapWidth = MapGenerator.MEDIUM_WIDTH;
                mapHeight = MapGenerator.MEDIUM_HEIGHT;
                break;
            case MapGenerator.MapSize.Large:
                mapWidth = MapGenerator.LARGE_WIDTH;
                mapHeight = MapGenerator.LARGE_HEIGHT;
                break;
            case MapGenerator.MapSize.Huge:
                mapWidth = MapGenerator.HUGE_WIDTH;
                mapHeight = MapGenerator.HUGE_HEIGHT;
                break;
        }
        Global.debugLog("Map Width: " + mapWidth);
        Global.debugLog("Map Height: " + mapHeight);

        abstractHexGrid = new List<List<AbstractHex>>();

        for (int y = 0; y < mapHeight; y++)
        {
            abstractHexGrid.Add(new List<AbstractHex>());
            for (int x = 0; x < mapWidth; x++)
            {
                AbstractHex hex = new AbstractHex();
                hex.x = x;
                hex.y = y;
                hex.resourceType = ResourceType.None;
                hex.terrainTemperature = AssignTemperature(y);
                hex.terrainType = TerrainType.Ocean;
                hex.features = new List<FeatureType>();
                abstractHexGrid[y].Add(hex);
            }
        }
    }

    private TerrainTemperature AssignTemperature(int y)
    {
        TerrainTemperature retval;
        if (y < 0.1f * mapHeight)
        { 
            //top of map
            retval = TerrainTemperature.Arctic;
        }
        else if (y<0.2f*mapHeight)
        {
            retval = TerrainTemperature.Tundra;
        }
        else if (y < 0.3f * mapHeight)
        {
            retval = TerrainTemperature.Grassland;
        }
        else if (y < 0.45f * mapHeight)
        {
            retval = TerrainTemperature.Plains;
        }
        else if (y < 0.55f * mapHeight)
        {
            retval = TerrainTemperature.Desert;
        }
        else if (y < 0.70f * mapHeight)
        {
            retval = TerrainTemperature.Plains;
        }
        else if (y < 0.8f * mapHeight)
        {
            retval = TerrainTemperature.Grassland;
        }
        else if (y < 0.9f * mapHeight)
        {
            retval = TerrainTemperature.Tundra;
        }
        else
        {
            retval = TerrainTemperature.Arctic;
        }
        return retval;
    }

    public void debugSquare()
    {
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                AbstractHex hex = abstractHexGrid[y][x];

                // Assign terrain type based on position
                if (x == 0 || y == 0 || x==mapWidth-1 || y==mapHeight-1)
                {
                    hex.terrainType = TerrainType.Ocean;
                }
                else if (x == 1 || y == 1 || x == mapWidth - 2 || y == mapHeight - 2)
                {
                    hex.terrainType = TerrainType.Coast;
                }
                else if (x == 2 || y == 2 || x == mapWidth - 3 || y == mapHeight - 3)
                {
                    hex.terrainType = TerrainType.Flat;
                }
                else if (x == 3 || y == 3 || x == mapWidth - 4 || y == mapHeight - 4)
                {
                    hex.terrainType = TerrainType.Flat;
                }
                else if (x == 4 || y == 4 || x == mapWidth - 5 || y == mapHeight - 5)
                {
                    hex.terrainType = TerrainType.Flat;
                }
                else if (x == 5 || y == 5 || x == mapWidth - 6 || y == mapHeight - 6)
                {
                    hex.terrainType = TerrainType.Flat;
                }
                else
                {
                    hex.terrainType = TerrainType.Rough;
                }
                if (Math.Abs(x - (mapWidth / 2)) < 2 && Math.Abs(y - (mapHeight / 2)) < 2)
                {
                    hex.terrainType = TerrainType.Mountain;
                }

                // Assign resource type
                hex.resourceType = ResourceType.None; // Default to None for simplicity
                hex.features = new List<FeatureType>();
                
                abstractHexGrid[y][x] = hex;
            }
        }
        AddFeatures();
        AddResources();
    }

    public void debugRandom()
    {
        Random rnd = new Random();
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                AbstractHex hex = abstractHexGrid[y][x];

                // Randomly assign terrain type
                hex.terrainType = (TerrainType)rnd.Next(0, Enum.GetValues(typeof(TerrainType)).Length);
                
                // Randomly assign terrain temperature
                hex.terrainTemperature = (TerrainTemperature)rnd.Next(0, Enum.GetValues(typeof(TerrainTemperature)).Length);
                
                // Randomly assign resource type
                hex.resourceType = (ResourceType)rnd.Next(0, Enum.GetValues(typeof(ResourceType)).Length);
                
                // Randomly assign features
                hex.features.Clear();
                if (rnd.NextDouble() < 0.5)
                {
                    hex.features.Add(FeatureType.Forest);
                }
                if (rnd.NextDouble() < 0.3)
                {
                    hex.features.Add(FeatureType.River);
                }
                
                abstractHexGrid[y][x] = hex;
            }
        }
        AddFeatures();
        AddResources();
    }

    public string MapToTextFormat()
    {
        string mapData = "";
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                mapData += (int)abstractHexGrid[y][x].terrainType;
                mapData += (int)abstractHexGrid[y][x].terrainTemperature;
                mapData += ParseFeatures(abstractHexGrid[y][x].features);
                mapData += ParseResources(abstractHexGrid[y][x].resourceType);
                mapData += " ";
            }
            mapData = mapData.Substring(0,mapData.Length-1);
            mapData += "\n";
        }
        //Global.debugLog(mapData);
        return mapData;
    }
    public string ParseResources(ResourceType resourceType)
    {
        //Global.debugLog(((char)resourceType).ToString());
        return ((char)resourceType).ToString();
    }

    public string ParseFeatures(List<FeatureType> features)
    {
        foreach (FeatureType feature in features)
        {
            if (feature == FeatureType.Forest)
                return "1";

            if (feature == FeatureType.River)
                return "2";

            if (feature == FeatureType.Road)
                return "3";
        }
        return "0";
    }

    private void generateContinentsMap()
    {
        int startingRegionSizeWidth = (mapWidth / 2)-2;
        int startingRegionSizeHeight = (int)Math.Floor(mapHeight*0.9);
        Random rng = new Random();
        FastNoiseLite noise = new FastNoiseLite();
        noise.Seed = rng.Next(0, 1000000);
        noise.NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth;
        noise.Frequency = 0.1f;
        noise.FractalOctaves = 6;
        noise.FractalGain = 0.75f;
       
        for (int y = (int)Math.Ceiling(mapHeight * 0.05); y < (int)Math.Floor(mapHeight * 0.95); y++)
        {
            for (int x = 1; x < startingRegionSizeWidth; x++)
            {
                float noiseValue = noise.GetNoise2D(y,x)/2 +0.5f;
                //Global.debugLog("Noise Value: " + noiseValue);
                AbstractHex hex = abstractHexGrid[y][x];
                hex.terrainType = NoiseToTerrainType(noiseValue);
                abstractHexGrid[y][x] = hex;
            }
        }

        for (int y = (int)Math.Ceiling(mapHeight * 0.05); y < (int)Math.Floor(mapHeight * 0.95); y++)
        {
            for (int x = startingRegionSizeWidth+2; x < mapWidth-2; x++)
            {
                float noiseValue = noise.GetNoise2D(y, x) / 2 + .5f;
                AbstractHex hex = abstractHexGrid[y][x];
                hex.terrainType = NoiseToTerrainType(noiseValue);
                abstractHexGrid[y][x] = hex;
            }
        }

        for (int i = 0; i < erosionFactor; i++)
        {
            ErodeEdges();
        }

        GenerateCoasts();
        switch (mapSize)
        {
            case MapSize.Tiny:
                AddArctic(TINY_ARCTIC_HEIGHT);
                break;
            case MapSize.Small:
                AddArctic(SMALL_ARCTIC_HEIGHT);
                break;
            case MapSize.Medium:
                AddArctic(MEDIUM_ARCTIC_HEIGHT);
                break;
            case MapSize.Large:
                AddArctic(LARGE_ARCTIC_HEIGHT);
                break;
            case MapSize.Huge:
                AddArctic(HUGE_ARCTIC_HEIGHT);
                break;
            default:
                break;
        }

        AddFeatures();
        AddResources();

    }

    private void AddFeatures()
    {
        Random rng = new Random();
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                AbstractHex hex = abstractHexGrid[y][x];
                if (hex.terrainType == TerrainType.Flat || hex.terrainType == TerrainType.Rough)
                {
                    if (rng.NextDouble() > 0.5f)
                    {
                        //Global.debugLog("added tree");
                        hex.features.Add(FeatureType.Forest);
                    }
                }
                if (hex.terrainTemperature == TerrainTemperature.Grassland && hex.terrainType == TerrainType.Flat)
                {
                    if (rng.NextDouble() > 0.8f)
                    {
                        hex.features.Add(FeatureType.Wetland);
                    }
                }

                abstractHexGrid[y][x] = hex;
            }
        }
    }

    private void AddResources()
    {
        Random rng = new Random();
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                AbstractHex hex = abstractHexGrid[y][x];
                if (hex.terrainType == TerrainType.Rough)
                {
                    if (rng.NextDouble() > 0.9f)
                    {
                        double resourceRoll = rng.NextDouble();
                        if (resourceRoll < 0.2f)
                        {
                            hex.resourceType = ResourceType.Coal;
                        }
                        else if (resourceRoll < 0.4f)
                        {
                            hex.resourceType = ResourceType.Iron;
                        }
                        else if (resourceRoll < 0.6f)
                        {
                            hex.resourceType = ResourceType.Salt;
                        }
                        else if (resourceRoll < 0.8f)
                        {
                            hex.resourceType = ResourceType.Jade;
                        }
                        else
                        {
                            hex.resourceType = ResourceType.Uranium; // Default to None if no resource found
                        }
                    }
                }
                if (hex.terrainType == TerrainType.Flat)
                {
                    if (rng.NextDouble() > 0.9f)
                    {
                        double resourceRoll = rng.NextDouble();
                        if (resourceRoll < 0.2f)
                        {
                            hex.resourceType = ResourceType.Wheat;
                        }
                        else if (resourceRoll < 0.4f)
                        {
                            hex.resourceType = ResourceType.Cotton;
                        }
                        else if (resourceRoll < 0.6f)
                        {
                            hex.resourceType = ResourceType.Sheep;
                        }
                        else if (resourceRoll < 0.8f)
                        {
                            hex.resourceType = ResourceType.Tobacco;
                        }
                        else
                        {
                            hex.resourceType = ResourceType.Horses; // Default to None if no resource found
                        }
                    }
                }
                abstractHexGrid[y][x] = hex;
            }
        }
    }

    private void ErodeEdges()
    {
        Random rng = new Random();
        List<AbstractHex> toErode = new List<AbstractHex>();
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if(abstractHexGrid[y][x].terrainType!=TerrainType.Ocean)
                {
                    if (HasOceanNeighbor(abstractHexGrid[y][x]))
                    {
                        toErode.Add(abstractHexGrid[y][x]);
                    }
                }
            }
        }
        foreach (var h in toErode)
        {
            if (rng.NextDouble() > 0.8f)
            {
                AbstractHex hex = h;
                hex.terrainType = TerrainType.Ocean;
                abstractHexGrid[hex.y][hex.x] = hex;
            }
        }
    }


    private void AddArctic(int arcticHeight)
    {
        for (int y = 0; y < arcticHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {

                AbstractHex hex = abstractHexGrid[y][x];
                hex.terrainTemperature = TerrainTemperature.Arctic;
                hex.terrainType = TerrainType.Mountain;
                abstractHexGrid[y][x] = hex;
            }
        }
        for (int y = mapHeight - arcticHeight; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                AbstractHex hex = abstractHexGrid[y][x];
                hex.terrainTemperature = TerrainTemperature.Arctic;
                hex.terrainType = TerrainType.Mountain; 
                abstractHexGrid[y][x] = hex;
            }
        }
    }

    private void GenerateCoasts()
    {
        List<AbstractHex> coasts = new List<AbstractHex>();
        Random rng = new Random();
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                AbstractHex hex = abstractHexGrid[y][x];
                if (hex.terrainType==TerrainType.Ocean)
                {
                    
                    if (HasNonOceanNeighbor(hex))
                    {
                        //Global.debugLog("hex detected as coast");
                        coasts.Add(hex);
                    }

                }
            }
        }
        foreach (var h in coasts)
        {
            if (true || rng.NextDouble() > 0.1f)
            {
                AbstractHex hex = h;
                hex.terrainType = TerrainType.Coast;
                abstractHexGrid[hex.y][hex.x] = hex;
            }
        }
    }
    private bool HasNonOceanNeighbor(AbstractHex hex)
    {
        if (hex.x > 0 && abstractHexGrid[hex.y][hex.x - 1].terrainType != TerrainType.Ocean)
        {
            return true;
        }
        else if (hex.x < mapWidth - 1 && abstractHexGrid[hex.y][hex.x + 1].terrainType != TerrainType.Ocean)
        {
            return true;
        }
        else if (hex.y > 0 && abstractHexGrid[hex.y - 1][hex.x].terrainType != TerrainType.Ocean)
        {
            return true;
        }
        else if (hex.y < mapHeight - 1 && abstractHexGrid[hex.y + 1][hex.x].terrainType != TerrainType.Ocean)
        {
            return true;
        }
        else if (hex.x > 0 && hex.y > 0 && abstractHexGrid[hex.y - 1][hex.x - 1].terrainType != TerrainType.Ocean)
        {
            return true;
        }
        else if (hex.x < mapWidth - 1 && hex.y < mapHeight - 1 && abstractHexGrid[hex.y + 1][hex.x + 1].terrainType != TerrainType.Ocean)
        {
            return true;
        }
        else if (hex.x > 0 && hex.y < mapHeight - 1 && abstractHexGrid[hex.y + 1][hex.x - 1].terrainType != TerrainType.Ocean)
        {
            return true;
        }
        else if (hex.x < mapWidth - 1 && hex.y > 0 && abstractHexGrid[hex.y - 1][hex.x + 1].terrainType != TerrainType.Ocean)
        {
            return true;
        }
        else
        {
            return false;
        }

    }
    private bool HasOceanNeighbor(AbstractHex hex)
    {
        if (hex.x>0&& abstractHexGrid[hex.y][hex.x - 1].terrainType == TerrainType.Ocean)
        {
            return true;
        }
        else if (hex.x<mapWidth-1 && abstractHexGrid[hex.y][hex.x + 1].terrainType == TerrainType.Ocean)
        {
            return true;
        }
        else if (hex.y>0 && abstractHexGrid[hex.y - 1][hex.x].terrainType == TerrainType.Ocean)
        {
            return true;
        }
        else if (hex.y<mapHeight-1 && abstractHexGrid[hex.y + 1][hex.x].terrainType == TerrainType.Ocean)
        {
            return true;
        }
        /*else if (hex.x > 0 && hex.y > 0 && abstractHexGrid[hex.y - 1][hex.x - 1].terrainType == TerrainType.Ocean)
        {
            return true;
        }
        else if (hex.x < mapWidth - 1 && hex.y < mapHeight - 1 && abstractHexGrid[hex.y + 1][hex.x + 1].terrainType == TerrainType.Ocean)
        {
            return true;
        }
        else if (hex.x > 0 && hex.y < mapHeight - 1 && abstractHexGrid[hex.y + 1][hex.x - 1].terrainType == TerrainType.Ocean)
        {
            return true;
        }
        else if (hex.x < mapWidth - 1 && hex.y > 0 && abstractHexGrid[hex.y - 1][hex.x + 1].terrainType == TerrainType.Ocean)
        {
            return true;
        }*/
        else
        {
            return false; 
        }

    }

    private TerrainType NoiseToTerrainType(float noiseValue)
    {
        TerrainType retval;// Convert noise value to terrain type
        if (noiseValue < 0.1f)
        {
            retval= TerrainType.Ocean;
        }
        if (noiseValue < 0.5f)
        {
            retval = TerrainType.Flat;
        }
        else if (noiseValue < 0.60f)
        {
            retval = TerrainType.Rough;
        }
        else
        {
            retval = TerrainType.Mountain;
        }
        return retval;
    }

    internal string GenerateMap()
    {
        InitMap();
        switch (mapType)
        {
            case MapType.Continents:
                generateContinentsMap();
                break;
            case MapType.ContinentsWithIslands:
                generateContinentsMap();
                break;
            case MapType.Pangea:
                generateContinentsMap();
                break;
            case MapType.DebugSquare:
                debugSquare();
                break;
            case MapType.DebugRandom:
                debugRandom();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(MapType), "Unknown map type: " + mapType);
        }
        
        return MapToTextFormat();
    }
}


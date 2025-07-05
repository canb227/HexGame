using Godot;
using NetworkMessages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

public class MapGenerator
{
    public const int TINY_WIDTH = 44;
    public const int TINY_HEIGHT = 26;
    public const int TINY_ARCTIC_HEIGHT = 1;
    public const int TINY_EROSION_FACTOR = 5;

    public const int SMALL_WIDTH = 72;
    public const int SMALL_HEIGHT = 46;
    public const int SMALL_ARCTIC_HEIGHT = 1;
    public const int SMALL_EROSION_FACTOR = 10;

    public const int MEDIUM_WIDTH = 84;
    public const int MEDIUM_HEIGHT = 54;
    public const int MEDIUM_ARCTIC_HEIGHT = 2;
    public const int MEDIUM_EROSION_FACTOR = 15;

    public const int LARGE_WIDTH = 96;
    public const int LARGE_HEIGHT = 60;
    public const int LARGE_ARCTIC_HEIGHT = 2;
    public const int LARGE_EROSION_FACTOR = 20;

    public const int HUGE_WIDTH = 108;
    public const int HUGE_HEIGHT = 66;
    public const int HUGE_ARCTIC_HEIGHT = 2;
    public const int HUGE_EROSION_FACTOR = 25;

    public const int MEGAHUGE_WIDTH = 120;
    public const int MEGAHUGE_HEIGHT = 72;
    public const int MEGAHUGE_ARCTIC_HEIGHT = 3;
    public const int MEGAHUGE_EROSION_FACTOR = 30;

    public int mapWidth;
    public int mapHeight;
    public MapSize mapSize;
    public int numberOfPlayers;
    public int numberOfHumanPlayers;
    public bool generateRivers;
    public ResourceAmount resourceAmount;
    public MapType mapType;
    public int erosionFactor = 15;

    public int top = 0;
    public int left = 0;
    public int right = 0;
    public int bottom = 0;

    public Dictionary<Hex, AbstractHex> abstractHexGrid;

    public struct AbstractHex
    {
        public Hex hex;
        public TerrainType terrainType;
        public TerrainTemperature terrainTemperature;
        public ResourceType resourceType;
        public List<FeatureType> features;

        public override string ToString()
        {
            return $"Hex: {hex}, TerrainType: {terrainType}, Temperature: {terrainTemperature}, Resource: {resourceType}, Features: [{string.Join(", ", features)}]";
        }
    }

    public enum MapSize
    {
        Tiny,
        Small,
        Medium,
        Large,
        Huge,
        MegaHuge,
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
        DebugCoasts,
    }

    public MapGenerator()
    {

    }

    public void InitMap()
    {
        //Global.debugLog("Starting map generation.");


        switch (mapSize)
        {
            case MapGenerator.MapSize.Tiny:
                mapWidth = MapGenerator.TINY_WIDTH;
                mapHeight = MapGenerator.TINY_HEIGHT;
                erosionFactor = MapGenerator.TINY_EROSION_FACTOR;
                break;
            case MapGenerator.MapSize.Small:
                mapWidth = MapGenerator.SMALL_WIDTH;
                mapHeight = MapGenerator.SMALL_HEIGHT;
                erosionFactor = MapGenerator.SMALL_EROSION_FACTOR;
                break;
            case MapGenerator.MapSize.Medium:
                mapWidth = MapGenerator.MEDIUM_WIDTH;
                mapHeight = MapGenerator.MEDIUM_HEIGHT;
                erosionFactor = MapGenerator.MEDIUM_EROSION_FACTOR;
                break;
            case MapGenerator.MapSize.Large:
                mapWidth = MapGenerator.LARGE_WIDTH;
                mapHeight = MapGenerator.LARGE_HEIGHT;
                erosionFactor = MapGenerator.LARGE_EROSION_FACTOR;
                break;
            case MapGenerator.MapSize.Huge:
                mapWidth = MapGenerator.HUGE_WIDTH;
                mapHeight = MapGenerator.HUGE_HEIGHT;
                erosionFactor = MapGenerator.HUGE_EROSION_FACTOR;
                break;
            case MapGenerator.MapSize.MegaHuge:
                mapWidth = MapGenerator.MEGAHUGE_WIDTH;
                mapHeight = MapGenerator.MEGAHUGE_HEIGHT;
                erosionFactor = MapGenerator.MEGAHUGE_EROSION_FACTOR;
                break;
        }
        //Global.debugLog("Map Width: " + mapWidth);
        //Global.debugLog("Map Height: " + mapHeight);

        right = mapWidth - 1;
        bottom = mapHeight - 1;
        abstractHexGrid = new();

        if (mapType==MapType.DebugSquare || mapType==MapType.DebugRandom || mapType==MapType.DebugCoasts)
        {
            //Global.debugLog("Debug Map Type Selected, using square grid.");
            mapWidth = 12;
            mapHeight = 12;
            right = mapWidth - 1;
            bottom = mapHeight - 1;
        }

        for (int r = 0; r < mapHeight; r++)
        {
            int r_offset = r >> 1; //same as (int)Math.Floor(r/2.0f)
            for (int q = 0 - r_offset; q < mapWidth - r_offset; q++)
            {

                AbstractHex aHex = new AbstractHex();
                aHex.hex = new Hex(q, r, -q - r);
                aHex.resourceType = ResourceType.None;
                aHex.terrainTemperature = AssignTemperature(r);
                aHex.terrainType = TerrainType.Ocean;
                aHex.features = new List<FeatureType>();
                //Global.debugLog("Created hex at (" + r + "," + q + ")");


                Hex indexHex = new Hex(q, r, -q - r);
                /*
                 *                 if (r==0)
                {
                    Global.debugLog("Top Row Q: " + q);
                }
                if (r == 6)
                {
                    Global.debugLog("r=6 Q: " + q);
                }
                Hex badHex = new Hex(45, 6, -45 - 6);
                if (badHex.Equals(indexHex))
                {
                    Global.debugLog("Bad Hex Inits!" + indexHex);
                }*/
                abstractHexGrid.Add(aHex.hex, aHex);
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

    public void debugCoasts()
    {
        for (int r = 0; r < mapHeight; r++)
        {
            int r_offset = r >> 1; //same as (int)Math.Floor(r/2.0f)
            for (int q = 0 - r_offset; q < mapWidth - r_offset; q++)
            {
                if(q==5 && r==5)
                {
                    AbstractHex aHex = abstractHexGrid[new Hex(q,r,-q-r)];
                    aHex.terrainType = TerrainType.Flat;
                    abstractHexGrid[aHex.hex] = aHex;
                }
            }
        }
        GenerateCoasts();
    }


    public void debugSquare()
    {
        for (int r = 0; r < mapHeight; r++)
        {
            int r_offset = r >> 1; //same as (int)Math.Floor(r/2.0f)
            for (int q = 0 - r_offset; q < mapWidth - r_offset; q++)
            {
                AbstractHex hex = abstractHexGrid[new Hex(q, r, -q - r)];

                // Assign terrain type based on position
                if (q == 0 || r == 0 || q==mapWidth-1 || r==mapHeight-1)
                {
                    hex.terrainType = TerrainType.Ocean;
                }
                else if (q == 1 || r == 1 || q == mapWidth - 2 || r == mapHeight - 2)
                {
                    hex.terrainType = TerrainType.Ocean;
                }
                else if (q == 2 || r == 2 || q == mapWidth - 3 || r == mapHeight - 3)
                {
                    hex.terrainType = TerrainType.Flat;
                }
                else if (q == 3 || r == 3 || q == mapWidth - 4 || r == mapHeight - 4)
                {
                    hex.terrainType = TerrainType.Flat;
                }
                else
                {
                    hex.terrainType = TerrainType.Rough;
                }
                if (Math.Abs(q - (mapWidth / 2)) < 2 && Math.Abs(r - (mapHeight / 2)) < 2)
                {
                    hex.terrainType = TerrainType.Mountain;
                }

                // Assign resource type
                hex.resourceType = ResourceType.None; // Default to None for simplicity
                hex.features = new List<FeatureType>();
                
                abstractHexGrid[new Hex(q,r,-q-r)] = hex;
            }
        }
        GenerateCoasts();
        AddFeatures();
        AddResources();
    }

    public void debugRandom()
    {
        Random rnd = new Random();
        for (int r = 0; r < mapHeight; r++)
        {
            int r_offset = r >> 1; //same as (int)Math.Floor(r/2.0f)
            for (int q = 0 - r_offset; q < mapWidth - r_offset; q++)
            {
                AbstractHex hex = abstractHexGrid[new Hex(q,r,-q-r)];

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
                
                abstractHexGrid[new Hex(q,r,-q-r)] = hex;
            }
        }
        AddFeatures();
        AddResources();
    }

    public string MapToTextFormat()
    {
        string mapData = "";
        for (int r = 0; r < mapHeight; r++)
        {
            int r_offset = r >> 1; //same as (int)Math.Floor(r/2.0f)
            for (int q = 0 - r_offset; q < mapWidth - r_offset; q++)
            {
                mapData += (int)abstractHexGrid[new Hex(q,r,-q-r)].terrainType;
                mapData += (int)abstractHexGrid[new Hex(q,r,-q-r)].terrainTemperature;
                mapData += ParseFeatures(abstractHexGrid[new Hex(q,r,-q-r)].features);
                mapData += ParseResources(abstractHexGrid[new Hex(q,r,-q-r)].resourceType);
                mapData += " ";
            }
            mapData = mapData.Substring(0,mapData.Length-1);
            mapData += "\n";
        }
        //Global.Log("\n"+mapData);
        return mapData;
    }
    public string ParseResources(ResourceType resourceType)
    {
        if (resourceType!=ResourceType.None)
        {
            //Global.Log(resourceType.ToString());
        }

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

            if (feature == FeatureType.Coral)
                return "4";
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
       
        for (int r = (int)Math.Ceiling(mapHeight * 0.05); r < (int)Math.Floor(mapHeight * 0.95); r++)
        {
            int r_offset = r >> 1;
            for (int q = 2 - r_offset ; q < startingRegionSizeWidth - r_offset; q++)
            {
                float noiseValue = noise.GetNoise2D(r,q)/2 +0.5f;
                //Global.debugLog("Noise Value: " + noiseValue);
                AbstractHex hex = abstractHexGrid[new Hex(q,r,-q-r)];
                hex.terrainType = NoiseToTerrainType(noiseValue);
                abstractHexGrid[new Hex(q,r,-q-r)] = hex;
            }
        }

        for (int r = (int)Math.Ceiling(mapHeight * 0.05); r < (int)Math.Floor(mapHeight * 0.95); r++)
        {
            int r_offset = r >> 1;
            for (int q = startingRegionSizeWidth+2 - r_offset; q < mapWidth-2 - r_offset; q++)
            {
                float noiseValue = noise.GetNoise2D(r, q) / 2 + .5f;

                Hex indexHex = new Hex(q, r, -q - r);

                /*
                Hex badHex = new Hex(45, 6, -45 - 6);
                if (badHex.Equals(indexHex))
                {
                    Global.debugLog("Bad Hex: " + indexHex);
                    Global.debugLog("mWidth: " + mapWidth + ", mHeight: " + mapHeight + " r_offset: " + r_offset + "startregionsize: " + startingRegionSizeWidth);
                }*/

                AbstractHex hex = abstractHexGrid[indexHex];
                hex.terrainType = NoiseToTerrainType(noiseValue);
                abstractHexGrid[new Hex(q,r,-q-r)] = hex;
            }
        }

        for (int i = 0; i < erosionFactor; i++)
        {
            ErodeEdges();
        }

        GenerateCoasts();
        ExtendCoasts();

        ErodeMountains();
        ErodeRough();
        AddHills();
        AddLakes();

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

    private void ErodeMountains()
    {
        Random rng = new Random();
        List<AbstractHex> toErode = new List<AbstractHex>();
        for (int r = 0; r < mapHeight; r++)
        {
            int r_offset = r >> 1;
            for (int q = 0 - r_offset; q < mapWidth - r_offset; q++)
            {
                AbstractHex hex = abstractHexGrid[new Hex(q, r, -q - r)];
                if (hex.terrainType == TerrainType.Mountain)
                {
                    if (rng.NextDouble() < 0.2f)
                    {
                        toErode.Add(hex);
                    }
                }
            }
        }
        foreach (var h in toErode)
        {
            AbstractHex hex = h;
            hex.terrainType = TerrainType.Rough;
            abstractHexGrid[hex.hex] = hex;
        }
    }


    private void ErodeRough()
    {
        Random rng = new Random();
        for (int r = 0; r < mapHeight; r++)
        {
            int r_offset = r >> 1;
            for (int q = 0 - r_offset; q < mapWidth - r_offset; q++)
            {
                AbstractHex hex = abstractHexGrid[new Hex(q, r, -q - r)];
                if (hex.terrainType == TerrainType.Rough)
                {
                    if (rng.NextDouble() < 0.2f)
                    {
                        hex.terrainType = TerrainType.Flat;
                        abstractHexGrid[hex.hex] = hex;
                    }
                }
            }
        }
    }

    private void AddHills()
    {
        Random rng = new Random();
        for (int r = 0; r < mapHeight; r++)
        {
            int r_offset = r >> 1;
            for (int q = 0 - r_offset; q < mapWidth - r_offset; q++)
            {
                AbstractHex hex = abstractHexGrid[new Hex(q, r, -q - r)];
                if (hex.terrainType == TerrainType.Flat)
                {
                    if (rng.NextDouble() < 0.2f)
                    {
                        hex.terrainType = TerrainType.Rough;
                        abstractHexGrid[hex.hex] = hex;
                    }
                }
            }
        }
    }

    private void AddLakes()
    {

    }

    private void ExtendCoasts()
    {
        Random rng = new Random();
        List<AbstractHex> toCoastsList = new List<AbstractHex>();
        for (int r = 0; r < mapHeight; r++)
        {
            int r_offset = r >> 1;
            for (int q = 0 - r_offset; q < mapWidth - r_offset; q++)
            {
                AbstractHex hex = abstractHexGrid[new Hex(q, r, -q - r)];
                if (hex.terrainType == TerrainType.Ocean)
                {

                    if (IsOceanTouchingCoast(hex))
                    {
                        if (rng.NextDouble() < 0.2f)
                        {
                            toCoastsList.Add(hex);
                        }

                    }

                }
            }
        }
        foreach (var h in toCoastsList)
        {
            AbstractHex hex = h;
            hex.terrainType = TerrainType.Coast;
            abstractHexGrid[hex.hex] = hex;
        }

    }
    private void AddFeatures()
    {
        Random rng = new Random();
        for (int r = 0; r < mapHeight; r++)
        {
            int r_offset = r >> 1;
            for (int q = 0 - r_offset; q < mapWidth-r_offset; q++)
            {
                AbstractHex hex = abstractHexGrid[new Hex(q,r,-q-r)];
                if (hex.terrainType == TerrainType.Flat || hex.terrainType == TerrainType.Rough)
                {
                    if (rng.NextDouble() < 0.5f)
                    {
                        //Global.debugLog("added tree");
                        hex.features.Add(FeatureType.Forest);
                    }
                }
                if (hex.terrainTemperature == TerrainTemperature.Grassland && hex.terrainType == TerrainType.Flat)
                {
                    if (rng.NextDouble() < 0.2f)
                    {
                        hex.features.Add(FeatureType.Wetland);
                    }
                }
                if (hex.terrainType == TerrainType.Coast)
                {
                    if (rng.NextDouble() < 0.15f)
                    {
                        hex.features.Add(FeatureType.Coral);
                    }
                }

                abstractHexGrid[new Hex(q,r,-q-r)] = hex;
            }
        }
    }

    private void AddResources()
    {
        Random rng = new Random();
        for (int r = 0; r < mapHeight; r++)
        {
            int r_offset = r >> 1;
            for (int q = 0 - r_offset; q < mapWidth - r_offset; q++)
            {
                AbstractHex hex = abstractHexGrid[new Hex(q,r,-q-r)];
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
                if (hex.terrainType == TerrainType.Coast)
                {
                    if (rng.NextDouble() > 0.9f)
                    {
                        double resourceRoll = rng.NextDouble();
                        if (resourceRoll < 0.2f)
                        {
                            hex.resourceType = ResourceType.None;
                        }
                        else
                        {
                            hex.resourceType = ResourceType.None; // Default to None if no resource found
                        }
                    }

                }
                abstractHexGrid[new Hex(q, r, -q - r)] = hex;
            }
        }
    }

    private void ErodeEdges()
    {
        Random rng = new Random();
        List<AbstractHex> toErode = new List<AbstractHex>();
        for (int r = 0; r < mapHeight; r++)
        {
            int r_offset = r >> 1;
            for (int q = 0 - r_offset; q < mapWidth - r_offset; q++)
            {
                if(abstractHexGrid[new Hex(q,r,-q-r)].terrainType!=TerrainType.Ocean)
                {
                    if (IsLandWithOceanNeighbor(abstractHexGrid[new Hex(q,r,-q-r)]))
                    {
                        toErode.Add(abstractHexGrid[new Hex(q,r,-q-r)]);
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
                abstractHexGrid[hex.hex] = hex;
            }
        }
    }


    private void AddArctic(int arcticHeight)
    {
        for (int r = 0; r < arcticHeight; r++)
        {
            int r_offset = r >> 1;
            for (int q = 0 - r_offset; q < mapWidth - r_offset; q++)
            {

                AbstractHex hex = abstractHexGrid[new Hex(q,r,-q-r)];
                hex.terrainTemperature = TerrainTemperature.Arctic;
                hex.terrainType = TerrainType.Mountain;
                abstractHexGrid[new Hex(q,r,-q-r)] = hex;
            }
        }
        for (int r = mapHeight - arcticHeight; r < mapHeight; r++)
        {
            int r_offset = r >> 1;
            for (int q = 0 - r_offset; q < mapWidth - r_offset; q++)
            {
                AbstractHex hex = abstractHexGrid[new Hex(q,r,-q-r)];
                hex.terrainTemperature = TerrainTemperature.Arctic;
                hex.terrainType = TerrainType.Mountain; 
                abstractHexGrid[new Hex(q,r,-q-r)] = hex;
            }
        }
    }

    private void GenerateCoasts()
    {
        Random rng = new Random();
        for (int r = 0; r < mapHeight; r++)
        {
            int r_offset = r >> 1;
            for (int q = 0 - r_offset; q < mapWidth - r_offset; q++)
            {
                AbstractHex hex = abstractHexGrid[new Hex(q,r,-q-r)];
                if (hex.terrainType==TerrainType.Ocean)
                {
                    
                    if (IsOceanTouchingLand(hex))
                    {
                        hex.terrainType = TerrainType.Coast;
                        abstractHexGrid[hex.hex] = hex;
                    }

                }
            }
        }
    }

    private bool IsOceanTouchingLand(AbstractHex hex)
    {
        Hex testHex = hex.hex;

        if (hex.terrainType == TerrainType.Ocean)
        {
            Hex[] neighbors = testHex.WrappingNeighbors(0, mapWidth - 1, mapHeight - 1);
            foreach (Hex neighbor in neighbors)
            {
                if (abstractHexGrid[neighbor].terrainType != TerrainType.Ocean && abstractHexGrid[neighbor].terrainType != TerrainType.Coast)
                {
                    //Global.debugLog("I am " + hex.hex.ToString() + "\n and I found neighbor that is Land!: " + abstractHexGrid[neighbor].hex.ToString());
                    return true;

                }
            }
        }
        return false;

    }

    private bool IsOceanTouchingCoast(AbstractHex hex)
    {
        Hex testHex = hex.hex;

        if (hex.terrainType == TerrainType.Ocean)
        {
            Hex[] neighbors = testHex.WrappingNeighbors(0, mapWidth - 1, mapHeight - 1);
            foreach (Hex neighbor in neighbors)
            {
                if (abstractHexGrid[neighbor].terrainType != TerrainType.Ocean && abstractHexGrid[neighbor].terrainType == TerrainType.Coast)
                {
                    //Global.debugLog("I am " + hex.hex.ToString() + "\n and I found neighbor that is Land!: " + abstractHexGrid[neighbor].hex.ToString());
                    return true;

                }
            }
        }
        return false;

    }


    private bool IsLandWithOceanNeighbor(AbstractHex hex)
    {
        Hex testHex = hex.hex;

        if (hex.terrainType != TerrainType.Ocean && hex.terrainType !=TerrainType.Coast)
        {
            Hex[] neighbors = testHex.WrappingNeighbors(0, mapWidth - 1, mapHeight - 1);
            foreach (Hex neighbor in neighbors)
            {
                if (abstractHexGrid[neighbor].terrainType == TerrainType.Ocean)
                {
                    //Global.debugLog("I am " + hex.hex.ToString() + "\n and I found neighbor that is Land!: " + abstractHexGrid[neighbor].hex.ToString());
                    return true;

                }
            }
        }
        return false;

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
            case MapType.DebugCoasts:
                debugCoasts();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(MapType), "Unknown map type: " + mapType);
        }
        
        return MapToTextFormat();
    }
}


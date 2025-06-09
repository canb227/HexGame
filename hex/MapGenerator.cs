using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MapGenerator
{

    public int mapWidth;
    public int mapHeight;
    public int numberOfPlayers;
    public int numberOfHumanPlayers;
    public bool generateRivers;
    public ResourceAmount resourceAmount;
    public MapType mapType;

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
        Global.debugLog("Map Width: " + mapWidth);
        Global.debugLog("Map Height: " + mapHeight);

        abstractHexGrid = new List<List<AbstractHex>>();

        for (int y = 0; y < mapHeight; y++)
        {
            abstractHexGrid.Add(new List<AbstractHex>());
            for (int x = 0; x < mapWidth; x++)
            {
                AbstractHex hex = new AbstractHex();
                hex.resourceType = ResourceType.None;
                hex.terrainTemperature = TerrainTemperature.Grassland;
                hex.terrainType = TerrainType.Ocean;
                hex.features = new List<FeatureType>();
                abstractHexGrid[y].Add(hex);
            }
        }
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
                mapData += ParseResources(abstractHexGrid[y][x].resourceType);
                mapData += ParseFeatures(abstractHexGrid[y][x].features);
                mapData += " ";
            }
            mapData = mapData.Substring(0,mapData.Length-1);
            mapData += "\n";
        }
        Global.debugLog(mapData);
        return mapData;
    }
    public string ParseResources(ResourceType resourceType)
    {
        switch (resourceType)
        {
            case ResourceType.None:
                return "0";
            default:
                return "0"; // Default to None if unknown
        }
    }

    public string ParseFeatures(List<FeatureType> features)
    {
        return "0";
    }

    internal string GenerateMap()
    {
        InitMap();
        switch (mapType)
        {
            case MapType.Continents:
                // Implement continent generation logic
                break;
            case MapType.ContinentsWithIslands:
                // Implement continent with islands generation logic
                break;
            case MapType.Pangea:
                // Implement Pangea generation logic
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


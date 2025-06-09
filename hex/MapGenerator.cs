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
    }

    public MapGenerator()
    {

    }

}


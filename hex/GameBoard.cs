using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data;
using System.IO;
using Godot;

[Serializable]
public class GameBoard
{

    public GameBoard()
    {
    }

    public void InitGameBoardFromFile(string mapName, int id)
    {
        gameHexDict = new();
        String mapData = System.IO.File.ReadAllText(mapName + ".map");
        ReadGameBoardData(mapData);
    }

    public void InitGameBoardFromData(string mapData, int id)
    {
        gameHexDict = new();
        ReadGameBoardData(mapData);
    }

    private void ReadGameBoardData(string mapData)
    {
        List<String> lines = mapData.Split('\n').ToList();
        //file format is 1110 1110 (each 4 numbers are a single hex)
        // first number is terraintype, second number is terraintemp, last number is features, last is resource type
        // 0, luxury, bonus, city, iron, horses, coal, oil, uranium, (lithium?), futurething
        int r = 0;
        int q = 0;
        foreach (String line in lines)
        {
            Queue<String> cells = new Queue<String>(line.Split(' ').ToList());
            int offset = r >> 1;
            //offset = (offset % cells.Count + cells.Count) % cells.Count; //negatives and overflow
            q = 0 - offset;
            /*            if(q < left)
                        {
                            left = q;
                        }*/
            for (int i = 1; i < offset; i++)
            {
                cells.Enqueue(cells.Dequeue());
            }
            foreach (String cell in cells)
            {
                if (cell.Length >= 4)
                {
                    TerrainType terrainType = (TerrainType)int.Parse(cell[0].ToString());
                    TerrainTemperature terrainTemperature = (TerrainTemperature)int.Parse(cell[1].ToString());
                    HashSet<FeatureType> features = new();
                    //cell[2] == 0 means no features
                    if (int.Parse(cell[2].ToString()) == 1)
                    {
                        features.Add(FeatureType.Forest);
                    }
                    if (int.Parse(cell[2].ToString()) == 2)
                    {
                        features.Add(FeatureType.River);
                    }
                    if (int.Parse(cell[2].ToString()) == 3)
                    {
                        features.Add(FeatureType.Road);
                    }
                    if (int.Parse(cell[2].ToString()) == 4)
                    {
                        features.Add(FeatureType.Coral);
                    }
                    if (int.Parse(cell[2].ToString()) == 5)
                    {
                        //openslot //TODO
                    }
                    if (int.Parse(cell[2].ToString()) == 6)
                    {
                        features.Add(FeatureType.Forest);
                        features.Add(FeatureType.River);
                    }
                    if (int.Parse(cell[2].ToString()) == 7)
                    {
                        features.Add(FeatureType.River);
                        features.Add(FeatureType.Road);
                    }
                    if (int.Parse(cell[2].ToString()) == 8)
                    {
                        features.Add(FeatureType.Forest);
                        features.Add(FeatureType.Road);
                    }
                    if (int.Parse(cell[2].ToString()) == 9)
                    {
                        features.Add(FeatureType.Forest);
                        features.Add(FeatureType.River);
                        features.Add(FeatureType.Road);
                    }
                    // if(int.Parse(cell[2].ToString()) == 9)
                    // {
                    //     features.Add(FeatureType.Forest);
                    //     features.Add(FeatureType.River);
                    //     features.Add(FeatureType.Road);
                    // }
                    //fourth number is for resources
                    ResourceType resource = ResourceLoader.resourceNames[cell[3].ToString()];
                    gameHexDict.Add(new Hex(q, r, -q - r), new GameHex(new Hex(q, r, -q - r), id, terrainType, terrainTemperature, resource, features, new List<int>(), null));
                }
                q += 1;
                if (q > right)
                {
                    right = q;
                }
            }
            r += 1;
        }
        this.bottom = r;
        GD.Print(left + "," + right + "," + bottom + "," + top);
    }

    public int id { get; set; }
    public Dictionary<Hex, GameHex> gameHexDict { get; set; } = new();
    public int top { get; set; } = 0;
    public int bottom { get; set; } = 0;
    public int left { get; set; } = 0;
    public int right { get; set; } = 0;

    public void OnTurnStarted(int turnNumber)
    {
        foreach (GameHex hex in gameHexDict.Values)
        {
            hex.OnTurnStarted(turnNumber);
        }
    }

    public void OnTurnEnded(int turnNumber)
    {
        foreach (GameHex hex in gameHexDict.Values)
        {
            hex.OnTurnEnded(turnNumber);
        }
    }

    public void PrintGameBoard()
    {
        //terraintype
        GameHex test;
        for(int r = top; r <= bottom; r++){
            String mapRow = ""; 
            if (r%2 == 1)
            {
                mapRow += " ";
            }
            for (int q = left; q <= right; q++){
                if(gameHexDict.TryGetValue(new Hex(q, r, -q-r), out test)){
                    if(test.terrainType == TerrainType.Flat)
                    {
                        mapRow += "F ";
                    }
                    else if(test.terrainType == TerrainType.Rough)
                    {
                        mapRow += "R ";
                    }
                    else if(test.terrainType == TerrainType.Mountain)
                    {
                        mapRow += "M ";
                    }
                    else if(test.terrainType == TerrainType.Coast)
                    {
                        mapRow += "C ";
                    }
                    else if(test.terrainType == TerrainType.Ocean)
                    {
                        mapRow += "O ";
                    }
                }
            }
            Console.WriteLine(mapRow);
        }
        Console.WriteLine();

        //features
        for(int r = top; r <= bottom; r++)
        {
            int r_offset = r>>1; //same as (int)Math.Floor(r/2.0f)
            String mapRow = ""; 
            if (r%2 == 1)
            {
                mapRow += " ";
            }
            for (int q = left - r_offset; q <= right - r_offset; q++){
                if(gameHexDict.TryGetValue(new Hex(q, r, -q-r), out test)){
                    foreach (FeatureType feature in test.featureSet)
                    {
                        if (feature == FeatureType.Road)
                        {
                            mapRow += "R ";
                            break;
                        }
                        else
                        {
                            mapRow += "* ";
                        }
                    }
                }
            }
            Console.WriteLine(mapRow);
        }
        Console.WriteLine();
    }
}

struct GameBoardTest
{
    
}

using Godot;
using NetworkMessages;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using static Google.Protobuf.Reflection.SourceCodeInfo.Types;
using static System.Runtime.InteropServices.JavaScript.JSType;
using String = System.String;


[GlobalClass]
public partial class GameManager : Node
{
    private const bool DEBUGNETWORK = true;

    public bool isHost = false;
    public static GameManager instance;
    public GraphicManager graphicManager;
    public AIManager AIManager;
    public Game game;
    public AudioManager audioManager = new();
    public bool gameStarted = false;
    public int SkipPlayerTurns = -1;
    internal int TurnsToSkip = 1;

    JsonSerializerOptions options;

    public GameManager()
    {
        options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new HexJsonConverter() },
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers =
                {
                    ti =>
                    {
                        if (ti.Type == typeof(Unit))
                        {
                            ti.PolymorphismOptions = new JsonPolymorphismOptions
                            {
                                TypeDiscriminatorPropertyName = "$type",
                                DerivedTypes =
                                {
                                    new JsonDerivedType(typeof(Hero), "hero")
                                }
                            };
                        }
                    }
                }
            }
        };
        instance = this;
        Global.gameManager = this;
        game = new Game();
        AddChild(audioManager);
        //startTerrainDemo();
    }


    /*
    public void SaveGame(String filePath)
    {
        Global.Log("So you're saving the game, time to dump some data.");
        Global.Log($"We're saving the game on turn #{Global.gameManager.game.turnManager.currentTurn}");
        Global.Log($"Lets check in on our AI turn statuses. (NUMAI:{Global.gameManager.game.numAI}) If all is well these should all be false.");
        foreach (AIUtils.AI ai in Global.gameManager.AIManager.aiList)
        {
            Global.Log($"AI#{ai.player.teamNum}: turn finished?: {ai.player.turnFinished}");
        }
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(game, options);


    }

    public byte[] SaveGameRaw()
    {
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(game, options);
        using (var compressedStream = new MemoryStream())
        {
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
            {
                zipStream.Write(json, 0, json.Length);
            }
            return compressedStream.ToArray();
        }
    }

    public byte[] SaveGameRaw(Game game)
    {
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(game, options);
        using (var compressedStream = new MemoryStream())
        {
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
            {
                zipStream.Write(json, 0, json.Length);
                zipStream.Close();
                return compressedStream.ToArray();
            }

        }
    }

    public byte[] ReadSave(String filePath)
    {
        Godot.FileAccess fileAccess = Godot.FileAccess.Open(filePath, Godot.FileAccess.ModeFlags.Read);
        return fileAccess.GetBuffer((long)fileAccess.GetLength());
    }


    public Game LoadGame(String filePath)
    {
        Global.Log("Loading Game from file: " + filePath);
        Godot.FileAccess fileAccess = Godot.FileAccess.Open(filePath, Godot.FileAccess.ModeFlags.Read);

        byte[] compressedBytes = fileAccess.GetBuffer((long)fileAccess.GetLength());
        using (var compressedStream = new MemoryStream(compressedBytes))
        {
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            {
                using (var jsonUTF8 = new MemoryStream())
                {
                    zipStream.CopyTo(jsonUTF8);
                    Game retVal = JsonSerializer.Deserialize<Game>(jsonUTF8);
                    fileAccess.Close();
                    return retVal;
                }

            }
        }
    }

    public Game LoadGameRaw(byte[] rawSave)
    {
        byte[] compressedBytes = rawSave;
        using (var compressedStream = new MemoryStream(compressedBytes))
        {
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            {
                using (var jsonUTF8 = new MemoryStream())
                {
                    zipStream.CopyTo(jsonUTF8);
                    Game retVal = JsonSerializer.Deserialize<Game>(jsonUTF8);
                    return retVal;
                }

            }
        }
    }*/

    public byte[] GameToUTF8(Game game)
    {
        return JsonSerializer.SerializeToUtf8Bytes(game, options);
    }

    public Game UTF8ToGame(byte[] UTF8)
    {
        return JsonSerializer.Deserialize<Game>(UTF8, options);
    }


    public string GameToString(Game game)
    {
        return JsonSerializer.Serialize(game, options);
    }

    public Game StringToGame(string str)
    {
        return JsonSerializer.Deserialize<Game>(str, options);
    }

    public byte[] UTF8ToCompressed(byte[] UTF8Bytes)
    {
        MemoryStream compressedBytesStream = new MemoryStream();
        GZipStream gZipStream = new GZipStream(compressedBytesStream, CompressionMode.Compress);
        gZipStream.Write(UTF8Bytes);
        return compressedBytesStream.ToArray();
    }

    public byte[] CompressedToUTF8(byte[] compressedBytes)
    {
        MemoryStream compressedBytesStream = new MemoryStream(compressedBytes);
        MemoryStream decompressedBytesStream = new MemoryStream();
        GZipStream gZipStream = new GZipStream(compressedBytesStream, CompressionMode.Decompress);
        gZipStream.CopyTo(decompressedBytesStream);
        return decompressedBytesStream.ToArray();
    }

    public byte[] CompressString(string str)
    {


        Global.Log($"Attempting string compression... | og string length: {str.Length}");
        MemoryStream compressedBytesStream = new MemoryStream();
        GZipStream gZipStream = new GZipStream(compressedBytesStream, CompressionMode.Compress);
        byte[] stringBytes = Encoding.UTF8.GetBytes(str);
        Global.Log($"String is {stringBytes.Length} bytes long.");
        gZipStream.Write(stringBytes);
        gZipStream.Flush();
        byte[] result = compressedBytesStream.ToArray();
        Global.Log($"Compressed string is {result.Length} bytes long.");
        return result;
    }

    public string CompressedToString(byte[] compressedBytes)
    {
        MemoryStream compressedBytesStream = new MemoryStream(compressedBytes);
        MemoryStream decompressedBytesStream = new MemoryStream();
        GZipStream gZipStream = new GZipStream(compressedBytesStream, CompressionMode.Decompress);
        gZipStream.CopyTo(decompressedBytesStream);
        return Encoding.UTF8.GetString(decompressedBytesStream.ToArray());
    }

    public void SaveGameToDisk(string filePath)
    {
        string JSONString = GameToString(game);

        Global.Log($"Storing uncompressed save as a PascalString file for debug purposes...");
        Global.Log($"Uncompressed save is length {JSONString.Length} size: {Encoding.UTF8.GetBytes(JSONString).Length}");
        Godot.FileAccess uncompressedStringWriter = Godot.FileAccess.Open(filePath + "NOTCOMPRESSEDSTRING.txt", Godot.FileAccess.ModeFlags.WriteRead);
        uncompressedStringWriter.StorePascalString(JSONString);
        Global.Log($"Uncompressed save PascalString FILE is size: {uncompressedStringWriter.GetLength()}");
        uncompressedStringWriter.Close();


        Global.Log($"Compressing save for storage and transmission...");
        Godot.FileAccess compressedStringWriter = Godot.FileAccess.Open(filePath, Godot.FileAccess.ModeFlags.WriteRead);
        byte[] compressedStringBytes = CompressString(JSONString);
        Global.Log($"Compressed save is size: {compressedStringBytes.Length}");
        compressedStringWriter.StoreBuffer(compressedStringBytes);
        Global.Log($"Compressed save FILE is size: {compressedStringWriter.GetLength()}");
        compressedStringWriter.Close();

        Global.Log($"Reloading compressed save for debug");
        Godot.FileAccess compressedStringReader = Godot.FileAccess.Open(filePath, Godot.FileAccess.ModeFlags.Read);
        byte[] compressedBytes = compressedStringReader.GetBuffer((long)compressedStringReader.GetLength());
        Global.Log($"Reloaded compressed save is size {compressedBytes.Length}");
        string uncompressedLoadedJSONString = CompressedToString(compressedBytes);
        Global.Log($"Reloaded UNcompressed save is length {uncompressedLoadedJSONString.Length} size {Encoding.UTF8.GetBytes(uncompressedLoadedJSONString).Length}");
        compressedStringReader.Close();

        Godot.FileAccess uncompressedStringWriter2 = Godot.FileAccess.Open(filePath + "UNCOMPRESSED_STRING.txt", Godot.FileAccess.ModeFlags.WriteRead);
        uncompressedStringWriter2.StorePascalString(uncompressedLoadedJSONString);
        uncompressedStringWriter2.Close();

    }

    public void startGame(int teamNum)
    {

        Global.Log("Starting Game as team: " + teamNum);
        Layout pointyReal = new Layout(Layout.pointy, new Point(10, 10), new Point(0, 0));
        Layout pointy = new Layout(Layout.pointy, new Point(-10, 10), new Point(0, 0));
        Global.layout = pointy;
        game.localPlayerTeamNum = teamNum;


        Global.lobby.lobbyPeerStatuses[Global.clientID].IsLoaded = true;
        LobbyMessage lobbyMessage = new LobbyMessage();
        lobbyMessage.Sender = Global.clientID;
        lobbyMessage.LobbyStatus = Global.lobby.lobbyPeerStatuses[Global.clientID];
        lobbyMessage.MessageType = "loaded";

        if (isHost)
        {
            Global.Log($"Done loading. I'm the host so its time to pick Spawn Locations and communicate them.");
        }
        else
        {
            Global.Log($"Done loading. Notifying peers and waiting to get Founder spawn from Host");
        }
        Global.networkPeer.LobbyMessageAllPeersAndSelf(lobbyMessage);

        //MoveCameraToStartLocation();
    }

    internal void ResumeGame()
    {
        Global.Log("Starting Game as team: " + game.localPlayerTeamNum);


        Global.lobby.lobbyPeerStatuses[Global.clientID].IsLoaded = true;
        LobbyMessage lobbyMessage = new LobbyMessage();
        lobbyMessage.Sender = Global.clientID;
        lobbyMessage.LobbyStatus = Global.lobby.lobbyPeerStatuses[Global.clientID];
        lobbyMessage.MessageType = "loaded";
        Global.networkPeer.LobbyMessageAllPeersAndSelf(lobbyMessage);
    }

    public void HostInitGame()
    {
        if (isHost)
        {
            this.AIManager = new AIManager();
            AddChild(this.AIManager);
            SpawnPlayers();
            SpawnRuins();
            SpawnEncampments();
            AIManager.InitAI();

            LobbyMessage lobbyMessage = new();
            lobbyMessage.MessageType = "startTurns";
            lobbyMessage.Sender = Global.clientID;

            Global.networkPeer.LobbyMessageAllPeersAndSelf(lobbyMessage);


        }
    }

    public void StartTurns()
    {
        StartGameForReal();
        game.turnManager.StartNewTurn();
        graphicManager.StartNewTurn();
    }

    private void SpawnRuins()
    {
        for (int i = 0; i < Global.gameManager.game.mainGameBoard.gameHexDict.Count() / 80; i++)
        {
            Hex spawnHex = PickRandomValidHex();
            int eventIndex = new Random().Next(AncientRuinsLoader.eventStartPoints.Count);
            SpawnRuin(spawnHex, eventIndex);
        }
    }


    private void SpawnEncampments()
    {
        int teamNumCounter = Global.gameManager.game.playerDictionary.Keys.Count+1;
        int count = Global.gameManager.game.mainGameBoard.gameHexDict.Count()/200;//(int) Mathf.Floor(Global.gameManager.game.playerDictionary.Count * 1.5f);
        for (int i = 0; i < count; i ++)
        {
            Hex spawnHex = PickRandomValidHexAwayFromSpawn(4);
            int teamNum = teamNumCounter++;
            ulong playerID = (ulong)new Random().NextInt64();
            Global.gameManager.SpawnEncampment(spawnHex, FactionType.Goblins,teamNum,playerID);
        }
    }


    public void HostInitSavedGame()
    {
        if (isHost)
        {
            this.AIManager = new AIManager();
            AIManager.InitAI();

            LobbyMessage lobbyMessage = new LobbyMessage();
            lobbyMessage.Sender = Global.clientID;
            lobbyMessage.MessageType = "gameReady";
            Global.networkPeer.LobbyMessageAllPeersAndSelf(lobbyMessage);
        }
    }

    public void SpawnPlayers()
    {
        Global.Log("Spawning Players. Total: " + game.playerDictionary.Count);
        foreach (Player player in game.playerDictionary.Values)
        {
            if (player.teamNum == -99) //TODO: IMPLEMENT NON MAJOR PLAYER SPAWNING
            {

            }
            else
            {
                Global.Log($"Attempting to find good spawn location for team:{player.teamNum}");
                Hex spawnLocation = GetPlayerSpawnHex(player);
                Global.Log($"Found:{spawnLocation}, spawning a Founder unit for that team.");
                Global.gameManager.SpawnUnit("Founder", player.teamNum, spawnLocation, false, false);
            }
        }
    }

    public List<Hex> FindRecommendedSettleLocations()
    {
        Global.Log($"Searching for good spawn locations.");
        List<Hex> retval = new List<Hex>();
        Dictionary<Hex, GameHex> map = Global.gameManager.game.mainGameBoard.gameHexDict;
        foreach (Hex hex in map.Keys)
        {
            if (isGoodSettleLocation(hex))
            {
                retval.Add(hex);
            }
        }
        Global.Log($"Found {retval.Count} good spots.");
        return retval;
    }

    private bool isGoodSettleLocation(Hex hex)
    {
        bool hasFreshWater = true;
        int resourcesInRange = 0;
        int distanceToOthers = Global.gameManager.game.mainGameBoard.gameHexDict[hex].rangeToNearestCity;
        int normalTilesInRange = 0;
        List<Hex> toCheck = hex.WrappingRange(4,Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.top, Global.gameManager.game.mainGameBoard.bottom);
        foreach (Hex h in toCheck)
        {
            GameHex gHex = Global.gameManager.game.mainGameBoard.gameHexDict[h];
            if (gHex.resourceType!=ResourceType.None)
            {
                resourcesInRange++;
            }
            if (gHex.terrainType!=TerrainType.Ocean && gHex.terrainType!=TerrainType.Mountain && gHex.terrainType!=TerrainType.Coast)
            {
                normalTilesInRange++;
            }
        }
        if (resourcesInRange >= 2 && distanceToOthers<5 && normalTilesInRange>10)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private Hex GetPlayerSpawnHex(Player player)
    {
        //return PickRandomValidHex();
        return PickSmartSpawnHex();
        /*
        Random rng = new Random();
        List<Hex> list = FindRecommendedSettleLocations();
        return list[rng.Next(list.Count)];*/
    }

    public Hex PickSmartSpawnHex()
    {
        Hex spawnHex = new Hex();
        List<Hex> candidates = new List<Hex>();
        int maxRange = 0;

        foreach (Hex hex in game.mainGameBoard.gameHexDict.Keys)
        {
            GameHex gameHex = game.mainGameBoard.gameHexDict[hex];
            if ((gameHex.terrainType == TerrainType.Flat || gameHex.terrainType == TerrainType.Rough) &&
                gameHex.units.Count == 0 &&
                gameHex.district == null &&
                gameHex.resourceType == ResourceType.None)
            {
                if (gameHex.rangeToNearestSpawn > maxRange)
                {
                    candidates.Clear();
                    maxRange = gameHex.rangeToNearestSpawn;
                }

                if (gameHex.rangeToNearestSpawn == maxRange)
                {
                    candidates.Add(hex);
                }
            }
        }
        spawnHex = candidates[new Random().Next(candidates.Count)];

        foreach (Hex hex in spawnHex.WrappingRange(6, Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.top, Global.gameManager.game.mainGameBoard.bottom))
        {
            if(hex.WrapDistance(spawnHex) < Global.gameManager.game.mainGameBoard.gameHexDict[hex].rangeToNearestSpawn)
            {
                Global.gameManager.game.mainGameBoard.gameHexDict[hex].rangeToNearestSpawn = hex.WrapDistance(spawnHex);
            }
        }

        return spawnHex;
    }

    public Hex PickRandomValidHexAwayFromSpawn(int range)
    {
        List<Hex> list = new List<Hex>();
        foreach (Hex hex in game.mainGameBoard.gameHexDict.Keys)
        {
            if (game.mainGameBoard.gameHexDict[hex].rangeToNearestSpawn >= range && (game.mainGameBoard.gameHexDict[hex].terrainType == TerrainType.Flat || game.mainGameBoard.gameHexDict[hex].terrainType == TerrainType.Rough) && game.mainGameBoard.gameHexDict[hex].units.Count == 0 && game.mainGameBoard.gameHexDict[hex].district == null && game.mainGameBoard.gameHexDict[hex].resourceType == ResourceType.None)
            {
                list.Add(hex);
            }
        }

        Random rng = new Random();
        return list[rng.Next(list.Count)];
    }

    public Hex PickRandomValidHex()
    {
        List<Hex> list = new List<Hex>();
        foreach (Hex hex in game.mainGameBoard.gameHexDict.Keys)
        {
            if ((game.mainGameBoard.gameHexDict[hex].terrainType == TerrainType.Flat || game.mainGameBoard.gameHexDict[hex].terrainType == TerrainType.Rough) && game.mainGameBoard.gameHexDict[hex].units.Count == 0 && game.mainGameBoard.gameHexDict[hex].district == null && game.mainGameBoard.gameHexDict[hex].resourceType==ResourceType.None && game.mainGameBoard.gameHexDict[hex].ancientRuins == null)
            {
                list.Add(hex);
            }
        }
        
        Random rng = new Random();
        return list[rng.Next(list.Count)];
    }

    public void startDebugGame(string savePath, int teamNum)
    {
        Global.Log("Starting Game");
        Layout pointyReal = new Layout(Layout.pointy, new Point(10, 10), new Point(0, 0));
        Layout pointy = new Layout(Layout.pointy, new Point(-10, 10), new Point(0, 0));
        Global.layout = pointy;
        //game = LoadGameFromDisk(savePath);
        game.localPlayerTeamNum = teamNum;
        InitGraphics(game, Global.layout);
        Global.menuManager.ClearMenus();
        gameStarted = true;

    }

    private void startTerrainDemo()
    {
        Global.Log("Starting Game");
        Layout pointyReal = new Layout(Layout.pointy, new Point(10, 10), new Point(0, 0));
        Layout pointy = new Layout(Layout.pointy, new Point(-10, 10), new Point(0, 0));
        Global.layout = pointy;

        if (game.mainGameBoard == null)
        {
/*            game = GameTests.TestSlingerCombat();
            SaveGame("test.txt");
            game = LoadGame("test.txt");*/
        }

        InitGraphics(game, Global.layout);
        Global.menuManager.ClearMenus();
        gameStarted = true;
        Global.Log("NEWTURN: " + game.turnManager.currentTurn + "///////////////////////////////////////////NEWTURN" + game.turnManager.currentTurn + "///////////////////////////////////////////////////////NEWTURN" + game.turnManager.currentTurn + "///////////////////////////////////////////NEWTURN" + game.turnManager.currentTurn + "");

    }

    private void InitGraphics(Game game, Layout layout)
    {
        Global.Log("Initializing Graphics");
        graphicManager = new GraphicManager(layout);
        if (Global.gameManager.game.mainGameBoard != null)
        {
            graphicManager.NewGameBoard(Global.gameManager.game.mainGameBoard);
        }
        graphicManager.Name = "GraphicManager";
        AddChild(graphicManager);
    }

    public bool TryGetGraphicManager(out GraphicManager manager)
    {
        if (graphicManager != null)
        {
            manager = graphicManager;
            return true;
        }
        manager = null;
        return false;
    }


    public override void _PhysicsProcess(double delta)
    {
        if (game== null || game.turnManager == null)
        {
            return;
        }
        if (game.teamManager.relationships.ContainsKey(0) && game.localPlayerTeamNum!=0)
        {
            //game.turnManager.EndCurrentTurn(0);
        }

        if (game.teamManager.relationships.ContainsKey(2))
        {
            //game.turnManager.EndCurrentTurn(2);
        }

        if (SkipPlayerTurns!=-1)
        {
            if (TurnsToSkip > 0)
            {
                game.turnManager.EndCurrentTurn(SkipPlayerTurns);
                TurnsToSkip--;
            }
            else
            {
                SkipPlayerTurns = -1;
            }
        }

        List<int> waitingForPlayerList = game.turnManager.CheckTurnStatus();
        if (graphicManager!=null&& !waitingForPlayerList.Any())
        {
            Global.Log("All players have finished their turns. Starting a new turn");
            game.turnManager.StartNewTurn();
            graphicManager.StartNewTurn();
        }
        else
        {
            //push waitingForPlayerList to UI
        }
    }

    

    public void MoveUnit(int unitID, Hex hex, bool isEnemy, bool local = true)
    {
        
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructMoveUnitCommand(unitID, hex, isEnemy));
            return;
        }
        else
        {
            //Global.Log("Network (or loopback) move command recevied, executing.");
        }


        Unit unit = SearchUnitByID(unitID);
        if (unit == null)
        {
            Global.Log("Unit is null"); //TODO - Potential Desync
            return;
        }

        GameHex target = game.mainGameBoard.gameHexDict[hex];
        if (target == null)
        {
            Global.Log("Target hex is null");//TODO - Potential Desync
            return;
        }

        if (isEnemy != Global.gameManager.game.mainGameBoard.gameHexDict[hex].IsEnemyPresent(unit.teamNum))
        {
            Global.Log("DESYNC ALARM");
        }
        try
        {
            unit.MoveTowards(target, Global.gameManager.game.teamManager, isEnemy);
        }
        catch (Exception e)
        {
            Global.Log("Error moving unit: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    private Unit SearchUnitByID(int unitID)
    {
        if (Global.gameManager.game.unitDictionary.TryGetValue(unitID, out Unit unit))
        {
            return unit;
        }
        else
        {
            return null;
        }
    }

    public void ActivateAbility(int unitID, string AbilityName, Hex Target, bool local = true) 
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructActivateAbilityCommand(unitID, AbilityName, Target));
            return;
        }

        Unit unit = SearchUnitByID(unitID);
        if (unit == null)
        {
            Global.Log("Unit is null"); //TODO - Potential Desync
            return;
        }

        GameHex target = game.mainGameBoard.gameHexDict[Target];
        if (target == null)
        {
            Global.Log("Target hex is null"); //TODO - Potential Desync
            return;
        }
        //find the ability like normal and check hero abilities if we didnt find it
        UnitAbility ability = unit.abilities.Find(x => x.name == AbilityName);
        int level = 0;
        if (ability == null)
        {
            if (unit is Hero hero)
            {
                HeroAbility heroAbility = hero.heroAbilities.Find(x => x.ability.name == AbilityName);
                ability = heroAbility.ability;
                level = heroAbility.level;
            }
        }
        if (ability == null)
        {
            Global.Log("Ability is null"); //TODO - Potential Desync
            return;
        }

        try
        {
            ability.ActivateAbility(target, level);
            if (unit is Hero hero)
            {
                HeroAbility heroAbility = hero.heroAbilities.Find(x => x.ability.name == AbilityName);
                if (heroAbility != null)
                {
                    hero.mana -= heroAbility.manaCost[heroAbility.level];
                    heroAbility.currentCooldown = heroAbility.cooldown[heroAbility.level];
                }
            }
        }
        catch (Exception e)
        {
            Global.Log("Error activating ability: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    public void AddToProductionQueue(int cityID, string name, Hex targetHex, bool front = false, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructAddToProductionQueueCommand(cityID,name,targetHex,front));
            return;
        }

        City city = Global.gameManager.game.cityDictionary[cityID];
        if (city == null)
        {
            Global.Log("City is null"); //TODO - Potential Desync
            return;
        }

        if (front)
        {
            try
            {
                city.AddToFrontOfQueue(name, targetHex);
            }
            catch (Exception e)
            {
                Global.Log("Error changing production queue: " + e.Message); //TODO - Potential Desync
                throw;
            }
        }
        else
        {
            try
            {
                city.AddToQueue(name, targetHex);
            }
            catch (Exception e)
            {
                Global.Log("Error changing production queue: " + e.Message); //TODO - Potential Desync
                throw;
            }
        }

    }

    public void RemoveFromProductionQueue(int cityID, int index, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructRemoveFromProductionQueueCommand(cityID, index));
            return;
        }

        City city = Global.gameManager.game.cityDictionary[cityID];
        if (city == null)
        {
            Global.Log("City is null"); //TODO - Potential Desync
            return;
        }

        try
        {
            city.RemoveFromQueue(index);
        }
        catch (Exception e)
        {
            Global.Log("Error changing production queue: " + e.Message); //TODO - Potential Desync
            throw;
        }

    }

    public void MoveToFrontOfProductionQueue(int cityID, int index, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructMoveToFrontOfProductionQueueCommand(cityID, index));
            return;
        }

        City city = Global.gameManager.game.cityDictionary[cityID];
        if (city == null)
        {
            Global.Log("City is null"); //TODO - Potential Desync
            return;
        }

        try
        {
            city.MoveToFrontOfProductionQueue(index);
        }
        catch (Exception e)
        {
            Global.Log("Error moving to front of prod queue: " + e.Message); //TODO - Potential Desync
            throw;
        }

    }


    public void ExpandToHex(int cityID, Hex Target, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructExpandToHexCommand(cityID, Target));
            return;
        }

        City city = Global.gameManager.game.cityDictionary[cityID];
        if (city == null)
        {
            Global.Log("City is null"); //TODO - Potential Desync
            return;
        }

        try
        {
            if (city is Encampment e)
            {
                e.ExpandToHex(Target);
            }
            else
            {
                city.ExpandToHex(Target);
            }
        }
        catch (Exception e)
        {
            Global.Log("Error expanding to hex: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    public void DevelopDistrict(int cityID, Hex Target, DistrictType districtType, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructDevelopDistrictCommand(cityID, Target, districtType));
            return;
        }

        City city = Global.gameManager.game.cityDictionary[cityID];
        if (city == null)
        {
            Global.Log("City is null"); //TODO - Potential Desync
            return;
        }

        try
        {
            city.DevelopDistrict(Target, districtType);
        }
        catch (Exception e)
        {
            Global.Log("Error developing district: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    public void RenameCity(int cityID, string name, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructRenameCityCommand(cityID, name));
            return;
        }

        City city = Global.gameManager.game.cityDictionary[cityID];
        if (city == null)
        {
            Global.Log("City is null"); //TODO - Potential Desync
            return;
        }

        try
        {
            city.RenameCity(name);
        }
        catch (Exception e)
        {
            Global.Log("Error renaming city: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    public void SelectResearch(int teamNum, string techName, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructSelectResearchCommand(teamNum, techName));
            return;
        }

        try
        {
            Global.gameManager.game.playerDictionary[teamNum].SelectResearch(techName);
        }
        catch (Exception e)
        {
            Global.Log("Error selecting research tech: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    public void SelectCulture(int teamNum, string cultureName, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructSelectCultureCommand(teamNum, cultureName));
            return;
        }

        try
        {
            Global.gameManager.game.playerDictionary[teamNum].SelectCultureResearch(cultureName);
        }
        catch (Exception e)
        {
            Global.Log("Error selecting culture tech: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    public void AddResourceAssignment(int cityID, ResourceType resourceType, Hex sourceHex, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructAddResourceAssignmentCommand(cityID, resourceType, sourceHex));
            return;
        }

        City city = Global.gameManager.game.cityDictionary[cityID];
        if (city == null)
        {
            Global.Log("City is null"); //TODO - Potential Desync
            return;
        }

        try
        {
            Global.gameManager.game.playerDictionary[city.teamNum].AddResource(sourceHex, resourceType, city);
        }
        catch (Exception e)
        {
            Global.Log("Error adding resource assignment: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    public void RemoveResourceAssignment(int teamNum, Hex sourceHex, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructRemoveResourceAssignmentCommand(teamNum, sourceHex));
            return;
        }

        try
        {
            Global.gameManager.game.playerDictionary[teamNum].RemoveResource(sourceHex);
        }
        catch (Exception e)
        {
            Global.Log("Error adding resource assignment: " + e.Message); //TODO - Potential Desync
            throw;
        }

    }
    
    public void EndTurn(int teamNum, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructEndTurnCommand(teamNum));
            return;
        }

        try
        {
            Global.gameManager.game.turnManager.EndCurrentTurn(teamNum);
        }
        catch (Exception e)
        {
            Global.Log("Error ending turn: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    internal void ExecutePendingDeal(int dealID, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructExecutePendingDealCommand(dealID));
            return;
        }

        try
        {
            Global.gameManager.game.teamManager.ExecuteDeal(dealID);
        }
        catch (Exception e)
        {
            Global.Log("Error executing pending deal: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    internal void RemovePendingDeal(int dealID, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructRemovePendingDealCommand(dealID));
            return;
        }

        try
        {
            Global.gameManager.game.teamManager.RemoveDeal(dealID);
        }
        catch (Exception e)
        {
            Global.Log("Error removing pending deal: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    internal void AddPendingDeal(int dealID, int fromTeamNum, int toTeamNum, List<DiplomacyAction> requests, List<DiplomacyAction> offers, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructAddPendingDealCommand(dealID,fromTeamNum,toTeamNum,requests,offers));
            return;
        }

        try
        {
            Global.gameManager.game.teamManager.AddPendingDeal(new DiplomacyDeal(dealID,fromTeamNum, toTeamNum, offers, requests));
        }
        catch (Exception e)
        {
            Global.Log("Error adding pending deal: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    internal void NewExportRoute(int fromCityID, int toCityID, YieldType yieldType, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructNewExportRouteCommand(fromCityID, toCityID, yieldType));
            return;
        }

        
        try
        {
            int teamNum = Global.gameManager.game.cityDictionary[fromCityID].teamNum;
            Global.gameManager.game.playerDictionary[teamNum].NewExportRoute(fromCityID, toCityID, yieldType);
        }
        catch (Exception e)
        {
            Global.Log("Error adding export route: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    internal void RemoveExportRoute(int fromCityID, int toCityID, YieldType yieldType, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructRemoveExportRouteCommand(fromCityID, toCityID, yieldType));
            return;
        }


        try
        {
            int teamNum = Global.gameManager.game.cityDictionary[fromCityID].teamNum;
            Global.gameManager.game.playerDictionary[teamNum].RemoveExportRoute(fromCityID, toCityID, yieldType);
        }
        catch (Exception e)
        {
            Global.Log("Error remove export route: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    internal void NewTradeRoute(int fromCityID, int toCityID, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructNewTradeRouteCommand(fromCityID, toCityID));
            return;
        }


        try
        {
            int teamNum = Global.gameManager.game.cityDictionary[fromCityID].teamNum;
            Global.gameManager.game.playerDictionary[teamNum].NewTradeRoute(fromCityID, toCityID);
        }
        catch (Exception e)
        {
            Global.Log("Error adding trade route: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    internal void RemoveTradeRoute(int fromCityID, int toCityID, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructRemoveTradeRouteCommand(fromCityID, toCityID));
            return;
        }


        try
        {
            int teamNum = Global.gameManager.game.cityDictionary[fromCityID].teamNum;
            Global.gameManager.game.playerDictionary[teamNum].RemoveTradeRoute(fromCityID, toCityID);
        }
        catch (Exception e)
        {
            Global.Log("Error removing trade route: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    internal void AssignPolicyCard(int teamNum, int policyCardID, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructAssignPolicyCardCommand(teamNum, policyCardID));
            return;
        }


        try
        {
            Global.gameManager.game.playerDictionary[teamNum].AssignPolicyCard(policyCardID);
        }
        catch (Exception e)
        {
            Global.Log("Error assigning policy card: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    public void CapturedCityChoice(int cityID, int teamNum, CityConquerOptions choice, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructCapturedCityChoiceCommand(cityID,teamNum,choice));
            return;
        }


        try
        {
            Global.gameManager.game.cityDictionary[cityID].CityConquered(teamNum,choice);
        }
        catch (Exception e)
        {
            Global.Log("Error assigning a captured city choice: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    public void CapturedEncampmentChoice(int cityID, int teamNum, EncampmentConquerOptions choice, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructCapturedEncampmentChoiceCommand(cityID, teamNum, choice));
            return;
        }


        try
        {
            ((Encampment)Global.gameManager.game.cityDictionary[cityID]).EncampmentConquered(teamNum, choice);
        }
        catch (Exception e)
        {
            Global.Log("Error assigning a captured city choice: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    internal void UnassignPolicyCard(int teamNum, int policyCardID, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructUnassignPolicyCardCommand(teamNum, policyCardID));
            return;
        }


        try
        {
            Global.gameManager.game.playerDictionary[teamNum].UnassignPolicyCard(policyCardID);
        }
        catch (Exception e)
        {
            Global.Log("Error unassigning policy card: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    internal void SetGovernment(int teamNum, GovernmentType govType, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructSetGovernmentCommand(teamNum, govType));
            return;
        }


        try
        {
            Global.gameManager.game.playerDictionary[teamNum].SetGovernment(govType);
        }
        catch (Exception e)
        {
            Global.Log("Error setting government: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    internal void SpawnUnit(string unitType, int teamNum, Hex position, bool stackable, bool flexible, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructSpawnUnitCommand(unitType, teamNum, position, stackable, flexible));
            return;
        }

        try
        {
            int newID = game.playerDictionary[teamNum].GetNextUniqueID();
            Global.Log($"Got a command over network (or loopback) to spawn a unit of type {unitType} for team {teamNum} at position {position}. I assigned this unit an ID of {newID}");
            Unit newUnit = new(unitType, 0,newID, teamNum);
            GameHex location = Global.gameManager.game.mainGameBoard.gameHexDict[position];
            if (location.SpawnUnit(newUnit,stackable,flexible)!=true)
            {
                Global.Log($"Error spawning unit "); //TODO - Potential Desync
            }
        }
        catch (Exception e)
        {
            Global.Log("Error spawning unit: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    public void PurchaseUnit(string unitType, int cityID, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructPurchaseUnitCommand(unitType, cityID));
            return;
        }

        try
        {
            City city = Global.gameManager.game.cityDictionary[cityID];
            city.PurchaseUnit(unitType);
        }
        catch (Exception e)
        {
            Global.Log("Error buying unit: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    public void PurchaseBuilding(string buildingType, Hex location, int cityID, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructPurchaseBuildingCommand(buildingType, location, cityID));
            return;
        }

        try
        {
            City city = Global.gameManager.game.cityDictionary[cityID];
            city.PurchaseBuilding(location,buildingType);
        }
        catch (Exception e)
        {
            Global.Log("Error buying building: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    public void StartGameForReal()
    {
        Global.Log("Starting the game for real...");
        if (Global.lobby.saveGameLoaded)
        {
            Global.Log("So you're loading a game, time to dump some data");
            Global.Log($"We're loading into the game on turn #{Global.gameManager.game.turnManager.currentTurn}");
            Global.Log($"Lets check in on our AI turn statuses. (NUMAI:{Global.gameManager.game.numAI}) If all is well these should all be false.");
            foreach (AIUtils.AI ai in Global.gameManager.AIManager.aiList)
            {
                Global.Log($"AI#{ai.player.teamNum}: turn finished?: {ai.player.turnFinished}");
            }
        }

        InitGraphics(game, Global.layout);
        Global.menuManager.ClearMenus();
        gameStarted = true;
    }


    public void SpawnEncampment(Hex location, FactionType type, int teamNum, ulong playerID, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructSpawnEncampmentCommand(location, type,teamNum,playerID));
            return;
        }

        try
        {
            Global.Log($"Got a command over network (or loopback) to spawn an encampment at location {location}");
            Global.gameManager.game.AddPlayer(0, teamNum, type, playerID, Colors.DarkRed, true, true);
            Player player = game.playerDictionary[teamNum];
            foreach (Hex hex in location.WrappingRange(9, Global.gameManager.game.mainGameBoard.left, Global.gameManager.game.mainGameBoard.right, Global.gameManager.game.mainGameBoard.top, Global.gameManager.game.mainGameBoard.bottom))
            {
                if (hex.WrapDistance(location) < Global.gameManager.game.mainGameBoard.gameHexDict[hex].rangeToNearestSpawn)
                {
                    Global.gameManager.game.mainGameBoard.gameHexDict[hex].rangeToNearestSpawn = hex.WrapDistance(location);
                }
            }
            new Encampment(Global.gameManager.game.GetUniqueID(player.teamNum), player.teamNum, "GoblinEncampment", true, Global.gameManager.game.mainGameBoard.gameHexDict[location]);
        }
        catch (Exception e)
        {
            Global.Log("Error spawning encampment: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    public void SpawnRuin(Hex location, int eventIndex, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructSpawnRuinCommand(location, eventIndex));
            return;
        }

        try
        {
            new AncientRuins(location, AncientRuinsLoader.eventStartPoints[eventIndex].eventID);
        }
        catch (Exception e)
        {
            Global.Log("Error spawning ruin: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    public void TriggerRuin(int teamNum, Hex location, int eventIndex, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructTriggerRuinCommand(teamNum,location,eventIndex));
            return;
        }

        try
        {
            AncientRuins ancientRuins = Global.gameManager.game.mainGameBoard.gameHexDict[location].ancientRuins;
            EventOption eventOption = null;
            if (AncientRuinsLoader.ruinsEventDict[ancientRuins.nextEventID].options != null)
            {
                eventOption = AncientRuinsLoader.ruinsEventDict[ancientRuins.nextEventID].options[eventIndex];
            }
            if (eventOption.eventEffects != null)
            {
                eventOption.eventEffects.Invoke(Global.gameManager.game.playerDictionary[teamNum], ancientRuins);
            }
            //if random selection do it or just select the only result and set it
            if(eventOption.nextEvents != null && eventOption.nextEvents.Any())
            {
                RuinsEvent chosenEvent = AncientRuinsLoader.PickWeightedEvent(eventOption.nextEvents, ancientRuins);
                ancientRuins.nextEventID = chosenEvent.eventID;
            }
        }
        catch (Exception e)
        {
            Global.Log("Error triggering ruin: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    internal void SetDiplomaticState(int teamNumOne, int teamNumTwo, DiplomaticState diplomaticState, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructSetDiplomaticStateCommand(teamNumOne, teamNumTwo, diplomaticState));
            return;
        }


        try
        {
            Global.gameManager.game.teamManager.SetDiplomaticState(teamNumOne,teamNumTwo,diplomaticState);
        }
        catch (Exception e)
        {
            Global.Log("Error setting diplomatic state: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }

    public void LevelUpAbility(int heroID, string abilityName, bool local = true)
    {
        if (local)
        {
            Global.networkPeer.CommandAllPeersAndSelf(CommandParser.ConstructLevelUpAbililtyCommand(heroID,abilityName));
            return;
        }

        try
        {
            Unit unit = Global.gameManager.game.unitDictionary[heroID];
            if (unit is Hero hero)
            {
                HeroAbility heroAbility = hero.heroAbilities.Find(x => x.ability.name == abilityName);
                heroAbility.LevelUpAbility(hero);
                Global.gameManager.graphicManager.uiManager.UpdateHeroUIDisplay();
            }

        }
        catch (Exception e)
        {
            Global.Log("Error leveling up hero ability: " + e.Message); //TODO - Potential Desync
            throw;
        }
    }
}

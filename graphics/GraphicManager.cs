using Godot;
using ImGuiNET;
using NetworkMessages;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

public partial class GraphicManager : Node3D
{
    public Dictionary<int, GraphicObject> graphicObjectDictionary;
    public Dictionary<Hex, List<GraphicObject>> hexObjectDictionary;
    public Dictionary<int, GraphicObject> toBeDeleted;
    public EnviromentManager enviroment;
    public Layout layout;
    public GraphicObject selectedObject;
    public int selectedObjectID;
    public UIManager uiManager;
    private bool waitForTargeting = false;
    public HexGameCamera camera;
    public float totalTime = 0.0f;

    bool ShowDebugConsole = false;


    public GraphicManager(Layout layout)
    {
        toBeDeleted = new();
        graphicObjectDictionary = new();
        hexObjectDictionary = new();
        foreach(Hex hex in Global.gameManager.game.mainGameBoard.gameHexDict.Keys)
        {
            hexObjectDictionary.Add(hex, new List<GraphicObject>());
        }
        this.layout = layout;
        uiManager = new UIManager(layout);
        uiManager.Name = "UIManager";
        AddChild(uiManager);
        
        EnviromentManager enviromentManager = new EnviromentManager();
        AddChild(enviromentManager);
        ConfigureAndAddCamera();

    }

    public override void _Process(double delta)
    {
        totalTime += (float)delta;
        RenderingServer.GlobalShaderParameterSet("time", totalTime);

    }



    private void ConfigureAndAddCamera()
    {
        HexGameCamera camera = new HexGameCamera();
        camera.Name = "HexGameCamera";

        camera.Position = new Vector3(150, 20, -150);
        camera.RotationDegrees = new Vector3(-50, 90, 0);

        AddChild(camera);
        Global.camera = camera;
        this.camera = camera;
    }


    public void NewGameBoard(GameBoard gameBoard)
    {
        GraphicGameBoard graphicGameBoard = new GraphicGameBoard(gameBoard, layout);
        AddChild(graphicGameBoard);
        graphicObjectDictionary.Add(graphicGameBoard.gameBoard.id, graphicGameBoard);
    }

    public void NewUnit(int unitID)
    {
        Unit unit = Global.gameManager.game.unitDictionary[unitID];
        GraphicUnit graphicUnit = new GraphicUnit(unit);
        graphicObjectDictionary.Add(graphicUnit.unit.id, graphicUnit);
        CallDeferred(Node.MethodName.AddChild, graphicUnit);
    }

    public void NewDistrict(Godot.Collections.Dictionary hexData)
    {
        Hex hex = new Hex((int)hexData["q"], (int)hexData["r"], (int)hexData["s"]);
        District district = Global.gameManager.game.mainGameBoard.gameHexDict[hex].district;

        GraphicDistrict graphicDistrict = new GraphicDistrict(district, layout);
        graphicObjectDictionary.Add(graphicDistrict.district.id, graphicDistrict);
        GraphicGameBoard ggb = ((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);
        ggb.chunkList[ggb.hexToChunkDictionary[graphicDistrict.district.hex]].multiMeshInstance.AddChild(graphicDistrict);
    }

    public void NewBuilding(string buildingName, Godot.Collections.Dictionary hexData, int id, bool isDistrictCenterBuilding)
    {
        Hex hex = new Hex((int)hexData["q"], (int)hexData["r"], (int)hexData["s"]);

        GraphicBuilding graphicBuilding = new GraphicBuilding(buildingName, hex, isDistrictCenterBuilding, layout);
        graphicObjectDictionary.Add(id, graphicBuilding);
        GraphicGameBoard ggb = ((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);
        ggb.chunkList[ggb.hexToChunkDictionary[hex]].multiMeshInstance.AddChild(graphicBuilding);
    }

    public void NewResource(ResourceType resource, Hex hex)
    {
        GraphicResource graphicResource = new GraphicResource(resource, hex);
        //graphicObjectDictionary.Add(graphicResource.id, graphicResource);
        hexObjectDictionary[hex].Add(graphicResource);
        GraphicGameBoard ggb = ((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);
        ggb.chunkList[ggb.hexToChunkDictionary[hex]].multiMeshInstance.AddChild(graphicResource);
    }

    public void NewRuins(Godot.Collections.Dictionary hexData)
    {
        Hex hex = new Hex((int)hexData["q"], (int)hexData["r"], (int)hexData["s"]);
        GraphicRuins graphicRuins = new GraphicRuins(Global.gameManager.game.mainGameBoard.gameHexDict[hex].ancientRuins, hex);
        hexObjectDictionary[hex].Add(graphicRuins);
        GraphicGameBoard ggb = ((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);
        ggb.chunkList[ggb.hexToChunkDictionary[hex]].multiMeshInstance.AddChild(graphicRuins);
    }

    public void NewCity(int cityID)
    {
        City city = Global.gameManager.game.cityDictionary[cityID];
        GraphicCity graphicCity = new GraphicCity(city, layout);
        graphicObjectDictionary.Add(graphicCity.city.id, graphicCity);
        GraphicGameBoard ggb = ((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);
        ggb.chunkList[ggb.hexToChunkDictionary[graphicCity.city.hex]].multiMeshInstance.AddChild(graphicCity);
    }

    public void NewFeature(Godot.Collections.Dictionary hexData, FeatureType featureType)
    {
        Hex hex = new Hex((int)hexData["q"], (int)hexData["r"], (int)hexData["s"]);
        GraphicFeature graphicFeature = new GraphicFeature(hex, featureType);
        GraphicGameBoard ggb = ((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);
        hexObjectDictionary[hex].Add(graphicFeature);
        ggb.chunkList[ggb.hexToChunkDictionary[hex]].multiMeshInstance.AddChild(graphicFeature);
    }

    public void NewYields(Hex hex, Yields yields)
    {
        foreach(KeyValuePair<YieldType, float> yield in yields.YieldsToDict())
        {
            if(yield.Value != 0)
            {
                GraphicYield graphicYield = new GraphicYield(hex, yield.Key, yield.Value);
                GraphicGameBoard ggb = ((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);
                hexObjectDictionary[hex].Add(graphicYield);
                ggb.chunkList[ggb.hexToChunkDictionary[hex]].multiMeshInstance.AddChild(graphicYield);
            }
        }
    }

    public void UpdateGraphic(int id, GraphicUpdateType graphicUpdateType)
    {
        if (graphicObjectDictionary.ContainsKey(id))
        {
            graphicObjectDictionary[id].UpdateGraphic(graphicUpdateType);
        }
        else
        {
            throw new Exception("No Graphic Object Associated With ID of: " + id);
        }
    }

    public void UpdateHexObjectDictionary(Godot.Collections.Dictionary previousHexData, int id, Godot.Collections.Dictionary newHexData)
    {
        GraphicObject graphicObj = graphicObjectDictionary[id];
        Hex previousHex = new Hex((int)previousHexData["q"], (int)previousHexData["r"], (int)previousHexData["s"]);
        Hex newHex = new Hex((int)newHexData["q"], (int)newHexData["r"], (int)newHexData["s"]);
        hexObjectDictionary[previousHex].Remove(graphicObj);
        graphicObj.previousHex = newHex;
        hexObjectDictionary[newHex].Add(graphicObj);
    }

    public void Update2DUI(UIElement element)
    {
        uiManager.Update(element);
    }

    public void UpdateHex(Godot.Collections.Dictionary hexData)
    {
        Hex hex = new Hex((int)hexData["q"], (int)hexData["r"], (int)hexData["s"]);
        foreach (GraphicObject graphicObj in hexObjectDictionary[hex])
        {
            if(IsInstanceValid(graphicObj))
            {
                graphicObj.UpdateGraphic(GraphicUpdateType.Visibility);
            }
        }
        GraphicGameBoard ggb = ((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);
        ggb.UpdateYield(hex);
    }

    public void UpdateVisibility()
    {
        foreach(Hex hex in Global.gameManager.game.localPlayerRef.visibilityChangedList)
        {
            foreach(GraphicObject graphicObj in hexObjectDictionary[hex])
            {
                graphicObj.UpdateGraphic(GraphicUpdateType.Visibility);
            }
        }
        Global.gameManager.game.localPlayerRef.visibilityChangedList.Clear();
        foreach(Player player in Global.gameManager.game.playerDictionary.Values)
        {
            player.UpdateTerritoryGraphic();
        }
    }

    public void StartNewTurn()
    {
        graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id].UpdateGraphic(GraphicUpdateType.Update);
        if (selectedObject != null)
        {
            selectedObject.Unselected();
            selectedObject = null;
        }
        foreach(int id in toBeDeleted.Keys)
        {
            if(graphicObjectDictionary[id] != null)
            {
                graphicObjectDictionary[id].QueueFree();
            }
            graphicObjectDictionary.Remove(id);
        }
        toBeDeleted.Clear();
    }

    public void ChangeSelectedObject(int newID, GraphicObject newSelectedObject)
    {
        if(selectedObject != null)
        {
            ClearWaitForTarget();
            selectedObject.Unselected();
        }
        selectedObject = newSelectedObject;
        selectedObjectID = newID;
        selectedObject.Selected();

    }

    public void UnselectObject()
    {
        if (selectedObject != null)
        {
            selectedObject.Unselected();
        }
        selectedObject = null;
        selectedObjectID = 0;
    }

    public void SetWaitForTargeting(bool waitForTargeting)
    {
        //if we were waiting and now arent, update UI stuff
        if (this.waitForTargeting && !waitForTargeting && selectedObject != null)
        {
            selectedObject.RemoveTargetingPrompt();
        }
        this.waitForTargeting = waitForTargeting;
    }

    public bool GetWaitForTargeting()
    {
        return waitForTargeting;
    }

    public void ClearWaitForTarget()
    {
        SetWaitForTargeting(false);
    }

    public MeshInstance3D GenerateHexSelectionLines(List<Hex> hexes, Godot.Color color, string name)
    {
        MeshInstance3D lines = new MeshInstance3D();

        SurfaceTool st = new SurfaceTool();

        st.Begin(Mesh.PrimitiveType.Lines);
        st.SetColor(color);


        foreach (Hex hex in hexes)
        {
            List<Point> points = layout.PolygonCorners(hex);
            st.AddVertex(new Vector3((float)points[0].y, 0.1f, (float)points[0].x));
            foreach (Point point in points)
            {
                Vector3 temp = new Vector3((float)point.y, 0.1f, (float)point.x);
                st.AddVertex(temp);
                st.AddVertex(temp);

            }
            st.AddVertex(new Vector3((float)points[0].y, 0.1f, (float)points[0].x));
        }
        //st.GenerateNormals();
        lines.Mesh = st.Commit();
        lines.Name = "TargetingLines" + name;
        return lines;
    }

    public MeshInstance3D GenerateHexSelectionTriangles(List<Hex> hexes, Godot.Color color, string name)
    {
        MeshInstance3D triangles = new MeshInstance3D();

        SurfaceTool st = new SurfaceTool();
        st.Begin(Mesh.PrimitiveType.Triangles);
        st.SetColor(color);


        foreach (Hex hex in hexes)
        {
            GraphicGameBoard ggb = ((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);
            int newQ = (Global.gameManager.game.mainGameBoard.left + (hex.r >> 1) + hex.q) % ggb.chunkSize - (hex.r >> 1);
            Hex modHex = new Hex(newQ, hex.r, -newQ - hex.r); 
            Hex graphicHex = ggb.HexToGraphicHex(hex);
            List<Point> points = layout.PolygonCorners(hex);
            Vector3 origin = new Vector3((float)points[0].y, 1.15f, (float)points[0].x);
            for (int i = 1; i < 6; i++)
            {
                st.AddVertex(origin); // Add the origin point as the first vertex for the triangle fan

                Vector3 pointTwo = new Vector3((float)points[i].y, 1.15f, (float)points[i].x); // Get the next point in the polygon
                st.AddVertex(pointTwo); // Add the next point in the polygon as the second vertex for the triangle fan

                Vector3 pointThree = new Vector3((float)points[i - 1].y, 1.15f, (float)points[i - 1].x);
                st.AddVertex(pointThree); // Add the next point in the polygon as the third vertex for the triangle fan
            }
        }
        st.GenerateNormals();

        triangles.Mesh = st.Commit();
        StandardMaterial3D material = new StandardMaterial3D();
        material.VertexColorUseAsAlbedo = true;
        if (triangles.GetSurfaceOverrideMaterialCount() != 0)
        {
            triangles.SetSurfaceOverrideMaterial(0, material);
            triangles.Name = "TargetHexes"+name;
            return triangles;
        }
        else
        {
            return null;
        }
    }

    public MeshInstance3D GenerateSingleHexSelectionTriangles(Hex hex, Godot.Color color, string name)
    {
        MeshInstance3D triangles = new MeshInstance3D();

        SurfaceTool st = new SurfaceTool();
        st.Begin(Mesh.PrimitiveType.Triangles);
        st.SetColor(color);
        GraphicGameBoard ggb = ((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);
        //int newQ = (Global.gameManager.game.mainGameBoard.left + (hex.r >> 1) + hex.q) % ggb.chunkSize - (hex.r >> 1);
        //Hex modHex = new Hex(newQ, hex.r, -newQ - hex.r);
        //Hex graphicHex = ggb.HexToGraphicHex(hex);
        List<Point> points = layout.PolygonCorners(hex);
        Vector3 origin = new Vector3((float)points[0].y, 1.15f, (float)points[0].x);
        for (int i = 1; i < 6; i++)
        {
            st.AddVertex(origin); // Add the origin point as the first vertex for the triangle fan

            Vector3 pointTwo = new Vector3((float)points[i].y, 1.15f, (float)points[i].x); // Get the next point in the polygon
            st.AddVertex(pointTwo); // Add the next point in the polygon as the second vertex for the triangle fan

            Vector3 pointThree = new Vector3((float)points[i - 1].y, 1.15f, (float)points[i - 1].x);
            st.AddVertex(pointThree); // Add the next point in the polygon as the third vertex for the triangle fan
        }
        st.GenerateNormals();

        triangles.Mesh = st.Commit();
        StandardMaterial3D material = new StandardMaterial3D();
        material.VertexColorUseAsAlbedo = true;
        if (triangles.GetSurfaceOverrideMaterialCount() != 0)
        {
            triangles.SetSurfaceOverrideMaterial(0, material);
            triangles.Name = "TargetHexes" + name;
            return triangles;
        }
        else
        {
            return null;
        }
    }

    public void ShowAllWorldUI()
    {
        foreach (GraphicObject tempObject in graphicObjectDictionary.Values)
        {
            if (tempObject is GraphicCity)
            {
                ((GraphicCity)tempObject).SetWorldUIVisibility(true);
            }
            if (tempObject is GraphicUnit)
            {
                ((GraphicUnit)tempObject).SetWorldUIVisibility(true);
            }
        }
    }

    public void HideAllWorldUI()
    {
        foreach (GraphicObject tempObject in graphicObjectDictionary.Values)
        {
            if (tempObject is GraphicCity)
            {
                ((GraphicCity)tempObject).SetWorldUIVisibility(false);
            }
            if (tempObject is GraphicUnit)
            {
                ((GraphicUnit)tempObject).SetWorldUIVisibility(false);
            }
        }
    }

    public void HideAllWorldUIBut(int id)
    {
        foreach(GraphicObject tempObject in graphicObjectDictionary.Values)
        {
            if(tempObject is GraphicCity)
            {
                if(((GraphicCity)tempObject).city.id != id)
                {
                    ((GraphicCity)tempObject).SetWorldUIVisibility(false);
                }
            }
            if(tempObject is GraphicUnit)
            {
                if (((GraphicUnit)tempObject).unit.id != id)
                {
                    ((GraphicUnit)tempObject).SetWorldUIVisibility(false);
                }
            }
        }
    }

    public void HideAllCityWorldUI()
    {
        foreach (GraphicObject tempObject in graphicObjectDictionary.Values)
        {
            if (tempObject is GraphicCity)
            {
                ((GraphicCity)tempObject).SetWorldUIVisibility(false);
            }
        }
    }
}

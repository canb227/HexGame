using Godot;
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


    private void ConfigureAndAddCamera()
    {
        HexGameCamera camera = new HexGameCamera();
        camera.Name = "HexGameCamera";

        camera.Position = new Vector3(0, 20, 0);
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

    public void NewUnit(Unit unit)
    {
        GraphicUnit graphicUnit = new GraphicUnit(unit);
        graphicObjectDictionary.Add(graphicUnit.unit.id, graphicUnit);
        AddChild(graphicUnit);
    }

    public void NewDistrict(District district)
    {
        GraphicDistrict graphicDistrict = new GraphicDistrict(district, layout);
        graphicObjectDictionary.Add(graphicDistrict.district.id, graphicDistrict);
        AddChild(graphicDistrict);
    }

    public void NewBuilding(Building building)
    {
        GraphicBuilding graphicBuilding = new GraphicBuilding(building, layout);
        graphicObjectDictionary.Add(graphicBuilding.building.id, graphicBuilding);
        AddChild(graphicBuilding);
    }

    public void NewCity(City city)
    {
        GraphicCity graphicCity = new GraphicCity(city, layout);
        graphicObjectDictionary.Add(graphicCity.city.id, graphicCity);
        AddChild(graphicCity);
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

    public void UpdateHexObjectDictionary(Hex previousHex, GraphicObject graphicObj, Hex newHex)
    {
        hexObjectDictionary[previousHex].Remove(graphicObj);
        graphicObj.previousHex = newHex;
        hexObjectDictionary[newHex].Add(graphicObj);
    }

    public void Update2DUI(UIElement element)
    {
        uiManager.Update(element);
    }

    public void UpdateVisibility()
    {
        foreach(Hex hex in Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].visibilityChangedList)
        {
            foreach(GraphicObject graphicObj in hexObjectDictionary[hex])
            {
                graphicObj.UpdateGraphic(GraphicUpdateType.Visibility);
            }
        }
        Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].visibilityChangedList.Clear();
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
            List<Point> points = layout.PolygonCorners(hex);

            Vector3 origin = new Vector3((float)points[0].y, 0.15f, (float)points[0].x);
            for (int i = 1; i < 6; i++)
            {
                st.AddVertex(origin); // Add the origin point as the first vertex for the triangle fan

                Vector3 pointTwo = new Vector3((float)points[i].y, 0.15f, (float)points[i].x); // Get the next point in the polygon
                st.AddVertex(pointTwo); // Add the next point in the polygon as the second vertex for the triangle fan

                Vector3 pointThree = new Vector3((float)points[i - 1].y, 0.15f, (float)points[i - 1].x);
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

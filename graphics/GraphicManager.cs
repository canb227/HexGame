﻿using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public partial class GraphicManager : Node3D
{
    public Dictionary<int, GraphicObject> graphicObjectDictionary;
    public Game game;
    public Layout layout;
    public GraphicObject selectedObject;
    public int selectedObjectID;
    public UIManager uiManager;
    private bool waitForTargeting = false;
    public UnitAbility waitingAbility;
    public City waitingCity;
    public String waitingBuildingName;

    public GraphicManager(Game game, Layout layout)
    {
        graphicObjectDictionary = new();
        this.game = game;
        this.layout = layout;
        uiManager = new UIManager(this, game, layout);
        uiManager.Name = "UIManager";
        AddChild(uiManager);
        game.graphicManager = this;
        if (game.mainGameBoard != null)
        {
            NewGameBoard(game.mainGameBoard);
        }
    }

    public void NewGameBoard(GameBoard gameBoard)
    {
        GraphicGameBoard graphicGameBoard = new GraphicGameBoard(gameBoard, this, layout);
        AddChild(graphicGameBoard);
        graphicObjectDictionary.Add(graphicGameBoard.gameBoard.id, graphicGameBoard);
    }

    public void NewUnit(Unit unit)
    {
        GraphicUnit graphicUnit = new GraphicUnit(unit, this);
        graphicObjectDictionary.Add(graphicUnit.unit.id, graphicUnit);
        AddChild(graphicUnit);
    }

    public void NewDistrict(District district)
    {
        GraphicDistrict graphicDistrict = new GraphicDistrict(district, layout, this);
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
        GraphicCity graphicCity = new GraphicCity(city, layout, this);
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

    public void Update2DUI(UIElement element)
    {
        uiManager.Update(element);
    }

    public void StartNewTurn()
    {
        if(selectedObject != null)
        {
            selectedObject.Unselected();
            selectedObject = null;
        }
    }

    public void ChangeSelectedObject(int newID, GraphicObject newSelectedObject)
    {
        if(selectedObject != null)
        {
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
        if (this.waitForTargeting && !waitForTargeting)
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
        waitingAbility = null;
        waitingCity = null;
        waitingBuildingName = null;
        SetWaitForTargeting(false);
    }

    public MeshInstance3D GenerateHexSelectionLines(List<Hex> hexes, Godot.Color color)
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
                //GD.Print(temp);
                st.AddVertex(temp);
                st.AddVertex(temp);

            }
            st.AddVertex(new Vector3((float)points[0].y, 0.1f, (float)points[0].x));
        }
        //st.GenerateNormals();
        lines.Mesh = st.Commit();
        lines.Name = "TargetingLines";
        return lines;
    }

    public MeshInstance3D GenerateHexSelectionTriangles(List<Hex> hexes, Godot.Color color)
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
            triangles.Name = "TargetHexes";
            return triangles;
        }
        else
        {
            return null;
        }
    }
}

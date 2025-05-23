using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public partial class GraphicCity : GraphicObject
{
    public City city;
    public Layout layout;
    public List<GraphicBuilding> graphicBuildings;
    private GraphicManager graphicManager;
    public CityWorldUI cityWorldUI;
    public GraphicCity(City city, Layout layout, GraphicManager graphicManager)
    {
        this.city = city;
        this.layout = layout;
        this.graphicManager = graphicManager;
        InitCity(city);
    }

    public override void UpdateGraphic(GraphicUpdateType graphicUpdateType)
    {
        if (graphicUpdateType == GraphicUpdateType.Remove)
        {
            QueueFree();
        }
        if(graphicUpdateType == GraphicUpdateType.Update)
        {
            cityWorldUI.Update();
        }
    }

    private void InitCity(City city)
    {
        cityWorldUI = new CityWorldUI(graphicManager, city);
        AddChild(cityWorldUI);
    }

    public override void Unselected()
    {
        graphicManager.uiManager.cityInfoPanel.CityUnselected(city);
    }

    public override void Selected()
    {
        graphicManager.uiManager.cityInfoPanel.CitySelected(city);
    }

    public override void ProcessRightClick(Hex hex)
    {
        GD.PushWarning("NOT IMPLEMENTED");
    }

    public override void RemoveTargetingPrompt()
    {
        foreach (Node3D child in GetChildren())
        {
            GD.Print(child.Name);
            if (child.Name == "TargetingLines")
            {
                child.Free();
            }
            else if (child.Name == "TargetHexes")
            {
                child.Free();
            }
        }
    }

    public void GenerateBuildingTargetingPrompt(String buildingName)
    {
        BuildingInfo buildingInfo = BuildingLoader.buildingsDict[buildingName];
        List<Hex> hexes = city.ValidUrbanBuildHexes(buildingInfo.TerrainTypes);
        if (hexes.Count > 0)
        {
            graphicManager.SetWaitForTargeting(true);
            graphicManager.waitingCity = city;
            graphicManager.waitingBuildingName = buildingName;
            AddChild(graphicManager.GenerateHexSelectionLines(hexes, Godot.Colors.Gold));
            AddChild(graphicManager.GenerateHexSelectionTriangles(hexes, Godot.Colors.DarkGreen));
        }
    }

    public override void _Process(double delta)
    {

    }
}
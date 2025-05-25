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
    public String waitingBuildingName;
    public bool waitingToGrow;
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
        graphicManager.ShowAllWorldUI();
        graphicManager.uiManager.cityInfoPanel.CityUnselected();
    }

    public override void Selected()
    {
        graphicManager.HideAllWorldUIBut(city.id);
        graphicManager.uiManager.cityInfoPanel.CitySelected(city);
    }

    public override void ProcessRightClick(Hex hex)
    {
        GD.PushWarning("NOT IMPLEMENTED");
    }

    public override void RemoveTargetingPrompt()
    {
        graphicManager.uiManager.cityInfoPanel.CitySelected(city);
        graphicManager.uiManager.ShowGenericUIAfterTargeting();
        graphicManager.ShowAllWorldUI();
        foreach (Node3D child in GetChildren())
        {
            if (child.Name.ToString().Contains("TargetingLines") || child.Name.ToString().Contains("TargetHexes"))
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
            waitingBuildingName = buildingName;
            graphicManager.uiManager.cityInfoPanel.HideCityInfoPanel();
            graphicManager.HideAllWorldUIBut(city.id);
            graphicManager.uiManager.HideGenericUIForTargeting();
            AddChild(graphicManager.GenerateHexSelectionLines(hexes, Godot.Colors.Gold, "Building"));
            AddChild(graphicManager.GenerateHexSelectionTriangles(hexes, Godot.Colors.DarkGreen, "Building"));
        }
    }

    public void GenerateGrowthTargetingPrompt()
    {
        List<Hex> hexes = city.ValidExpandHexes(new List<TerrainType>());
        List<Hex> urbanhexes = city.ValidUrbanExpandHexes(new List<TerrainType>());
        //rural hexes
        if(hexes.Count > 0 || urbanhexes.Count > 0)
        {
            graphicManager.ChangeSelectedObject(city.id, this);
            graphicManager.SetWaitForTargeting(true);

            waitingToGrow = true;
            graphicManager.uiManager.cityInfoPanel.HideCityInfoPanel();
            graphicManager.HideAllWorldUIBut(city.id);
            graphicManager.uiManager.HideGenericUIForTargeting();
        }
        if (hexes.Count > 0)
        { 
            AddChild(graphicManager.GenerateHexSelectionLines(hexes, Godot.Colors.Gold, "RuralGrow"));
            AddChild(graphicManager.GenerateHexSelectionTriangles(hexes, Godot.Colors.DarkGreen, "RuralGrow"));
        }
        //urban expand hexes
        if (urbanhexes.Count > 0)
        {
            AddChild(graphicManager.GenerateHexSelectionLines(urbanhexes, Godot.Colors.Gold, "UrbanGrow"));
            AddChild(graphicManager.GenerateHexSelectionTriangles(urbanhexes, Godot.Colors.Orange, "UrbanGrow"));
        }
    }

    public void SetWorldUIVisibility(bool visible)
    {
        cityWorldUI.Visible = visible;
    }

    public override void _Process(double delta)
    {

    }
}
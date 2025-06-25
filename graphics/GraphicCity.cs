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
    public CityWorldUI cityWorldUI;
    public String waitingBuildingName;
    public bool waitingToGrow;
    public GraphicCity(City city, Layout layout)
    {
        this.city = city;
        this.layout = layout;
        //GD.Print(city.teamNum);
        InitCity(city);
        UpdateGraphic(GraphicUpdateType.Visibility);
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
        else if (graphicUpdateType == GraphicUpdateType.Visibility)
        {
            if (Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].visibleGameHexDict.ContainsKey(city.hex) || Global.gameManager.game.playerDictionary[Global.gameManager.game.localPlayerTeamNum].seenGameHexDict.ContainsKey(city.hex))
            {
                this.Visible = true;
                cityWorldUI.Visible = true;
            }
            else
            {
                this.Visible = false;
                cityWorldUI.Visible = false;
            }
        }
    }

    private void InitCity(City city)
    {
        cityWorldUI = new CityWorldUI(city);
        Global.gameManager.graphicManager.hexObjectDictionary[city.hex].Add(this);
        AddChild(cityWorldUI);
    }

    public override void Unselected()
    {
        Global.gameManager.graphicManager.ShowAllWorldUI();
        Global.gameManager.graphicManager.uiManager.cityInfoPanel.CityUnselected();
    }

    public override void Selected()
    {
        Global.gameManager.graphicManager.HideAllWorldUIBut(city.id);
        Global.gameManager.graphicManager.uiManager.cityInfoPanel.CitySelected(city);
    }

    public override void ProcessRightClick(Hex hex)
    {
        GD.PushWarning("NOT IMPLEMENTED");
    }

    public override void RemoveTargetingPrompt()
    {
        Global.gameManager.graphicManager.uiManager.cityInfoPanel.CitySelected(city);
        //Global.gameManager.graphicManager.uiManager.ShowGenericUIAfterTargeting();
        Global.gameManager.graphicManager.uiManager.endTurnButton.Visible = true;
        Global.gameManager.graphicManager.ShowAllWorldUI();
        foreach (Node child in Global.gameManager.graphicManager.GetChildren())
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
        List<Hex> hexes = city.ValidUrbanBuildHexes(buildingInfo.TerrainTypes, BuildingLoader.buildingsDict[buildingName].DistrictType);
        if (hexes.Count > 0)
        {
            Global.gameManager.graphicManager.SetWaitForTargeting(true);
            waitingBuildingName = buildingName;
            Global.gameManager.graphicManager.uiManager.cityInfoPanel.HideCityInfoPanel();
            Global.gameManager.graphicManager.HideAllWorldUIBut(city.id);
            Global.gameManager.graphicManager.uiManager.HideGenericUIForTargeting();
            GraphicGameBoard ggb = ((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);
            //ggb.chunkList[ggb.hexToChunkDictionary[city.hex]].multiMeshInstance.AddChild(Global.gameManager.graphicManager.GenerateHexSelectionLines(hexes, Godot.Colors.Gold, "Building"));
            //ggb.chunkList[ggb.hexToChunkDictionary[city.hex]].multiMeshInstance.AddChild(Global.gameManager.graphicManager.GenerateHexSelectionTriangles(hexes, Godot.Colors.DarkGreen, "Building"));
            Global.gameManager.graphicManager.AddChild(Global.gameManager.graphicManager.GenerateHexSelectionLines(hexes, Godot.Colors.Gold, "Building"));
            Global.gameManager.graphicManager.AddChild(Global.gameManager.graphicManager.GenerateHexSelectionTriangles(hexes, Godot.Colors.DarkGreen, "Building"));
        }
    }

    public void GenerateGrowthTargetingPrompt()
    {
        GraphicGameBoard ggb = ((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);

        List<Hex> hexes = city.ValidExpandHexes(new List<TerrainType> { TerrainType.Flat, TerrainType.Rough, TerrainType.Coast });
        List<Hex> urbanhexes = city.ValidUrbanExpandHexes(new List<TerrainType> { TerrainType.Flat, TerrainType.Rough, TerrainType.Coast });
        //rural hexes
        if(hexes.Count > 0 || urbanhexes.Count > 0)
        {
            Global.gameManager.graphicManager.ChangeSelectedObject(city.id, this);
            Global.gameManager.graphicManager.SetWaitForTargeting(true);

            waitingToGrow = true;
            Global.gameManager.graphicManager.uiManager.cityInfoPanel.HideCityInfoPanel();
            Global.gameManager.graphicManager.HideAllWorldUIBut(city.id);
            Global.gameManager.graphicManager.uiManager.HideGenericUIForTargeting();
        }
        if (hexes.Count > 0)
        {
            Global.gameManager.graphicManager.AddChild(Global.gameManager.graphicManager.GenerateHexSelectionLines(hexes, Godot.Colors.Gold, "RuralGrow"));
            Global.gameManager.graphicManager.AddChild(Global.gameManager.graphicManager.GenerateHexSelectionTriangles(hexes, Godot.Colors.DarkGreen, "RuralGrow"));
        }
        //urban expand hexes
        if (urbanhexes.Count > 0)
        {
            Global.gameManager.graphicManager.AddChild(Global.gameManager.graphicManager.GenerateHexSelectionLines(urbanhexes, Godot.Colors.Gold, "UrbanGrow"));
            Global.gameManager.graphicManager.AddChild(Global.gameManager.graphicManager.GenerateHexSelectionTriangles(urbanhexes, Godot.Colors.Orange, "UrbanGrow"));
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
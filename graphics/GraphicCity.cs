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
            if (Global.gameManager.game.localPlayerRef.visibleGameHexDict.ContainsKey(city.hex) || Global.gameManager.game.localPlayerRef.seenGameHexDict.ContainsKey(city.hex))
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
        if (Global.gameManager.graphicManager.uiManager.waitingOnLocalPlayer)
        {
            Global.gameManager.graphicManager.uiManager.waitingOnYouPanel.Visible = true;
        }
        Global.gameManager.graphicManager.ShowAllWorldUI();
        GraphicGameBoard ggb = ((GraphicGameBoard)Global.gameManager.graphicManager.graphicObjectDictionary[Global.gameManager.game.mainGameBoard.id]);
        foreach (HexChunk hexChunk in ggb.chunkList)
        {
            foreach (Node child in hexChunk.multiMeshInstance.GetChildren())
            {
                if (child.Name.ToString().Contains("TargetingLines") || child.Name.ToString().Contains("TargetHexes"))
                {
                    child.QueueFree();
                }
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
            foreach (Hex hex in hexes)
            {
                Hex originHex = ggb.chunkList[ggb.hexToChunkDictionary[hex]].origin;
                Hex adjustedHex = new Hex(hex.q - originHex.q, hex.r - originHex.r, -(hex.q - originHex.q) - (hex.r - originHex.r));
                ggb.chunkList[ggb.hexToChunkDictionary[hex]].multiMeshInstance.AddChild(Global.gameManager.graphicManager.GenerateSingleHexSelectionTriangles(adjustedHex, Godot.Colors.DarkGreen, "Building" + hex));
            }
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
            foreach (Hex hex in hexes)
            {
                Hex originHex = ggb.chunkList[ggb.hexToChunkDictionary[hex]].origin;
                Hex adjustedHex = new Hex(hex.q - originHex.q, hex.r - originHex.r, -(hex.q - originHex.q) - (hex.r - originHex.r));
                ggb.chunkList[ggb.hexToChunkDictionary[hex]].multiMeshInstance.AddChild(Global.gameManager.graphicManager.GenerateSingleHexSelectionTriangles(adjustedHex, Godot.Colors.DarkGreen, "RuralGrow" + adjustedHex));
            }
        }
        //urban expand hexes
        if (urbanhexes.Count > 0)
        {
            foreach(Hex hex in urbanhexes)
            {
                Hex originHex = ggb.chunkList[ggb.hexToChunkDictionary[hex]].origin;
                Hex adjustedHex = new Hex(hex.q - originHex.q, hex.r - originHex.r, -(hex.q - originHex.q) - (hex.r - originHex.r));
                ggb.chunkList[ggb.hexToChunkDictionary[hex]].multiMeshInstance.AddChild(Global.gameManager.graphicManager.GenerateSingleHexSelectionTriangles(adjustedHex, Godot.Colors.Orange, "UrbanGrow"+ adjustedHex));
            }
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
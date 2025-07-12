using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.AccessControl;

public partial class DistrictPickerPanel : Control
{
    public Control control;
    private VBoxContainer districtTypeVBox;
    private Button closeButton;
    GraphicCity targetGraphicCity;
    Hex targetHex;


    public DistrictPickerPanel(Hex targetHex, GraphicCity targetGraphicCity)
    {
        this.targetHex = targetHex;
        this.targetGraphicCity = targetGraphicCity;
        control = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/PickDistrictPanel.tscn").Instantiate<Control>();
        AddChild(control);

        districtTypeVBox = control.GetNode<VBoxContainer>("PickDistrictPanel/DistrictTypeVBox");
        closeButton = control.GetNode<Button>("CloseButton");

        closeButton.Pressed += () => CancelSelection();

        foreach (DistrictType type in Global.gameManager.game.playerDictionary[targetGraphicCity.city.teamNum].allowedDistricts)
        {
            if (type == DistrictType.refinement && BuildingLoader.buildingsDict["RefineryDistrict"].TerrainTypes.Contains(Global.gameManager.game.mainGameBoard.gameHexDict[targetHex].terrainType) )
            {
                Button button = new Button();
                button.Text = "Refining District";
                button.Pressed += () => PrepareToBuildOnHex(DistrictType.refinement);;
                districtTypeVBox.AddChild(button);
            }
            else if (type == DistrictType.production && BuildingLoader.buildingsDict["IndustryDistrict"].TerrainTypes.Contains(Global.gameManager.game.mainGameBoard.gameHexDict[targetHex].terrainType) )
            {
                Button button = new Button();
                button.Text = "Industrial District";
                button.Pressed += () => PrepareToBuildOnHex(DistrictType.production);
                districtTypeVBox.AddChild(button);
            }
            else if (type == DistrictType.gold && BuildingLoader.buildingsDict["CommerceDistrict"].TerrainTypes.Contains(Global.gameManager.game.mainGameBoard.gameHexDict[targetHex].terrainType))
            {
                Button button = new Button();
                button.Text = "Commercial District";
                button.Pressed += () => PrepareToBuildOnHex(DistrictType.gold);
                districtTypeVBox.AddChild(button);
            }
            else if (type == DistrictType.science && BuildingLoader.buildingsDict["CampusDistrict"].TerrainTypes.Contains(Global.gameManager.game.mainGameBoard.gameHexDict[targetHex].terrainType))
            {
                Button button = new Button();
                button.Text = "Campus District";
                button.Pressed += () => PrepareToBuildOnHex(DistrictType.science);
                districtTypeVBox.AddChild(button);
            }
            else if (type == DistrictType.culture && BuildingLoader.buildingsDict["CulturalDistrict"].TerrainTypes.Contains(Global.gameManager.game.mainGameBoard.gameHexDict[targetHex].terrainType))
            {
                Button button = new Button();
                button.Text = "Cultural District";
                button.Pressed += () => PrepareToBuildOnHex(DistrictType.culture);
                districtTypeVBox.AddChild(button);
            }
            else if (type == DistrictType.happiness && BuildingLoader.buildingsDict["EntertainmentDistrict"].TerrainTypes.Contains(Global.gameManager.game.mainGameBoard.gameHexDict[targetHex].terrainType))
            {
                Button button = new Button();
                button.Text = "Entertainment District";
                button.Pressed += () => PrepareToBuildOnHex(DistrictType.happiness);
                districtTypeVBox.AddChild(button);
            }
            else if (type == DistrictType.influence && BuildingLoader.buildingsDict["AdministrativeDistrict"].TerrainTypes.Contains(Global.gameManager.game.mainGameBoard.gameHexDict[targetHex].terrainType))
            {
                Button button = new Button();
                button.Text = "Administrative District";
                button.Pressed += () => PrepareToBuildOnHex(DistrictType.influence);
                districtTypeVBox.AddChild(button);
            }
            else if (type == DistrictType.dock && BuildingLoader.buildingsDict["HarborDistrict"].TerrainTypes.Contains(Global.gameManager.game.mainGameBoard.gameHexDict[targetHex].terrainType))
            {
                Button button = new Button();
                button.Text = "Harbor District";
                button.Pressed += () => PrepareToBuildOnHex(DistrictType.dock);
                districtTypeVBox.AddChild(button);
            }
            else if (type == DistrictType.military && BuildingLoader.buildingsDict["MilitaristicDistrict"].TerrainTypes.Contains(Global.gameManager.game.mainGameBoard.gameHexDict[targetHex].terrainType))
            {
                Button button = new Button();
                button.Text = "Militaristic District";
                button.Pressed += () => PrepareToBuildOnHex(DistrictType.military);
                districtTypeVBox.AddChild(button);
            }
        }

    }

    private void PrepareToBuildOnHex(DistrictType districtType)
    {
        Global.gameManager.DevelopDistrict(targetGraphicCity.city.id, targetHex, districtType);//networked command
        targetGraphicCity.waitingToGrow = false;
        Global.gameManager.graphicManager.Update2DUI(UIElement.endTurnButton);
        Global.gameManager.graphicManager.ClearWaitForTarget();
        this.QueueFree();
    }

    public void CancelSelection()
    {
        this.Visible = false;
    }


    public void Update(UIElement element)
    {
    }
}

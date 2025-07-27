using Godot;
using NetworkMessages;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.AccessControl;

public partial class CityTakenPopUp : Control
{
    public Control cityTakenPopUp;
    private Button KeepButton;
    private Button RazeButton;

    private City takenCity;
    private int takerTeamNum;

    public CityTakenPopUp()
    {
        cityTakenPopUp = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/CityTakenPopup.tscn").Instantiate<Control>();
        AddChild(cityTakenPopUp);

        KeepButton = cityTakenPopUp.GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/OccupyButton");
        RazeButton = cityTakenPopUp.GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/VassalizeButton");

        KeepButton.Pressed += () => KeepPressed();
        RazeButton.Pressed += () => RazePressed();
    }

    private void KeepPressed()
    {
        //need networked statement
        takenCity.ChangeTeam(Global.gameManager.game.unitDictionary[Global.gameManager.game.mainGameBoard.gameHexDict[takenCity.hex].units[0]].teamNum);
    }

    private void RazePressed()
    {
        //need networked statement
        takenCity.Raze();
    }

    public void UpdateCityTakenPopUp(City city, int takerTeamNum)
    {
        takenCity = city;
        this.takerTeamNum = takerTeamNum;
    }
}

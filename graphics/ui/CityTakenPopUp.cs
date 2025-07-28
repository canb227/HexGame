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

        KeepButton = cityTakenPopUp.GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/KeepButton");
        RazeButton = cityTakenPopUp.GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/RazeButton");

        KeepButton.Pressed += () => KeepPressed();
        RazeButton.Pressed += () => RazePressed();
    }

    private void KeepPressed()
    {
        //networked statement
        Global.gameManager.CapturedCityChoice(takenCity.id, takerTeamNum, CityConquerOptions.Keep);
    }

    private void RazePressed()
    {
        //networked statement
        Global.gameManager.CapturedCityChoice(takenCity.id, takerTeamNum, CityConquerOptions.Raze);
    }
    private void FreePressed()
    {
        //networked statement
        Global.gameManager.CapturedCityChoice(takenCity.id, takerTeamNum, CityConquerOptions.Free);
    }

    public void UpdateCityTakenPopUp(City city, int takerTeamNum)
    {
        takenCity = city;
        this.takerTeamNum = takerTeamNum;
    }
}

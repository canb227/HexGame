using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.AccessControl;

public partial class EncampementTakenPopUp : Control
{
    public Control encampmentTakenPopUp;
    private Button OccupyButton;
    private Button VassalizeButton;
    private Button RazeButton;

    private Encampment takenEncampment;
    private int takerTeamNum;

    public EncampementTakenPopUp()
    {
        encampmentTakenPopUp = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/EncampmentTakenPopup.tscn").Instantiate<Control>();
        AddChild(encampmentTakenPopUp);

        OccupyButton = encampmentTakenPopUp.GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/OccupyButton");
        VassalizeButton = encampmentTakenPopUp.GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/VassalizeButton");
        RazeButton = encampmentTakenPopUp.GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/RazeButton");

        OccupyButton.Pressed += () => OccupyPressed();
        VassalizeButton.Pressed += () => VassalizePressed();
        RazeButton.Pressed += () => RazePressed();
    }

    private void OccupyPressed()
    {
        //networked statement
        Global.gameManager.CapturedEncampmentChoice(takenEncampment.id, takerTeamNum, EncampmentConquerOptions.Occupy);
        Global.gameManager.graphicManager.uiManager.CloseCurrentWindow();
        Global.gameManager.graphicManager.uiManager.ShowGenericUIAfterTargeting();
    }

    private void VassalizePressed()
    {
        //networked statement
        Global.gameManager.CapturedEncampmentChoice(takenEncampment.id, takerTeamNum, EncampmentConquerOptions.Vassalize);
        Global.gameManager.graphicManager.uiManager.CloseCurrentWindow();
        Global.gameManager.graphicManager.uiManager.ShowGenericUIAfterTargeting();
    }

    private void RazePressed()
    {
        //networked statement
        Global.gameManager.CapturedEncampmentChoice(takenEncampment.id, takerTeamNum, EncampmentConquerOptions.Raze);
        Global.gameManager.graphicManager.uiManager.CloseCurrentWindow();
        Global.gameManager.graphicManager.uiManager.ShowGenericUIAfterTargeting();
    }

    public void UpdateEncampementTakenPopUp(Encampment encampment, int takerTeamNum)
    {
        takenEncampment = encampment;
        this.takerTeamNum = takerTeamNum;
    }
}
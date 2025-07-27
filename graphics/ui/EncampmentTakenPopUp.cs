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

    private Encampment takenEncampment;
    private int takerTeamNum;

    public EncampementTakenPopUp()
    {
        encampmentTakenPopUp = Godot.ResourceLoader.Load<PackedScene>("res://graphics/ui/EncampmentTakenPopup.tscn").Instantiate<Control>();
        AddChild(encampmentTakenPopUp);

        OccupyButton = encampmentTakenPopUp.GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/OccupyButton");
        VassalizeButton = encampmentTakenPopUp.GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/VassalizeButton");

        OccupyButton.Pressed += () => OccupyPressed();
        VassalizeButton.Pressed += () => VassalizePressed();
    }

    private void OccupyPressed()
    {
        //need networked statement
        takenEncampment.EncampmentOccupied(takerTeamNum);
    }

    private void VassalizePressed()
    {
        //need networked statement
        Global.gameManager.game.teamManager.SetDiplomaticState(takerTeamNum, takenEncampment.teamNum, DiplomaticState.Ally);
    }

    public void UpdateEncampementTakenPopUp(Encampment encampment, int takerTeamNum)
    {
        takenEncampment = encampment;
        this.takerTeamNum = takerTeamNum;
    }
}
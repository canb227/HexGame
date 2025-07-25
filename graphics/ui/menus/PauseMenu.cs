using Godot;
using System;

public partial class PauseMenu : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode<Button>("PanelContainer/PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/quitButton").Pressed += onQuitButtonPressed;
        GetNode<Button>("PanelContainer/PanelContainer/MarginContainer/VBoxContainer/loadButton").Pressed += onLoadButtonPressed;
        GetNode<Button>("PanelContainer/PanelContainer/MarginContainer/VBoxContainer/saveButton").Pressed += onSaveButtonPressed;
        GetNode<Button>("PanelContainer/PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/resumeButton").Pressed += onResumeButtonPressed;
    }

	public void onQuitButtonPressed() 
	{
		Global.menuManager.SpawnPopup("res://graphics/ui/menus/PopupQuitConfirm.tscn");
	
	}


    public void onLoadButtonPressed() { }

	public void onSaveButtonPressed()
	{
		Global.gameManager.SaveGame("user://testsave.txt");
	}

	public void onResumeButtonPressed()
	{ 
		Global.menuManager.ClearMenus();
	}
}

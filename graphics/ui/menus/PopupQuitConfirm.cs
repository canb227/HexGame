using Godot;
using System;

public partial class PopupQuitConfirm : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        GetNode<Button>("quitButton").Pressed += onQuitButtonPressed;
        GetNode<Button>("backButton").Pressed += onBackButtonPressed;
    }

    private void onQuitButtonPressed()
    {
        GetTree().Quit();
    }

    private void onBackButtonPressed()
    {
        Global.menuManager.ClearPopup();
    }
}

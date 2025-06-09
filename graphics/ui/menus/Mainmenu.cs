using Godot;
using System;

public partial class Mainmenu : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void onPlayButtonPressed()
    {
		
		Global.menuManager.LoadLobby();
    }

	public void onDebugStart1Pressed()
	{
        Global.gameManager.startGame(1);
    }

	public void onOptionsButtonPressed()
    {

    }
	public void onExitButtonPressed()
    {
        GetTree().Quit();
    }
}

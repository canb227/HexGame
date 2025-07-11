using Godot;
using NetworkMessages;
using System;
using System.Linq;

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
		Global.lobby.Show();
		Global.lobby.MoveToFront();
		Global.lobby.CreateLobby();
    }


    public void onOptionsButtonPressed()
    {

    }
	public void onExitButtonPressed()
    {
        GetTree().Quit();
    }
}
